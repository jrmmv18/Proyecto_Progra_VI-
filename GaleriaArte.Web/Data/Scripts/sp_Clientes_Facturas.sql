/*=========================================================
  MODULO: CLIENTES, FACTURAS Y OBRAS DISPONIBLES
  Procedimientos tomados del respaldo GaleriaArteDB actualizada.bak
  Necesarios para los modulos de Clientes y Ventas.
=========================================================*/



/* ============================================================
   CLIENTES - LISTAR
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCliente,
        Nombre,
        Apellido,
        Telefono,
        Correo,
        Direccion,
        Estado,
        FechaRegistro
    FROM dbo.Clientes
    ORDER BY Nombre, Apellido;
END


GO



/* ============================================================
   CLIENTES - LISTAR ACTIVOS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCliente,
        Nombre,
        Apellido,
        Telefono,
        Correo,
        Direccion,
        Estado,
        FechaRegistro
    FROM dbo.Clientes
    WHERE Estado = 1
    ORDER BY Nombre, Apellido;
END


GO



/* ============================================================
   CLIENTES - OBTENER POR ID
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_ObtenerPorId
    @IdCliente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCliente,
        Nombre,
        Apellido,
        Telefono,
        Correo,
        Direccion,
        Estado,
        FechaRegistro
    FROM dbo.Clientes
    WHERE IdCliente = @IdCliente;
END


GO



/* ============================================================
   CLIENTES - CREAR
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_Crear
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @Telefono VARCHAR(20) = NULL,
    @Correo VARCHAR(100) = NULL,
    @Direccion NVARCHAR(200) = NULL,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@Nombre)), '') IS NULL
    BEGIN
        RAISERROR('El nombre del cliente es obligatorio.', 16, 1);
        RETURN;
    END;

    IF NULLIF(LTRIM(RTRIM(@Apellido)), '') IS NULL
    BEGIN
        RAISERROR('El apellido del cliente es obligatorio.', 16, 1);
        RETURN;
    END;

    IF @Correo IS NOT NULL
       AND EXISTS
       (
           SELECT 1
           FROM dbo.Clientes
           WHERE Correo = @Correo
       )
    BEGIN
        RAISERROR('Ya existe un cliente registrado con ese correo.', 16, 1);
        RETURN;
    END;

    INSERT INTO dbo.Clientes
    (
        Nombre,
        Apellido,
        Telefono,
        Correo,
        Direccion,
        Estado,
        FechaRegistro
    )
    VALUES
    (
        LTRIM(RTRIM(@Nombre)),
        LTRIM(RTRIM(@Apellido)),
        NULLIF(LTRIM(RTRIM(@Telefono)), ''),
        NULLIF(LTRIM(RTRIM(@Correo)), ''),
        NULLIF(LTRIM(RTRIM(@Direccion)), ''),
        1,
        SYSDATETIME()
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdCliente;
END


GO



/* ============================================================
   CLIENTES - ACTUALIZAR
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Clientes_Actualizar
    @IdCliente INT,
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @Telefono VARCHAR(20) = NULL,
    @Correo VARCHAR(100) = NULL,
    @Direccion NVARCHAR(200) = NULL,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Clientes
        WHERE IdCliente = @IdCliente
    )
    BEGIN
        RAISERROR('El cliente indicado no existe.', 16, 1);
        RETURN;
    END;

    IF NULLIF(LTRIM(RTRIM(@Nombre)), '') IS NULL
    BEGIN
        RAISERROR('El nombre del cliente es obligatorio.', 16, 1);
        RETURN;
    END;

    IF NULLIF(LTRIM(RTRIM(@Apellido)), '') IS NULL
    BEGIN
        RAISERROR('El apellido del cliente es obligatorio.', 16, 1);
        RETURN;
    END;

    IF @Correo IS NOT NULL
       AND EXISTS
       (
           SELECT 1
           FROM dbo.Clientes
           WHERE Correo = @Correo
             AND IdCliente <> @IdCliente
       )
    BEGIN
        RAISERROR('Ya existe otro cliente registrado con ese correo.', 16, 1);
        RETURN;
    END;

    UPDATE dbo.Clientes
    SET
        Nombre = LTRIM(RTRIM(@Nombre)),
        Apellido = LTRIM(RTRIM(@Apellido)),
        Telefono = NULLIF(LTRIM(RTRIM(@Telefono)), ''),
        Correo = NULLIF(LTRIM(RTRIM(@Correo)), ''),
        Direccion = NULLIF(LTRIM(RTRIM(@Direccion)), '')
    WHERE IdCliente = @IdCliente;
END


GO


CREATE OR ALTER PROCEDURE dbo.sp_Clientes_CambiarEstado
    @IdCliente INT,
    @NuevoEstado BIT,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @EstadoAnterior BIT;
    DECLARE @NombreCliente NVARCHAR(120);

    SELECT
        @EstadoAnterior = Estado,
        @NombreCliente = CONCAT(Nombre, N' ', Apellido)
    FROM dbo.Clientes
    WHERE IdCliente = @IdCliente;

    IF @EstadoAnterior IS NULL
    BEGIN
        RAISERROR(
            'El cliente indicado no existe.',
            16,
            1
        );

        RETURN;
    END;

    BEGIN TRY

        BEGIN TRANSACTION;

        UPDATE dbo.Clientes
        SET Estado = @NuevoEstado
        WHERE IdCliente = @IdCliente;

        INSERT INTO dbo.BitacoraGeneral
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
            'Clientes',
            'Clientes',
            'UPDATE',
            @IdCliente,
            CONCAT(
                'Cliente: ',
                @NombreCliente,
                ' | Estado: ',
                CASE
                    WHEN @EstadoAnterior = 1 THEN 'Activo'
                    ELSE 'Inactivo'
                END
            ),
            CONCAT(
                'Cliente: ',
                @NombreCliente,
                ' | Estado: ',
                CASE
                    WHEN @NuevoEstado = 1 THEN 'Activo'
                    ELSE 'Inactivo'
                END
            ),
            SYSDATETIME(),
            'EXITOSO'
        );

        COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;

    END CATCH;
END


GO



/* ============================================================
   OBRAS DISPONIBLES PARA VENTA
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Obras_ListarDisponibles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        O.IdObra,
        O.IdArtista,
        O.IdCategoria,
        O.Codigo,
        O.Nombre,
        O.Descripcion,
        O.FechaCreacion,
        O.FechaIngresoGaleria,
        O.ValorEstimado,
        O.EstadoConservacion,
        O.Estado,

        CAST(N'' AS NVARCHAR(200)) AS NombreArtista,
        CAST(N'' AS NVARCHAR(200)) AS NombreCategoria

    FROM dbo.Obras AS O

    WHERE O.Estado = 1

      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.DetalleFactura AS DF
          INNER JOIN dbo.Facturas AS F
              ON DF.IdFactura = F.IdFactura
          INNER JOIN dbo.EstadoFactura AS EF
              ON F.IdEstadoFactura = EF.IdEstadoFactura
          WHERE DF.IdObra = O.IdObra
            AND EF.Nombre <> N'Anulada'
      )

    ORDER BY O.Nombre;
END


GO



/* ============================================================
   FACTURAS - LISTAR
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        F.IdFactura,
        F.IdCliente,
        F.IdUsuario,
        F.IdEstadoFactura,
        F.FechaFactura,
        F.MetodoPago,
        F.Subtotal,
        F.Impuesto,
        F.Total,

        CONCAT(
            C.Nombre,
            N' ',
            C.Apellido
        ) AS NombreCliente,

        CONCAT(
            U.Nombre,
            N' ',
            U.Apellido
        ) AS NombreUsuario,

        EF.Nombre AS EstadoFactura

    FROM dbo.Facturas F

    INNER JOIN dbo.Clientes C
        ON F.IdCliente = C.IdCliente

    INNER JOIN dbo.Usuarios U
        ON F.IdUsuario = U.IdUsuario

    INNER JOIN dbo.EstadoFactura EF
        ON F.IdEstadoFactura =
           EF.IdEstadoFactura

    ORDER BY
        F.FechaFactura DESC,
        F.IdFactura DESC;
END


GO



/* ============================================================
   FACTURAS - OBTENER FACTURA Y DETALLE
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Obtener
    @IdFactura INT
AS
BEGIN
    SET NOCOUNT ON;

    /* PRIMER RESULTADO: CABECERA */

    SELECT
        F.IdFactura,
        F.IdCliente,
        F.IdUsuario,
        F.IdEstadoFactura,
        F.FechaFactura,
        F.MetodoPago,
        F.Subtotal,
        F.Impuesto,
        F.Total,

        CONCAT(
            C.Nombre,
            N' ',
            C.Apellido
        ) AS NombreCliente,

        CONCAT(
            U.Nombre,
            N' ',
            U.Apellido
        ) AS NombreUsuario,

        EF.Nombre AS EstadoFactura

    FROM dbo.Facturas F

    INNER JOIN dbo.Clientes C
        ON F.IdCliente = C.IdCliente

    INNER JOIN dbo.Usuarios U
        ON F.IdUsuario = U.IdUsuario

    INNER JOIN dbo.EstadoFactura EF
        ON F.IdEstadoFactura =
           EF.IdEstadoFactura

    WHERE F.IdFactura = @IdFactura;


    /* SEGUNDO RESULTADO: DETALLE */

    SELECT
        DF.IdDetalleFactura,
        DF.IdFactura,
        DF.IdObra,
        DF.Cantidad,
        DF.PrecioUnitario,
        DF.Subtotal,

        O.Codigo AS CodigoObra,

        O.Nombre AS NombreObra,

        CAST(N'' AS NVARCHAR(200))
            AS NombreArtista

    FROM dbo.DetalleFactura DF

    INNER JOIN dbo.Obras O
        ON DF.IdObra = O.IdObra

    WHERE DF.IdFactura = @IdFactura

    ORDER BY DF.IdDetalleFactura;
END


GO


CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Crear
    @IdCliente INT,
    @IdUsuario INT,
    @MetodoPago VARCHAR(20),
    @PorcentajeImpuesto DECIMAL(5,4),
    @ObrasJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdEstadoFactura INT;
    DECLARE @IdFactura INT;
    DECLARE @Subtotal DECIMAL(18,2);
    DECLARE @Impuesto DECIMAL(18,2);
    DECLARE @Total DECIMAL(18,2);

    SELECT TOP 1
        @IdEstadoFactura = IdEstadoFactura
    FROM dbo.EstadoFactura
    WHERE Nombre = N'Emitida'
      AND Estado = 1;

    IF @IdEstadoFactura IS NULL
    BEGIN
        RAISERROR(
            'No existe el estado Emitida para las facturas.',
            16,
            1
        );
        RETURN;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Clientes
        WHERE IdCliente = @IdCliente
          AND Estado = 1
    )
    BEGIN
        RAISERROR(
            'El cliente seleccionado no existe o está inactivo.',
            16,
            1
        );
        RETURN;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE IdUsuario = @IdUsuario
          AND Estado = 1
    )
    BEGIN
        RAISERROR(
            'El usuario no existe o está inactivo.',
            16,
            1
        );
        RETURN;
    END;

    IF NULLIF(LTRIM(RTRIM(@MetodoPago)), '') IS NULL
    BEGIN
        RAISERROR(
            'Debe indicar el método de pago.',
            16,
            1
        );
        RETURN;
    END;

    IF @PorcentajeImpuesto < 0
    BEGIN
        RAISERROR(
            'El porcentaje de impuesto no puede ser negativo.',
            16,
            1
        );
        RETURN;
    END;

    IF ISJSON(@ObrasJson) <> 1
    BEGIN
        RAISERROR(
            'La información de las obras no tiene un formato válido.',
            16,
            1
        );
        RETURN;
    END;

    DECLARE @Obras TABLE
    (
        IdObra INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL
    );

    INSERT INTO @Obras
    (
        IdObra,
        PrecioUnitario
    )
    SELECT
        IdObra,
        PrecioUnitario
    FROM OPENJSON(@ObrasJson)
    WITH
    (
        IdObra INT '$.IdObra',
        PrecioUnitario DECIMAL(18,2) '$.PrecioUnitario'
    );

    IF NOT EXISTS
    (
        SELECT 1
        FROM @Obras
    )
    BEGIN
        RAISERROR(
            'Debe seleccionar al menos una obra.',
            16,
            1
        );
        RETURN;
    END;

    IF EXISTS
    (
        SELECT IdObra
        FROM @Obras
        GROUP BY IdObra
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR(
            'No se puede agregar la misma obra más de una vez.',
            16,
            1
        );
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @Obras
        WHERE PrecioUnitario <= 0
    )
    BEGIN
        RAISERROR(
            'Todas las obras deben tener un precio mayor que cero.',
            16,
            1
        );
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @Obras X
        LEFT JOIN dbo.Obras O
            ON X.IdObra = O.IdObra
        WHERE O.IdObra IS NULL
           OR O.Estado = 0
    )
    BEGIN
        RAISERROR(
            'Una de las obras seleccionadas no existe o no está disponible.',
            16,
            1
        );
        RETURN;
    END;

    SELECT
        @Subtotal = SUM(PrecioUnitario)
    FROM @Obras;

    SET @Impuesto =
        ROUND(
            @Subtotal * @PorcentajeImpuesto,
            2
        );

    SET @Total =
        @Subtotal + @Impuesto;

    BEGIN TRY

        BEGIN TRANSACTION;

        INSERT INTO dbo.Facturas
        (
            IdCliente,
            IdUsuario,
            IdEstadoFactura,
            FechaFactura,
            MetodoPago,
            Subtotal,
            Impuesto,
            Total
        )
        VALUES
        (
            @IdCliente,
            @IdUsuario,
            @IdEstadoFactura,
            SYSDATETIME(),
            @MetodoPago,
            @Subtotal,
            @Impuesto,
            @Total
        );

        SET @IdFactura =
            CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.DetalleFactura
        (
            IdFactura,
            IdObra,
            Cantidad,
            PrecioUnitario,
            Subtotal
        )
        SELECT
            @IdFactura,
            IdObra,
            1,
            PrecioUnitario,
            PrecioUnitario
        FROM @Obras;

        UPDATE O
        SET O.Estado = 0
        FROM dbo.Obras O
        INNER JOIN @Obras X
            ON O.IdObra = X.IdObra;


        /* =====================================================
           BITÁCORA DE VENTAS
           ===================================================== */

        INSERT INTO dbo.BitacoraVentas
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
            'sp_Facturas_Crear',
            'INSERT',
            @IdFactura,
            CONCAT(
                'Venta registrada. Factura #',
                @IdFactura,
                '. Cliente ID: ',
                @IdCliente,
                '. Método de pago: ',
                @MetodoPago,
                '. Subtotal: ',
                @Subtotal,
                '. Impuesto: ',
                @Impuesto,
                '. Total: ',
                @Total
            ),
            SYSDATETIME(),
            'EXITOSO'
        );


        /* =====================================================
           BITÁCORA GENERAL
           ===================================================== */

        INSERT INTO dbo.BitacoraGeneral
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
            'Ventas',
            'Facturas',
            'INSERT',
            @IdFactura,
            NULL,
            CONCAT(
                'Factura #',
                @IdFactura,
                ' | Cliente: ',
                @IdCliente,
                ' | Total: ',
                @Total,
                ' | Pago: ',
                @MetodoPago
            ),
            SYSDATETIME(),
            'EXITOSO'
        );


        COMMIT TRANSACTION;


        /* IMPORTANTE:
           FacturaRepository espera este resultado.
        */

        SELECT
            @IdFactura AS IdFactura;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;

    END CATCH;
END


GO


CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Anular
    @IdFactura INT,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdEstadoAnulada INT;
    DECLARE @EstadoAnterior NVARCHAR(30);

    SELECT TOP 1
        @IdEstadoAnulada =
            IdEstadoFactura
    FROM dbo.EstadoFactura
    WHERE Nombre = N'Anulada'
      AND Estado = 1;

    IF @IdEstadoAnulada IS NULL
    BEGIN
        RAISERROR(
            'No existe el estado Anulada.',
            16,
            1
        );
        RETURN;
    END;

    SELECT
        @EstadoAnterior = EF.Nombre
    FROM dbo.Facturas F
    INNER JOIN dbo.EstadoFactura EF
        ON F.IdEstadoFactura =
           EF.IdEstadoFactura
    WHERE F.IdFactura =
        @IdFactura;

    IF @EstadoAnterior IS NULL
    BEGIN
        RAISERROR(
            'La factura indicada no existe.',
            16,
            1
        );
        RETURN;
    END;

    IF @EstadoAnterior = N'Anulada'
    BEGIN
        RAISERROR(
            'La factura ya se encuentra anulada.',
            16,
            1
        );
        RETURN;
    END;

    BEGIN TRY

        BEGIN TRANSACTION;

        UPDATE dbo.Facturas
        SET IdEstadoFactura =
            @IdEstadoAnulada
        WHERE IdFactura =
            @IdFactura;


        /* Devolver obras a disponibles */

        UPDATE O
        SET O.Estado = 1
        FROM dbo.Obras O
        INNER JOIN dbo.DetalleFactura DF
            ON O.IdObra =
               DF.IdObra
        WHERE DF.IdFactura =
            @IdFactura;


        /* =====================================================
           BITÁCORA DE VENTAS
           ===================================================== */

        INSERT INTO dbo.BitacoraVentas
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
            'sp_Facturas_Anular',
            'UPDATE',
            @IdFactura,
            CONCAT(
                'Factura #',
                @IdFactura,
                ' anulada. Estado anterior: ',
                @EstadoAnterior,
                '. Nuevo estado: Anulada.'
            ),
            SYSDATETIME(),
            'EXITOSO'
        );


        /* =====================================================
           BITÁCORA GENERAL
           ===================================================== */

        INSERT INTO dbo.BitacoraGeneral
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
            'Ventas',
            'Facturas',
            'UPDATE',
            @IdFactura,
            @EstadoAnterior,
            'Anulada',
            SYSDATETIME(),
            'EXITOSO'
        );


        COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;

    END CATCH;
END


GO

