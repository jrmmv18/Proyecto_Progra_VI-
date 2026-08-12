/*=========================================================
  MODULO DE VENTAS: VUELVE A VENDER OBRAS

  Se reemplaza el modulo de ventas por el del desarrollo
  final, que factura obras de arte y no productos. Este
  script deja la base como ese modulo la espera.

  Que hace:
  1. Borra las ventas de prueba y devuelve el inventario
     que habian descontado.
  2. Quita IdVisita de Facturas.
  3. DetalleFactura vuelve a apuntar a Obras.
  4. Restaura los procedimientos de facturacion que
     trabajan con obras.

  Que NO toca:
  - El modulo de Visitas.
  - Las bitacoras de Clientes.
  - Ningun otro modulo.
=========================================================*/

SET XACT_ABORT ON;
GO


/*---------------------------------------------------------
  1. DEVOLVER EL INVENTARIO Y BORRAR LAS VENTAS DE PRUEBA

  Las facturas existentes se hicieron con el modulo de
  productos, que ya no aplica. Antes de borrarlas se
  reponen las unidades que habian descontado.
---------------------------------------------------------*/

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DetalleFactura')
      AND name = 'IdProducto'
)
BEGIN
    /* Reponer el saldo neto de cada producto */
    UPDATE P
    SET P.Stock = P.Stock + M.Saldo
    FROM dbo.Productos P
    INNER JOIN
    (
        SELECT
            IdProducto,
            SUM(CASE
                    WHEN TipoMovimiento = 'SALIDA' THEN Cantidad
                    ELSE -Cantidad
                END) AS Saldo
        FROM dbo.MovimientoInventario
        GROUP BY IdProducto
    ) AS M
        ON M.IdProducto = P.IdProducto;

    DELETE FROM dbo.MovimientoInventario;
    DELETE FROM dbo.DetalleFactura;
    DELETE FROM dbo.Facturas;

    /* Las bitacoras de esas ventas dejan de tener sentido */
    DELETE FROM dbo.BitacoraVentas;
    DELETE FROM dbo.BitacoraInventario;
END;
GO


/*---------------------------------------------------------
  2. LA FACTURA DEJA DE PERTENECER A UNA VISITA
---------------------------------------------------------*/

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Facturas_Visitas'
)
BEGIN
    ALTER TABLE dbo.Facturas
        DROP CONSTRAINT FK_Facturas_Visitas;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Facturas_IdVisita'
      AND object_id = OBJECT_ID('dbo.Facturas')
)
BEGIN
    DROP INDEX IX_Facturas_IdVisita ON dbo.Facturas;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Facturas')
      AND name = 'IdVisita'
)
BEGIN
    ALTER TABLE dbo.Facturas
        DROP COLUMN IdVisita;
END;
GO


/*---------------------------------------------------------
  3. EL DETALLE VUELVE A SER DE OBRAS
---------------------------------------------------------*/

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_DetalleFactura_Productos'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        DROP CONSTRAINT FK_DetalleFactura_Productos;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DetalleFactura')
      AND name = 'IdProducto'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        DROP COLUMN IdProducto;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DetalleFactura')
      AND name = 'IdObra'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        ADD IdObra INT NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_DetalleFactura_Obras'
)
BEGIN
    ALTER TABLE dbo.DetalleFactura
        ADD CONSTRAINT FK_DetalleFactura_Obras
        FOREIGN KEY (IdObra)
        REFERENCES dbo.Obras (IdObra);
END;
GO


/*---------------------------------------------------------
  4. CADA LINEA VUELVE A SER UNA OBRA UNICA

  El modulo de obras vende piezas unicas, por eso la
  cantidad siempre es uno.
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

ALTER TABLE dbo.DetalleFactura
    ADD CONSTRAINT CK_DetalleFactura_Cantidad
    CHECK (Cantidad = 1);
GO


/*---------------------------------------------------------
  5. EL INDICE DE VISITAS SE MANTIENE

  IX_Visitas_Cliente_Salida se creo para el modulo de
  visitas y sigue siendo util, asi que no se elimina.
---------------------------------------------------------*/
