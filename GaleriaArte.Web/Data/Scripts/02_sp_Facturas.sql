/*=========================================================
  MODULO: VENTAS
  La visita compra productos y el inventario se rebaja.

  Regla principal:
  Para facturar, el cliente debe estar dentro de la galeria,
  es decir tener una visita con entrada registrada y sin
  salida. Esa visita se asocia sola a la factura.

  Bitacoras:
  - BitacoraVentas      : alta y anulacion de la factura
  - BitacoraInventario  : cada rebaja o devolucion de stock
  - MovimientoInventario: existencia anterior y nueva
  - BitacoraErrores     : cualquier fallo
=========================================================*/


/*=========================================================
  PROCEDIMIENTO: CREAR FACTURA
  MODULO: VENTAS
  DESCRIPCION:
  Registra la compra hecha durante una visita y descuenta
  del inventario los productos vendidos.

  VALIDACIONES:
  - El cliente debe existir y estar activo.
  - El cliente debe tener una visita en curso.
  - El usuario debe existir y estar activo.
  - Debe indicarse el metodo de pago.
  - Debe venderse al menos un producto.
  - Cada producto debe existir, estar activo y tener stock
    suficiente.

  SEGURIDAD:
  - Usa transaccion.
  - Registra en BitacoraVentas y BitacoraInventario.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Crear
    @IdCliente INT,
    @IdUsuario INT,
    @MetodoPago VARCHAR(20),
    @PorcentajeImpuesto DECIMAL(5,4),
    @ProductosJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdEstadoFactura INT;
    DECLARE @IdFactura INT;
    DECLARE @IdVisita INT;
    DECLARE @Subtotal DECIMAL(18,2);
    DECLARE @Impuesto DECIMAL(18,2);
    DECLARE @Total DECIMAL(18,2);
    DECLARE @RollbackEjecutado BIT = 0;

    DECLARE @Productos TABLE
    (
        IdProducto INT,
        Cantidad INT,
        PrecioUnitario DECIMAL(18,2),
        StockAnterior INT
    );

    BEGIN TRY

        /*---------------------------------------------
          1. Estado inicial de la factura
        ---------------------------------------------*/

        SELECT TOP 1
            @IdEstadoFactura = IdEstadoFactura
        FROM dbo.EstadoFactura
        WHERE Nombre = N'Emitida'
          AND Estado = 1;

        IF @IdEstadoFactura IS NULL
        BEGIN
            THROW 50200,
                  'No existe el estado Emitida para las facturas.',
                  1;
        END;


        /*---------------------------------------------
          2. Validar cliente
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Clientes
            WHERE IdCliente = @IdCliente
              AND Estado = 1
        )
        BEGIN
            THROW 50201,
                  'El cliente seleccionado no existe o está inactivo.',
                  1;
        END;


        /*---------------------------------------------
          3. El cliente debe estar dentro de la galeria
        ---------------------------------------------*/

        SELECT TOP 1
            @IdVisita = IdVisita
        FROM dbo.Visitas
        WHERE IdCliente = @IdCliente
          AND FechaSalida IS NULL
        ORDER BY FechaIngreso DESC;

        IF @IdVisita IS NULL
        BEGIN
            THROW 50202,
                  'El cliente no tiene una visita en curso. Registre su entrada antes de facturar.',
                  1;
        END;


        /*---------------------------------------------
          4. Validar usuario
        ---------------------------------------------*/

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Usuarios
            WHERE IdUsuario = @IdUsuario
              AND Estado = 1
        )
        BEGIN
            THROW 50203,
                  'El usuario no existe o está inactivo.',
                  1;
        END;


        /*---------------------------------------------
          5. Validar metodo de pago
        ---------------------------------------------*/

        IF NULLIF(LTRIM(RTRIM(@MetodoPago)), '') IS NULL
        BEGIN
            THROW 50204,
                  'Debe indicar el método de pago.',
                  1;
        END;


        /*---------------------------------------------
          6. Leer los productos solicitados
        ---------------------------------------------*/

        INSERT INTO @Productos
        (
            IdProducto,
            Cantidad,
            PrecioUnitario,
            StockAnterior
        )
        SELECT
            J.IdProducto,
            SUM(J.Cantidad),
            0,
            0
        FROM OPENJSON(@ProductosJson)
        WITH
        (
            IdProducto INT '$.IdProducto',
            Cantidad INT '$.Cantidad'
        ) AS J
        GROUP BY J.IdProducto;

        IF NOT EXISTS (SELECT 1 FROM @Productos)
        BEGIN
            THROW 50205,
                  'Debe seleccionar al menos un producto.',
                  1;
        END;

        IF EXISTS (SELECT 1 FROM @Productos WHERE Cantidad <= 0)
        BEGIN
            THROW 50206,
                  'La cantidad de cada producto debe ser mayor que cero.',
                  1;
        END;


        /*---------------------------------------------
          7. Tomar precio y existencia actual
        ---------------------------------------------*/

        UPDATE P
        SET P.PrecioUnitario = PR.Precio,
            P.StockAnterior = PR.Stock
        FROM @Productos P
        INNER JOIN dbo.Productos PR
            ON PR.IdProducto = P.IdProducto
           AND PR.Estado = 1;

        DECLARE @Inexistente NVARCHAR(200);

        SELECT TOP 1
            @Inexistente = CAST(IdProducto AS NVARCHAR(20))
        FROM @Productos
        WHERE PrecioUnitario = 0;

        IF @Inexistente IS NOT NULL
        BEGIN
            THROW 50207,
                  'Alguno de los productos no existe o está inactivo.',
                  1;
        END;


        /*---------------------------------------------
          8. Validar existencias
        ---------------------------------------------*/

        DECLARE @SinStock NVARCHAR(300);

        SELECT TOP 1
            @SinStock = CONCAT(
                PR.Nombre,
                N' (disponibles: ', P.StockAnterior,
                N', solicitados: ', P.Cantidad, N')')
        FROM @Productos P
        INNER JOIN dbo.Productos PR
            ON PR.IdProducto = P.IdProducto
        WHERE P.Cantidad > P.StockAnterior;

        IF @SinStock IS NOT NULL
        BEGIN
            DECLARE @MensajeStock NVARCHAR(400) =
                CONCAT(N'No hay stock suficiente de ', @SinStock, N'.');

            THROW 50208, @MensajeStock, 1;
        END;


        /*---------------------------------------------
          9. Calcular importes
        ---------------------------------------------*/

        SELECT
            @Subtotal = SUM(PrecioUnitario * Cantidad)
        FROM @Productos;

        SET @Impuesto =
            ROUND(@Subtotal * @PorcentajeImpuesto, 2);

        SET @Total = @Subtotal + @Impuesto;


        /*---------------------------------------------
          10. Guardar la factura
        ---------------------------------------------*/

        BEGIN TRANSACTION;

        INSERT INTO dbo.Facturas
        (
            IdCliente,
            IdVisita,
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
            @IdVisita,
            @IdUsuario,
            @IdEstadoFactura,
            SYSDATETIME(),
            @MetodoPago,
            @Subtotal,
            @Impuesto,
            @Total
        );

        SET @IdFactura = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.DetalleFactura
        (
            IdFactura,
            IdProducto,
            Cantidad,
            PrecioUnitario,
            Subtotal
        )
        SELECT
            @IdFactura,
            IdProducto,
            Cantidad,
            PrecioUnitario,
            PrecioUnitario * Cantidad
        FROM @Productos;


        /*---------------------------------------------
          11. Rebajar el inventario
        ---------------------------------------------*/

        UPDATE PR
        SET PR.Stock = PR.Stock - P.Cantidad
        FROM dbo.Productos PR
        INNER JOIN @Productos P
            ON PR.IdProducto = P.IdProducto;


        /*---------------------------------------------
          12. Registrar el movimiento de inventario
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
        SELECT
            P.IdProducto,
            @IdUsuario,
            'SALIDA',
            P.Cantidad,
            P.StockAnterior,
            P.StockAnterior - P.Cantidad,
            CONCAT(N'Venta en factura #', @IdFactura,
                   N', visita #', @IdVisita),
            SYSDATETIME()
        FROM @Productos P;


        /*---------------------------------------------
          13. Bitacora de inventario
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
        SELECT
            @IdUsuario,
            N'sp_Facturas_Crear',
            'UPDATE',
            P.IdProducto,
            CONCAT(
                PR.Nombre,
                N' | Vendidos: ', P.Cantidad,
                N' | Stock: ', P.StockAnterior,
                N' -> ', P.StockAnterior - P.Cantidad,
                N' | Factura #', @IdFactura),
            SYSDATETIME(),
            N'EXITOSO'
        FROM @Productos P
        INNER JOIN dbo.Productos PR
            ON PR.IdProducto = P.IdProducto;


        /*---------------------------------------------
          14. Bitacora de ventas
        ---------------------------------------------*/

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
            N'sp_Facturas_Crear',
            'INSERT',
            @IdFactura,
            CONCAT(
                N'Venta registrada. Factura #', @IdFactura,
                N' | Cliente: ', @IdCliente,
                N' | Visita: ', @IdVisita,
                N' | Productos: ', (SELECT COUNT(*) FROM @Productos),
                N' | Pago: ', @MetodoPago,
                N' | Total: ', @Total),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

        SELECT @IdFactura AS IdFactura;

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
            N'sp_Facturas_Crear',
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
  PROCEDIMIENTO: ANULAR FACTURA
  MODULO: VENTAS
  DESCRIPCION:
  Anula una factura emitida y devuelve al inventario los
  productos que se habian vendido.

  VALIDACIONES:
  - La factura debe existir.
  - No puede anularse dos veces.

  SEGURIDAD:
  - Usa transaccion.
  - Registra en BitacoraVentas y BitacoraInventario.
  - Registra errores en BitacoraErrores.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Anular
    @IdFactura INT,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdAnulada INT;
    DECLARE @IdEstadoActual INT;
    DECLARE @RollbackEjecutado BIT = 0;

    DECLARE @Devueltos TABLE
    (
        IdProducto INT,
        Cantidad INT,
        StockAnterior INT
    );

    BEGIN TRY

        /*---------------------------------------------
          1. Validar que la factura exista
        ---------------------------------------------*/

        SELECT
            @IdEstadoActual = IdEstadoFactura
        FROM dbo.Facturas
        WHERE IdFactura = @IdFactura;

        IF @IdEstadoActual IS NULL
        BEGIN
            THROW 50210,
                  'La factura indicada no existe.',
                  1;
        END;


        /*---------------------------------------------
          2. Estado Anulada
        ---------------------------------------------*/

        SELECT TOP 1
            @IdAnulada = IdEstadoFactura
        FROM dbo.EstadoFactura
        WHERE Nombre = N'Anulada';

        IF @IdAnulada IS NULL
        BEGIN
            THROW 50211,
                  'No existe el estado Anulada para las facturas.',
                  1;
        END;

        IF @IdEstadoActual = @IdAnulada
        BEGIN
            THROW 50212,
                  'La factura ya estaba anulada.',
                  1;
        END;


        /*---------------------------------------------
          3. Tomar lo vendido para devolverlo
        ---------------------------------------------*/

        INSERT INTO @Devueltos
        (
            IdProducto,
            Cantidad,
            StockAnterior
        )
        SELECT
            D.IdProducto,
            D.Cantidad,
            PR.Stock
        FROM dbo.DetalleFactura D
        INNER JOIN dbo.Productos PR
            ON PR.IdProducto = D.IdProducto
        WHERE D.IdFactura = @IdFactura;


        BEGIN TRANSACTION;


        /*---------------------------------------------
          4. Marcar la factura como anulada
        ---------------------------------------------*/

        UPDATE dbo.Facturas
        SET IdEstadoFactura = @IdAnulada
        WHERE IdFactura = @IdFactura;


        /*---------------------------------------------
          5. Devolver el stock
        ---------------------------------------------*/

        UPDATE PR
        SET PR.Stock = PR.Stock + D.Cantidad
        FROM dbo.Productos PR
        INNER JOIN @Devueltos D
            ON PR.IdProducto = D.IdProducto;


        /*---------------------------------------------
          6. Movimiento de inventario de entrada
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
        SELECT
            D.IdProducto,
            @IdUsuario,
            'ENTRADA',
            D.Cantidad,
            D.StockAnterior,
            D.StockAnterior + D.Cantidad,
            CONCAT(N'Devolución por anulación de la factura #',
                   @IdFactura),
            SYSDATETIME()
        FROM @Devueltos D;


        /*---------------------------------------------
          7. Bitacora de inventario
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
        SELECT
            @IdUsuario,
            N'sp_Facturas_Anular',
            'UPDATE',
            D.IdProducto,
            CONCAT(
                PR.Nombre,
                N' | Devueltos: ', D.Cantidad,
                N' | Stock: ', D.StockAnterior,
                N' -> ', D.StockAnterior + D.Cantidad,
                N' | Factura anulada #', @IdFactura),
            SYSDATETIME(),
            N'EXITOSO'
        FROM @Devueltos D
        INNER JOIN dbo.Productos PR
            ON PR.IdProducto = D.IdProducto;


        /*---------------------------------------------
          8. Bitacora de ventas
        ---------------------------------------------*/

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
            N'sp_Facturas_Anular',
            'UPDATE',
            @IdFactura,
            CONCAT(
                N'Factura anulada #', @IdFactura,
                N' | Productos devueltos: ',
                (SELECT COUNT(*) FROM @Devueltos)),
            SYSDATETIME(),
            N'EXITOSO'
        );

        COMMIT TRANSACTION;

        SELECT CAST(1 AS BIT) AS Resultado;

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
            N'sp_Facturas_Anular',
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
  PROCEDIMIENTO: LISTAR FACTURAS
  MODULO: VENTAS
  DESCRIPCION:
  Historial de ventas con el visitante y la visita en que
  se hizo cada compra. No modifica datos.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        F.IdFactura,
        F.IdCliente,
        F.IdVisita,
        F.IdUsuario,
        F.IdEstadoFactura,
        F.FechaFactura,
        F.MetodoPago,
        F.Subtotal,
        F.Impuesto,
        F.Total,
        C.Nombre + ' ' + C.Apellido AS NombreCliente,
        U.Nombre + ' ' + U.Apellido AS NombreUsuario,
        EF.Nombre AS EstadoFactura,
        V.FechaIngreso AS FechaIngresoVisita,
        V.FechaSalida AS FechaSalidaVisita
    FROM dbo.Facturas AS F
    INNER JOIN dbo.Clientes AS C
        ON C.IdCliente = F.IdCliente
    INNER JOIN dbo.Usuarios AS U
        ON U.IdUsuario = F.IdUsuario
    INNER JOIN dbo.EstadoFactura AS EF
        ON EF.IdEstadoFactura = F.IdEstadoFactura
    INNER JOIN dbo.Visitas AS V
        ON V.IdVisita = F.IdVisita
    ORDER BY F.FechaFactura DESC;
END;
GO


/*=========================================================
  PROCEDIMIENTO: OBTENER FACTURA
  MODULO: VENTAS
  DESCRIPCION:
  Devuelve dos resultados: la factura con los datos de la
  visita, y el detalle de productos vendidos.
  No modifica datos.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Facturas_Obtener
    @IdFactura INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        F.IdFactura,
        F.IdCliente,
        F.IdVisita,
        F.IdUsuario,
        F.IdEstadoFactura,
        F.FechaFactura,
        F.MetodoPago,
        F.Subtotal,
        F.Impuesto,
        F.Total,
        C.Nombre + ' ' + C.Apellido AS NombreCliente,
        C.Correo AS CorreoCliente,
        C.Telefono AS TelefonoCliente,
        U.Nombre + ' ' + U.Apellido AS NombreUsuario,
        EF.Nombre AS EstadoFactura,
        V.FechaIngreso AS FechaIngresoVisita,
        V.FechaSalida AS FechaSalidaVisita
    FROM dbo.Facturas AS F
    INNER JOIN dbo.Clientes AS C
        ON C.IdCliente = F.IdCliente
    INNER JOIN dbo.Usuarios AS U
        ON U.IdUsuario = F.IdUsuario
    INNER JOIN dbo.EstadoFactura AS EF
        ON EF.IdEstadoFactura = F.IdEstadoFactura
    INNER JOIN dbo.Visitas AS V
        ON V.IdVisita = F.IdVisita
    WHERE F.IdFactura = @IdFactura;

    SELECT
        D.IdDetalleFactura,
        D.IdFactura,
        D.IdProducto,
        D.Cantidad,
        D.PrecioUnitario,
        D.Subtotal,
        PR.Codigo AS CodigoProducto,
        PR.Nombre AS NombreProducto
    FROM dbo.DetalleFactura AS D
    INNER JOIN dbo.Productos AS PR
        ON PR.IdProducto = D.IdProducto
    WHERE D.IdFactura = @IdFactura
    ORDER BY D.IdDetalleFactura;
END;
GO


/*=========================================================
  PROCEDIMIENTO: PRODUCTOS DISPONIBLES PARA VENDER
  MODULO: VENTAS
  DESCRIPCION:
  Productos activos con existencias. No modifica datos.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Productos_ListarDisponibles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,
        P.Descripcion,
        P.Stock,
        P.StockMinimo,
        P.Precio,
        PR.Nombre AS NombreProveedor
    FROM dbo.Productos AS P
    INNER JOIN dbo.Proveedores AS PR
        ON PR.IdProveedor = P.IdProveedor
    WHERE P.Estado = 1
      AND P.Stock > 0
    ORDER BY P.Nombre;
END;
GO


/*=========================================================
  PROCEDIMIENTO: VISITA EN CURSO DE UN CLIENTE
  MODULO: VISITAS
  DESCRIPCION:
  Indica si el cliente esta dentro de la galeria, para saber
  si puede comprar. No modifica datos.
=========================================================*/

CREATE OR ALTER PROCEDURE dbo.sp_Visitas_ObtenerEnCursoPorCliente
    @IdCliente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        V.IdVisita,
        V.IdCliente,
        V.FechaIngreso,
        V.FechaSalida,
        V.Observaciones,
        C.Nombre AS NombreCliente,
        C.Apellido AS ApellidoCliente,
        C.Correo AS CorreoCliente,
        C.Telefono AS TelefonoCliente
    FROM dbo.Visitas AS V
    INNER JOIN dbo.Clientes AS C
        ON C.IdCliente = V.IdCliente
    WHERE V.IdCliente = @IdCliente
      AND V.FechaSalida IS NULL
    ORDER BY V.FechaIngreso DESC;
END;
GO
