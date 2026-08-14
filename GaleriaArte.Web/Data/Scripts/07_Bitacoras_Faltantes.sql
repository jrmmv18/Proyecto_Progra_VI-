/*=========================================================
  BITACORAS QUE FALTABAN

  Cuatro procedimientos modificaban datos sin dejar rastro:

    sp_Obras_Crear                      creaba obras
    sp_InsertarObraAutogenerada         creaba obras
    sp_RegistrarGastoProducto           rebajaba stock
    sp_Mantenimiento_RegistrarProducto  gastaba producto

  Se les agrega el registro que ya usan los demas, cada uno
  en la bitacora de su modulo. La logica y las validaciones
  no cambian.
=========================================================*/


/*=========================================================
  PROCEDIMIENTO: CREAR OBRA
  MODULO: GALERIA
  DESCRIPCION:
  Registra una obra con codigo correlativo automatico y
  deja constancia en la bitacora de obras.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraObras.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE [dbo].[sp_Obras_Crear]
    @IdArtista INT,
    @IdCategoria INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @FechaCreacion DATE = NULL,
    @ValorEstimado DECIMAL(18,2),
    @EstadoConservacion NVARCHAR(20),
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        BEGIN TRANSACTION;

        DECLARE @CodigoTemporal VARCHAR(30);
        DECLARE @IdObraNueva INT;
        DECLARE @CodigoNuevo VARCHAR(30);

        /*---------------------------------------------
          1. Codigo provisional mientras se genera el ID
        ---------------------------------------------*/

        SET @CodigoTemporal =
            CONCAT(
                'TMP-',
                LEFT(
                    REPLACE(
                        CONVERT(VARCHAR(36), NEWID()),
                        '-',
                        ''
                    ),
                    26
                )
            );

        /*---------------------------------------------
          2. Insertar la obra
        ---------------------------------------------*/

        INSERT INTO Obras
        (
            IdArtista,
            IdCategoria,
            Codigo,
            Nombre,
            Descripcion,
            FechaCreacion,
            FechaIngresoGaleria,
            ValorEstimado,
            EstadoConservacion,
            Estado
        )
        VALUES
        (
            @IdArtista,
            @IdCategoria,
            @CodigoTemporal,
            @Nombre,
            @Descripcion,
            @FechaCreacion,
            SYSDATETIME(),
            @ValorEstimado,
            @EstadoConservacion,
            1
        );

        SET @IdObraNueva =
            CONVERT(INT, SCOPE_IDENTITY());

        /*---------------------------------------------
          3. Codigo definitivo con el ID ya generado
        ---------------------------------------------*/

        SET @CodigoNuevo =
            CONCAT(
                'OBR-',
                RIGHT(
                    CONCAT('000000', @IdObraNueva),
                    6
                )
            );

        UPDATE Obras
        SET Codigo = @CodigoNuevo
        WHERE IdObra = @IdObraNueva;

        /*---------------------------------------------
          4. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO dbo.BitacoraObras
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Obras_Crear',
            'INSERT',
            @IdObraNueva,
            CONCAT(
                N'Código: ', @CodigoNuevo,
                N' | Nombre: ', @Nombre,
                N' | IdArtista: ', @IdArtista,
                N' | IdCategoría: ', @IdCategoria,
                N' | Valor: ', @ValorEstimado,
                N' | Conservación: ', @EstadoConservacion,
                N' | Estado: Activa'),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

        SELECT
            @IdObraNueva AS IdObra,
            @CodigoNuevo AS Codigo;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;

        INSERT INTO dbo.BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Obras_Crear',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );

        THROW;

    END CATCH;
END;
GO


/*=========================================================
  PROCEDIMIENTO: INSERTAR OBRA CON CODIGO EXISTENTE
  MODULO: GALERIA
  DESCRIPCION:
  Inserta una obra conservando su identificador y codigo
  originales. Se usa al reponer obras que ya tenian numero.

  SEGURIDAD:
  - Registra operaciones exitosas en BitacoraObras.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_InsertarObraAutogenerada
    @IdArtista INT,
    @IdCategoria INT,
    @Nombre NVARCHAR(200),
    @Descripcion NVARCHAR(1000),
    @FechaCreacion DATE,
    @ValorEstimado DECIMAL(18,2),
    @EstadoConservacion NVARCHAR(40),
    @CodigoObra VARCHAR(30),
    @IdObraOriginal INT,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        /* Permite escribir el identificador a mano */
        SET IDENTITY_INSERT Obras ON;

        INSERT INTO Obras
        (
            IdObra,
            IdArtista,
            IdCategoria,
            Codigo,
            Nombre,
            Descripcion,
            FechaCreacion,
            ValorEstimado,
            EstadoConservacion,
            Estado
        )
        VALUES
        (
            @IdObraOriginal,
            @IdArtista,
            @IdCategoria,
            @CodigoObra,
            @Nombre,
            @Descripcion,
            @FechaCreacion,
            @ValorEstimado,
            @EstadoConservacion,
            1
        );

        SET IDENTITY_INSERT Obras OFF;

        INSERT INTO dbo.BitacoraObras
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_InsertarObraAutogenerada',
            'INSERT',
            @IdObraOriginal,
            CONCAT(
                N'Código: ', @CodigoObra,
                N' | Nombre: ', @Nombre,
                N' | Valor: ', @ValorEstimado,
                N' | Insertada conservando su identificador'),
            SYSDATETIME(),
            N'EXITOSO'
        );

    END TRY

    BEGIN CATCH

        /* El permiso debe quedar apagado pase lo que pase */
        IF (SELECT OBJECTPROPERTY(OBJECT_ID('Obras'), 'TableHasIdentity')) = 1
        BEGIN
            SET IDENTITY_INSERT Obras OFF;
        END;

        INSERT INTO dbo.BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_InsertarObraAutogenerada',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            0
        );

        THROW;

    END CATCH;
END;
GO


/*=========================================================
  PROCEDIMIENTO: REGISTRAR GASTO DE PRODUCTO
  MODULO: INVENTARIO
  DESCRIPCION:
  Descuenta del inventario el producto usado en una obra.

  VALIDACIONES:
  - La obra debe ser valida.
  - El producto debe existir y estar activo.
  - La cantidad debe ser mayor que cero.
  - Debe haber existencias suficientes.

  SEGURIDAD:
  - Usa transaccion.
  - Registra el movimiento en MovimientoInventario.
  - Registra operaciones exitosas en BitacoraInventario.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarGastoProducto
    @IdObra INT,
    @IdProducto INT,
    @CantidadUsada INT,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @StockAnterior INT;
    DECLARE @NombreProducto NVARCHAR(200);
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar la obra
        ---------------------------------------------*/

        IF @IdObra IS NULL OR @IdObra <= 0
        BEGIN
            THROW 50300, 'La obra indicada no es valida.', 1;
        END;


        /*---------------------------------------------
          2. Validar el producto
        ---------------------------------------------*/

        IF @IdProducto IS NULL OR @IdProducto <= 0
        BEGIN
            THROW 50301, 'El producto indicado no es valido.', 1;
        END;


        /*---------------------------------------------
          3. Validar la cantidad
        ---------------------------------------------*/

        IF @CantidadUsada IS NULL OR @CantidadUsada <= 0
        BEGIN
            THROW 50302,
                  'La cantidad utilizada debe ser mayor que cero.',
                  1;
        END;


        /*---------------------------------------------
          4. Tomar la existencia actual
        ---------------------------------------------*/

        SELECT
            @StockAnterior = Stock,
            @NombreProducto = Nombre
        FROM dbo.Productos
        WHERE IdProducto = @IdProducto
          AND Estado = 1;

        IF @StockAnterior IS NULL
        BEGIN
            THROW 50303,
                  'El producto no existe o esta inactivo.',
                  1;
        END;

        IF @StockAnterior < @CantidadUsada
        BEGIN
            THROW 50304,
                  'El producto no tiene stock suficiente.',
                  1;
        END;


        /*---------------------------------------------
          5. Rebajar el inventario
        ---------------------------------------------*/

        BEGIN TRANSACTION;

        UPDATE dbo.Productos
        SET Stock = Stock - @CantidadUsada
        WHERE IdProducto = @IdProducto;


        /*---------------------------------------------
          6. Registrar el movimiento
        ---------------------------------------------*/

        INSERT INTO dbo.MovimientoInventario
        (
            IdProducto,
            IdUsuario,
            TipoMovimiento,
            Cantidad,
            ExistenciaAnterior,
            ExistenciaNueva,
            Motivo,
            FechaMovimiento
        )
        VALUES
        (
            @IdProducto,
            @IdUsuario,
            'SALIDA',
            @CantidadUsada,
            @StockAnterior,
            @StockAnterior - @CantidadUsada,
            CONCAT(N'Gasto en la obra #', @IdObra),
            SYSDATETIME()
        );


        /*---------------------------------------------
          7. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO dbo.BitacoraInventario
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_RegistrarGastoProducto',
            'UPDATE',
            @IdProducto,
            CONCAT(
                @NombreProducto,
                N' | Usados: ', @CantidadUsada,
                N' | Stock: ', @StockAnterior,
                N' -> ', @StockAnterior - @CantidadUsada,
                N' | Obra #', @IdObra),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;

        INSERT INTO dbo.BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_RegistrarGastoProducto',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );

        THROW;

    END CATCH;
END;
GO


/*=========================================================
  PROCEDIMIENTO: REGISTRAR PRODUCTO EN UN MANTENIMIENTO
  MODULO: MANTENIMIENTO
  DESCRIPCION:
  Anota que producto se uso en un mantenimiento.

  VALIDACIONES:
  - El mantenimiento debe existir.
  - El producto debe existir.
  - La cantidad debe ser mayor que cero.
  - El producto no puede repetirse en el mismo trabajo.

  SEGURIDAD:
  - Usa transaccion.
  - Registra operaciones exitosas en BitacoraMantenimiento.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Mantenimiento_RegistrarProducto
    @IdMantenimiento INT,
    @IdProducto INT,
    @CantidadUtilizada INT,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NombreProducto NVARCHAR(200);
    DECLARE @RollbackEjecutado BIT = 0;

    BEGIN TRY

        /*---------------------------------------------
          1. Validar el mantenimiento
        ---------------------------------------------*/

        IF @IdMantenimiento IS NULL OR @IdMantenimiento <= 0
        BEGIN
            THROW 50310, 'El mantenimiento no es válido.', 1;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Mantenimiento
            WHERE IdMantenimiento = @IdMantenimiento
        )
        BEGIN
            THROW 50311, 'El mantenimiento no existe.', 1;
        END;


        /*---------------------------------------------
          2. Validar el producto
        ---------------------------------------------*/

        IF @IdProducto IS NULL OR @IdProducto <= 0
        BEGIN
            THROW 50312, 'El producto no es válido.', 1;
        END;

        SELECT @NombreProducto = Nombre
        FROM dbo.Productos
        WHERE IdProducto = @IdProducto;

        IF @NombreProducto IS NULL
        BEGIN
            THROW 50313, 'El producto no existe.', 1;
        END;


        /*---------------------------------------------
          3. Validar la cantidad
        ---------------------------------------------*/

        IF @CantidadUtilizada IS NULL OR @CantidadUtilizada <= 0
        BEGIN
            THROW 50314,
                  'La cantidad utilizada debe ser mayor que cero.',
                  1;
        END;


        /*---------------------------------------------
          4. No repetir el producto en el mismo trabajo
        ---------------------------------------------*/

        IF EXISTS
        (
            SELECT 1
            FROM dbo.DetalleMantenimiento
            WHERE IdMantenimiento = @IdMantenimiento
              AND IdProducto = @IdProducto
        )
        BEGIN
            THROW 50315,
                  'El producto ya fue registrado en este mantenimiento.',
                  1;
        END;


        /*---------------------------------------------
          5. Guardar el detalle
        ---------------------------------------------*/

        BEGIN TRANSACTION;

        INSERT INTO dbo.DetalleMantenimiento
        (
            IdMantenimiento,
            IdProducto,
            CantidadUtilizada
        )
        VALUES
        (
            @IdMantenimiento,
            @IdProducto,
            @CantidadUtilizada
        );


        /*---------------------------------------------
          6. Registrar auditoria
        ---------------------------------------------*/

        INSERT INTO dbo.BitacoraMantenimiento
        (
            IdUsuario,
            Procedimiento,
            Operacion,
            RegistroAfectado,
            Detalle,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Mantenimiento_RegistrarProducto',
            'INSERT',
            @IdMantenimiento,
            CONCAT(
                N'Producto: ', @NombreProducto,
                N' | Cantidad: ', @CantidadUtilizada,
                N' | Mantenimiento #', @IdMantenimiento),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;

        INSERT INTO dbo.BitacoraErrores
        (
            IdUsuario,
            Procedimiento,
            NumeroError,
            Descripcion,
            FechaHora,
            RollbackEjecutado
        )
        VALUES
        (
            @IdUsuario,
            N'sp_Mantenimiento_RegistrarProducto',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );

        THROW;

    END CATCH;
END;
GO
