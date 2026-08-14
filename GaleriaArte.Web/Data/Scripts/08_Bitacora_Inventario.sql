/*=========================================================
  BITACORA DE INVENTARIO: ENTRADAS Y SALIDAS

  BitacoraInventario existia desde el diseño original pero
  estaba vacia, porque el modulo de Productos registraba
  sus cambios solo en BitacoraGeneral y nunca dejaba
  constancia del movimiento de existencias.

  Ahora cada cambio de stock queda registrado:

    Crear producto      entrada por la existencia inicial
    Actualizar producto entrada o salida por la diferencia

  Se escribe en las mismas tablas que ya existian:
  MovimientoInventario y BitacoraInventario. El registro en
  BitacoraGeneral se mantiene igual que antes.
=========================================================*/

/*=========================================================
  PROCEDIMIENTO: CREAR PRODUCTO
  MÓDULO: INVENTARIO
  DESCRIPCIÓN:
  Registra un nuevo producto y genera la auditoría
  correspondiente.

  VALIDACIONES:
  - El proveedor debe existir.
  - El proveedor debe estar activo.
  - El código no puede estar duplicado.
  - El stock no puede ser negativo.
  - El stock mínimo no puede ser negativo.
  - El precio debe ser mayor que cero.

  SEGURIDAD:
  - Usa transacción.
  - Registra operaciones exitosas en BitacoraGeneral.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Productos_Crear
    @IdProveedor INT,
    @Codigo VARCHAR(30),
    @Nombre NVARCHAR(200),
    @Descripcion NVARCHAR(500) = NULL,
    @Stock INT,
    @StockMinimo INT,
    @Precio DECIMAL(18,2),
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdProducto INT;
    DECLARE @RollbackEjecutado BIT = 0;
    DECLARE @NombreProducto NVARCHAR(200);

    BEGIN TRY

        /*---------------------------------------------
          1. Validar que el proveedor exista
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM Proveedores
            WHERE IdProveedor = @IdProveedor
        )
        BEGIN
            THROW 50001,
                  'El proveedor indicado no existe.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar que el proveedor esté activo
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM Proveedores
            WHERE IdProveedor = @IdProveedor
              AND Estado = 1
        )
        BEGIN
            THROW 50002,
                  'El proveedor indicado está inactivo.',
                  1;
        END;


        /*---------------------------------------------
          3. Validar código duplicado
        ---------------------------------------------*/

        IF EXISTS
        (
            SELECT 1
            FROM Productos
            WHERE Codigo = @Codigo
        )
        BEGIN
            THROW 50003,
                  'Ya existe un producto con el código indicado.',
                  1;
        END;


        /*---------------------------------------------
          4. Validar stock
        ---------------------------------------------*/

        IF @Stock < 0
        BEGIN
            THROW 50004,
                  'El stock no puede ser negativo.',
                  1;
        END;


        /*---------------------------------------------
          5. Validar stock mínimo
        ---------------------------------------------*/

        IF @StockMinimo < 0
        BEGIN
            THROW 50005,
                  'El stock mínimo no puede ser negativo.',
                  1;
        END;


        /*---------------------------------------------
          6. Validar precio
        ---------------------------------------------*/

        IF @Precio <= 0
        BEGIN
            THROW 50006,
                  'El precio debe ser mayor que cero.',
                  1;
        END;


        /*---------------------------------------------
          7. Iniciar transacción
        ---------------------------------------------*/

        BEGIN TRANSACTION;


        /*---------------------------------------------
          8. Insertar producto
        ---------------------------------------------*/

        INSERT INTO Productos
        (
            IdProveedor,
            Codigo,
            Nombre,
            Descripcion,
            Stock,
            StockMinimo,
            Precio,
            Estado,
            FechaRegistro
        )
        VALUES
        (
            @IdProveedor,
            @Codigo,
            @Nombre,
            @Descripcion,
            @Stock,
            @StockMinimo,
            @Precio,
            1,
            SYSDATETIME()
        );


        /*---------------------------------------------
          9. Obtener ID generado
        ---------------------------------------------*/

        SET @IdProducto = SCOPE_IDENTITY();


        /*---------------------------------------------
          10. Registrar auditoría
        ---------------------------------------------*/

        INSERT INTO BitacoraGeneral
        (
            IdUsuario,
            Modulo,
            TablaAfectada,
            TipoOperacion,
            RegistroAfectado,
            ValorAnterior,
            ValorNuevo,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'Inventario',
            N'Productos',
            'INSERT',
            @IdProducto,
            NULL,
            CONCAT(
                N'Código: ', @Codigo,
                N' | Nombre: ', @Nombre,
                N' | IdProveedor: ', @IdProveedor,
                N' | Stock: ', @Stock,
                N' | Stock mínimo: ', @StockMinimo,
                N' | Precio: ', @Precio,
                N' | Estado: Activo'
            ),
            SYSDATETIME(),
            N'EXITOSO'
        );


        /*---------------------------------------------
          11. Confirmar transacción
        ---------------------------------------------*/

        /* =====================================================
           REGISTRAR LA ENTRADA AL INVENTARIO
           El producto nace con existencias, asi que esa
           cantidad es una entrada y debe verse en la
           bitacora de inventario.
           ===================================================== */

        IF @Stock > 0
        BEGIN
            INSERT INTO MovimientoInventario
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
                'ENTRADA',
                @Stock,
                0,
                @Stock,
                N'Existencia inicial del producto',
                SYSDATETIME()
            );

            INSERT INTO BitacoraInventario
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
                N'sp_Productos_Crear',
                'INSERT',
                @IdProducto,
                CONCAT(
                    @Nombre,
                    N' | Entrada inicial: ', @Stock,
                    N' | Stock: 0 -> ', @Stock),
                SYSDATETIME(),
                N'EXITOSO'
            );
        END;


        COMMIT TRANSACTION;


        /*---------------------------------------------
          12. Devolver producto creado
        ---------------------------------------------*/

        SELECT @IdProducto AS IdProducto;

    END TRY

    BEGIN CATCH

        /*---------------------------------------------
          13. Revertir si existe transacción activa
        ---------------------------------------------*/

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;


        /*---------------------------------------------
          14. Registrar el error
        ---------------------------------------------*/

        INSERT INTO BitacoraErrores
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
            N'sp_Productos_Crear',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );


        /*---------------------------------------------
          15. Devolver el error a la aplicación
        ---------------------------------------------*/

        THROW;

    END CATCH;
END;

GO


CREATE OR ALTER PROCEDURE sp_Productos_Actualizar
    @IdProducto INT,
    @IdProveedor INT,
    @Codigo VARCHAR(30),
    @Nombre NVARCHAR(200),
    @Descripcion NVARCHAR(500) = NULL,
    @Stock INT,
    @StockMinimo INT,
    @Precio DECIMAL(18,2),
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ValorAnterior NVARCHAR(MAX);
    DECLARE @ValorNuevo NVARCHAR(MAX);
    DECLARE @RollbackEjecutado BIT = 0;
    DECLARE @StockAnterior INT;
    DECLARE @Diferencia INT;

    BEGIN TRY

        /* =====================================================
           1. VALIDAR QUE EL PRODUCTO EXISTA
           ===================================================== */

        IF NOT EXISTS
        (
            SELECT 1
            FROM Productos
            WHERE IdProducto = @IdProducto
        )
        BEGIN
            THROW 50004,
                  'El producto indicado no existe.',
                  1;
        END;


        /* =====================================================
           2. VALIDAR QUE EL PROVEEDOR EXISTA Y ESTÉ ACTIVO
           ===================================================== */

        IF NOT EXISTS
        (
            SELECT 1
            FROM Proveedores
            WHERE IdProveedor = @IdProveedor
              AND Estado = 1
        )
        BEGIN
            THROW 50005,
                  'El proveedor indicado no existe o se encuentra inactivo.',
                  1;
        END;


        /* =====================================================
           3. VALIDAR CÓDIGO DUPLICADO
           Permite conservar el código del producto actual,
           pero impide usar el código de otro producto.
           ===================================================== */

        IF EXISTS
        (
            SELECT 1
            FROM Productos
            WHERE Codigo = @Codigo
              AND IdProducto <> @IdProducto
        )
        BEGIN
            THROW 50006,
                  'Ya existe otro producto con el código indicado.',
                  1;
        END;


        /* =====================================================
           4. GUARDAR VALORES ANTERIORES PARA AUDITORÍA
           ===================================================== */

        SELECT
            @ValorAnterior = CONCAT(
                N'Proveedor: ', IdProveedor,
                N' | Código: ', Codigo,
                N' | Nombre: ', Nombre,
                N' | Descripción: ', ISNULL(Descripcion, N''),
                N' | Stock: ', Stock,
                N' | Stock mínimo: ', StockMinimo,
                N' | Precio: ', Precio,
                N' | Estado: ',
                    CASE
                        WHEN Estado = 1 THEN N'Activo'
                        ELSE N'Inactivo'
                    END
            )
        FROM Productos
        WHERE IdProducto = @IdProducto;

        /* Existencia antes del cambio, para saber si entra o sale */
        SELECT @StockAnterior = Stock
        FROM Productos
        WHERE IdProducto = @IdProducto;


        /* =====================================================
           5. INICIAR TRANSACCIÓN
           ===================================================== */

        BEGIN TRANSACTION;


        /* =====================================================
           6. ACTUALIZAR PRODUCTO
           No modificamos Estado ni FechaRegistro.
           ===================================================== */

        UPDATE Productos
        SET
            IdProveedor = @IdProveedor,
            Codigo = @Codigo,
            Nombre = @Nombre,
            Descripcion = @Descripcion,
            Stock = @Stock,
            StockMinimo = @StockMinimo,
            Precio = @Precio
        WHERE IdProducto = @IdProducto;


        /* =====================================================
           7. PREPARAR VALORES NUEVOS PARA AUDITORÍA
           ===================================================== */

        SELECT
            @ValorNuevo = CONCAT(
                N'Proveedor: ', IdProveedor,
                N' | Código: ', Codigo,
                N' | Nombre: ', Nombre,
                N' | Descripción: ', ISNULL(Descripcion, N''),
                N' | Stock: ', Stock,
                N' | Stock mínimo: ', StockMinimo,
                N' | Precio: ', Precio,
                N' | Estado: ',
                    CASE
                        WHEN Estado = 1 THEN N'Activo'
                        ELSE N'Inactivo'
                    END
            )
        FROM Productos
        WHERE IdProducto = @IdProducto;


        /* =====================================================
           8. REGISTRAR AUDITORÍA
           ===================================================== */

        INSERT INTO BitacoraGeneral
        (
            IdUsuario,
            Modulo,
            TablaAfectada,
            TipoOperacion,
            RegistroAfectado,
            ValorAnterior,
            ValorNuevo,
            FechaHora,
            Resultado
        )
        VALUES
        (
            @IdUsuario,
            N'Inventario',
            N'Productos',
            'UPDATE',
            @IdProducto,
            @ValorAnterior,
            @ValorNuevo,
            SYSDATETIME(),
            N'EXITOSO'
        );


        /* =====================================================
           9. CONFIRMAR TRANSACCIÓN
           ===================================================== */


        /* =====================================================
           REGISTRAR EL MOVIMIENTO DE INVENTARIO
           Si la existencia cambio, queda constancia de si el
           producto entro o salio, con el antes y el despues.
           ===================================================== */

        SET @Diferencia = @Stock - @StockAnterior;

        IF @Diferencia <> 0
        BEGIN
            INSERT INTO MovimientoInventario
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
                CASE WHEN @Diferencia > 0 THEN 'ENTRADA' ELSE 'SALIDA' END,
                ABS(@Diferencia),
                @StockAnterior,
                @Stock,
                N'Ajuste desde el módulo de productos',
                SYSDATETIME()
            );

            INSERT INTO BitacoraInventario
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
                N'sp_Productos_Actualizar',
                'UPDATE',
                @IdProducto,
                CONCAT(
                    @Nombre,
                    CASE
                        WHEN @Diferencia > 0 THEN N' | Entraron: '
                        ELSE N' | Salieron: '
                    END,
                    ABS(@Diferencia),
                    N' | Stock: ', @StockAnterior,
                    N' -> ', @Stock),
                SYSDATETIME(),
                N'EXITOSO'
            );
        END;


        COMMIT TRANSACTION;


        /* =====================================================
           10. DEVOLVER RESULTADO
           ===================================================== */

        SELECT CAST(1 AS BIT) AS Actualizado;

    END TRY

    BEGIN CATCH

        /* =====================================================
           11. ROLLBACK SI EXISTE TRANSACCIÓN ACTIVA
           ===================================================== */

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @RollbackEjecutado = 1;
        END;


        /* =====================================================
           12. REGISTRAR ERROR
           ===================================================== */

        INSERT INTO BitacoraErrores
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
            N'sp_Productos_Actualizar',
            ERROR_NUMBER(),
            ERROR_MESSAGE(),
            SYSDATETIME(),
            @RollbackEjecutado
        );

        THROW;

    END CATCH;

END;
GO
