/*=========================================================
  DATOS BASE: ESTADOS DE FACTURA

  La tabla EstadoFactura estaba vacia, por lo que ninguna
  venta podia registrarse: sp_Facturas_Crear busca el estado
  Emitida y sp_Facturas_Anular busca el estado Anulada.

  Se insertan solo si no existen, asi el script puede
  ejecutarse varias veces sin duplicar.
=========================================================*/

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.EstadoFactura
    WHERE Nombre = N'Emitida'
)
BEGIN
    INSERT INTO dbo.EstadoFactura
    (
        Nombre,
        Descripcion,
        Estado,
        FechaRegistro
    )
    VALUES
    (
        N'Emitida',
        N'Factura registrada y vigente.',
        1,
        SYSDATETIME()
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.EstadoFactura
    WHERE Nombre = N'Anulada'
)
BEGIN
    INSERT INTO dbo.EstadoFactura
    (
        Nombre,
        Descripcion,
        Estado,
        FechaRegistro
    )
    VALUES
    (
        N'Anulada',
        N'Factura anulada. Los productos regresaron al inventario.',
        1,
        SYSDATETIME()
    );
END;
GO

SELECT
    IdEstadoFactura,
    Nombre,
    Estado
FROM dbo.EstadoFactura
ORDER BY IdEstadoFactura;
GO
