/*=========================================================
  CAMBIO DE ESQUEMA: LA VISITA COMPRA PRODUCTOS

  Antes: una factura vendia obras y se ligaba solo al cliente.
  Ahora: una factura pertenece a una visita concreta y vende
  productos, rebajando el inventario.

  Cadena que queda armada:
      Cliente -> Visita (entrada y salida) -> Factura -> Productos

  Se ejecuta una sola vez. Las tablas de venta estaban vacias,
  por eso las columnas nuevas pueden ser obligatorias.
=========================================================*/

SET XACT_ABORT ON;
GO


/*---------------------------------------------------------
  1. LA FACTURA PERTENECE A UNA VISITA
---------------------------------------------------------*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Facturas')
      AND name = 'IdVisita'
)
BEGIN
    ALTER TABLE dbo.Facturas
        ADD IdVisita INT NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Facturas_Visitas'
)
BEGIN
    ALTER TABLE dbo.Facturas
        ADD CONSTRAINT FK_Facturas_Visitas
        FOREIGN KEY (IdVisita)
        REFERENCES dbo.Visitas (IdVisita);
END;
GO


/*---------------------------------------------------------
  2. EL DETALLE PASA DE OBRAS A PRODUCTOS
---------------------------------------------------------*/

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_DetalleFactura_Obras'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        DROP CONSTRAINT FK_DetalleFactura_Obras;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DetalleFactura')
      AND name = 'IdObra'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        DROP COLUMN IdObra;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DetalleFactura')
      AND name = 'IdProducto'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        ADD IdProducto INT NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_DetalleFactura_Productos'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        ADD CONSTRAINT FK_DetalleFactura_Productos
        FOREIGN KEY (IdProducto)
        REFERENCES dbo.Productos (IdProducto);
END;
GO


/*---------------------------------------------------------
  3. LA CANTIDAD DEJA DE SER SIEMPRE UNO

  La restriccion anterior exigia Cantidad = 1 porque cada
  linea era una obra unica. Con productos se pueden llevar
  varias unidades del mismo articulo.
---------------------------------------------------------*/

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_DetalleFactura_Cantidad'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        DROP CONSTRAINT CK_DetalleFactura_Cantidad;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_DetalleFactura_Cantidad'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        ADD CONSTRAINT CK_DetalleFactura_Cantidad
        CHECK (Cantidad > 0);
END;
GO


/*---------------------------------------------------------
  4. INDICES DE APOYO
---------------------------------------------------------*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Facturas_IdVisita'
      AND object_id = OBJECT_ID('dbo.Facturas')
)
BEGIN
    CREATE INDEX IX_Facturas_IdVisita
        ON dbo.Facturas (IdVisita);
END;
GO

/* Para buscar rapido si un cliente esta dentro de la galeria */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Visitas_Cliente_Salida'
      AND object_id = OBJECT_ID('dbo.Visitas')
)
BEGIN
    CREATE INDEX IX_Visitas_Cliente_Salida
        ON dbo.Visitas (IdCliente, FechaSalida);
END;
GO
