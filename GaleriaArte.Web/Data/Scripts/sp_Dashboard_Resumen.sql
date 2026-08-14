/*=========================================================
  PROCEDIMIENTO: RESUMEN DEL PANEL DE GESTION
  MODULO: GENERAL
  DESCRIPCION:
  Devuelve los totales que muestran las tarjetas del panel
  principal. No modifica datos.

  Ventas y Mantenimientos venian con el valor cero fijo, asi
  que sus tarjetas nunca mostraban nada aunque hubiera
  registros. Ahora se cuentan de verdad.

  Ventas cuenta solo las facturas vigentes: las anuladas no
  suman, igual que una obra inactiva no suma en su tarjeta.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Dashboard_Resumen
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdAnulada INT =
    (
        SELECT TOP 1 IdEstadoFactura
        FROM dbo.EstadoFactura
        WHERE Nombre = N'Anulada'
    );

    SELECT

        (SELECT COUNT(*)
         FROM Obras
         WHERE Estado = 1) AS TotalObras,

        (SELECT COUNT(*)
         FROM Artistas
         WHERE Estado = 1) AS TotalArtistas,

        (SELECT COUNT(*)
         FROM Productos
         WHERE Estado = 1) AS TotalProductos,

        (SELECT COUNT(*)
         FROM Proveedores
         WHERE Estado = 1) AS TotalProveedores,

        -- Facturas vigentes, sin contar las anuladas
        (SELECT COUNT(*)
         FROM dbo.Facturas
         WHERE @IdAnulada IS NULL
            OR IdEstadoFactura <> @IdAnulada) AS TotalVentas,

        (SELECT COUNT(*)
         FROM dbo.Mantenimiento) AS TotalMantenimientos,

        -- Visitas registradas en total
        (SELECT COUNT(*)
         FROM Visitas) AS TotalVisitas,

        -- Visitantes que aun no registran su salida
        (SELECT COUNT(*)
         FROM Visitas
         WHERE FechaSalida IS NULL) AS VisitasEnCurso;
END;
GO
