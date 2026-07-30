/*=========================================================
  PROCEDIMIENTO: RESUMEN DEL PANEL DE GESTION
  MODULO: GENERAL
  DESCRIPCION:
  Devuelve los totales que muestran las tarjetas del panel
  principal. No modifica datos.

  CAMBIO:
  Se agregan TotalVisitas y VisitasEnCurso para la tarjeta
  del modulo de Visitas. Las columnas anteriores se
  mantienen igual para no afectar al resto del panel.
=========================================================*/

CREATE OR ALTER PROCEDURE sp_Dashboard_Resumen
AS
BEGIN
    SET NOCOUNT ON;

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

        0 AS TotalVentas,

        0 AS TotalMantenimientos,

        -- Visitas registradas en total
        (SELECT COUNT(*)
         FROM Visitas) AS TotalVisitas,

        -- Visitantes que aun no registran su salida
        (SELECT COUNT(*)
         FROM Visitas
         WHERE FechaSalida IS NULL) AS VisitasEnCurso;
END;
GO
