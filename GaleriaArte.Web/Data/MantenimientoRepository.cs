using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using GaleriaArte.Web.Models;

namespace GaleriaArte.Web.Data
{
    public class MantenimientoRepository : IMantenimientoRepository
    {
        private readonly string _connectionString;


    public MantenimientoRepository(
        IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            _connectionString =
                configuration.GetConnectionString("GaleriaArteDB")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'GaleriaArteDB'.");
        }

        // 1. REGISTRAR EL MANTENIMIENTO PRINCIPAL
        // Devuelve el IdMantenimiento generado.
        public int RegistrarMantenimientoCompleto(Mantenimiento mantenimiento)
        {
            if (mantenimiento == null)
                throw new ArgumentNullException(nameof(mantenimiento));

            using var conn = new SqlConnection(_connectionString);

            using var cmd = new SqlCommand(
                "dbo.sp_RegistrarMantenimiento",
                conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@IdObra", SqlDbType.Int).Value =
                mantenimiento.IdObra;

            cmd.Parameters.Add("@TipoMantenimiento", SqlDbType.NVarChar, 80).Value =
                mantenimiento.TipoMantenimiento;

            cmd.Parameters.Add("@Observaciones", SqlDbType.NVarChar, 1000).Value =
                string.IsNullOrWhiteSpace(mantenimiento.DescripcionTrabajo)
                    ? DBNull.Value
                    : mantenimiento.DescripcionTrabajo;

            cmd.Parameters.Add("@FechaMantenimiento", SqlDbType.DateTime2).Value =
                mantenimiento.FechaMantenimiento;

            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value =
                mantenimiento.IdUsuario;

            conn.Open();

            object? resultado = cmd.ExecuteScalar();

            if (resultado == null || resultado == DBNull.Value)
                throw new InvalidOperationException(
                    "El procedimiento no devolvió el IdMantenimiento.");

            return Convert.ToInt32(resultado);
        }

        // 2. LISTAR LOS MANTENIMIENTOS
        public List<Mantenimiento> ListarTodos()
        {
            var lista =
                new List<Mantenimiento>();

            using var conn =
                new SqlConnection(_connectionString);

            using var cmd = new SqlCommand(
                "dbo.sp_ListarMantenimientos",
                conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            conn.Open();

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                var mantenimiento = new Mantenimiento
                {
                     IdMantenimiento =
                         ObtenerEntero(
                             reader,
                             "IdMantenimiento"),

                                    IdObra =
                         ObtenerEntero(
                             reader,
                             "IdObra"),

                                    NombreObra =
                         ObtenerTexto(
                             reader,
                             "NombreObra"),

                                    TipoMantenimiento =
                         ObtenerTexto(
                             reader,
                             "TipoMantenimiento"),

                                    DescripcionTrabajo =
                         ObtenerTexto(
                             reader,
                             "DescripcionTrabajo"),

                                    FechaMantenimiento =
                         ObtenerFecha(
                             reader,
                             "FechaMantenimiento"),

                                    IdUsuario =
                         ObtenerEntero(
                             reader,
                             "IdUsuario")
                     };

                lista.Add(mantenimiento);
            }

            return lista;
        }

        // 3. REGISTRAR UN PRODUCTO UTILIZADO
        // El procedimiento debe insertar el detalle
        // y descontar el stock.
        public bool RegistrarProductoUsado(
        int idMantenimiento,
        int idProducto,
        int cantidadUtilizada)
        {
            if (idMantenimiento <= 0)
            {
                throw new ArgumentException(
                    "El identificador del mantenimiento no es válido.",
                    nameof(idMantenimiento));
            }

            if (idProducto <= 0)
            {
                throw new ArgumentException(
                    "El identificador del producto no es válido.",
                    nameof(idProducto));
            }

            if (cantidadUtilizada <= 0)
            {
                throw new ArgumentException(
                    "La cantidad utilizada debe ser mayor que cero.",
                    nameof(cantidadUtilizada));
            }

            using var conn =
                new SqlConnection(_connectionString);

            conn.Open();

            using var transaction =
                conn.BeginTransaction();

            try
            {
                using (var cmdRegistrar = new SqlCommand(
                    "dbo.sp_Mantenimiento_RegistrarProducto",
                    conn,
                    transaction))
                {
                    cmdRegistrar.CommandType =
                        CommandType.StoredProcedure;

                    cmdRegistrar.Parameters.Add(
                        "@IdMantenimiento",
                        SqlDbType.Int
                    ).Value = idMantenimiento;

                    cmdRegistrar.Parameters.Add(
                        "@IdProducto",
                        SqlDbType.Int
                    ).Value = idProducto;

                    cmdRegistrar.Parameters.Add(
                        "@CantidadUtilizada",
                        SqlDbType.Int
                    ).Value = cantidadUtilizada;

                    cmdRegistrar.ExecuteNonQuery();
                }

                const string actualizarStockSql = @"
            UPDATE dbo.Productos
            SET Stock = Stock - @CantidadUtilizada
            WHERE IdProducto = @IdProducto
              AND Estado = 1
              AND Stock >= @CantidadUtilizada;";

                using (var cmdStock = new SqlCommand(
                    actualizarStockSql,
                    conn,
                    transaction))
                {
                    cmdStock.CommandType =
                        CommandType.Text;

                    cmdStock.Parameters.Add(
                        "@IdProducto",
                        SqlDbType.Int
                    ).Value = idProducto;

                    cmdStock.Parameters.Add(
                        "@CantidadUtilizada",
                        SqlDbType.Int
                    ).Value = cantidadUtilizada;

                    int filasActualizadas =
                        cmdStock.ExecuteNonQuery();

                    if (filasActualizadas == 0)
                    {
                        throw new InvalidOperationException(
                            "El producto no existe, está inactivo o no tiene stock suficiente.");
                    }
                }

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // 4. OBTENER PRODUCTOS DISPONIBLES
        // Devuelve únicamente productos activos y con existencias.
        public List<Producto> ObtenerProductosDisponibles()
        {
            var productos =
                new List<Producto>();

            using var conn =
                new SqlConnection(_connectionString);

            const string sql = @"
            SELECT
                IdProducto,
                IdProveedor,
                Codigo,
                Nombre,
                Descripcion,
                Stock,
                StockMinimo,
                Precio,
                Estado,
                FechaRegistro
            FROM dbo.Productos
            WHERE Estado = 1
              AND Stock > 0
            ORDER BY Nombre ASC;";

            using var cmd =
                new SqlCommand(sql, conn);

            conn.Open();

            using var reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                var producto =
                    new Producto
                    {
                        IdProducto =
                            ObtenerEntero(
                                reader,
                                "IdProducto"),

                        IdProveedor =
                            ObtenerEntero(
                                reader,
                                "IdProveedor"),

                        Codigo =
                            ObtenerTexto(
                                reader,
                                "Codigo"),

                        Nombre =
                            ObtenerTexto(
                                reader,
                                "Nombre"),

                        Descripcion =
                            ObtenerTextoNullable(
                                reader,
                                "Descripcion"),

                        Stock =
                            ObtenerEntero(
                                reader,
                                "Stock"),

                        StockMinimo =
                            ObtenerEntero(
                                reader,
                                "StockMinimo"),

                        Precio =
                            ObtenerDecimal(
                                reader,
                                "Precio"),

                        Estado =
                            ObtenerBooleano(
                                reader,
                                "Estado"),

                        FechaRegistro =
                            ObtenerFecha(
                                reader,
                                "FechaRegistro")
                    };

                productos.Add(producto);
            }

            return productos;
        }

        // MÉTODOS AUXILIARES PARA LEER DATOS DE SQL SERVER

        private static int ObtenerEntero(
            SqlDataReader reader,
            string nombreColumna)
        {
            int posicion =
                reader.GetOrdinal(nombreColumna);

            return reader.IsDBNull(posicion)
                ? 0
                : Convert.ToInt32(
                    reader.GetValue(posicion));
        }

        private static decimal ObtenerDecimal(
            SqlDataReader reader,
            string nombreColumna)
        {
            int posicion =
                reader.GetOrdinal(nombreColumna);

            return reader.IsDBNull(posicion)
                ? 0m
                : Convert.ToDecimal(
                    reader.GetValue(posicion));
        }

        private static bool ObtenerBooleano(
            SqlDataReader reader,
            string nombreColumna)
        {
            int posicion =
                reader.GetOrdinal(nombreColumna);

            return !reader.IsDBNull(posicion) &&
                   Convert.ToBoolean(
                       reader.GetValue(posicion));
        }

        private static string ObtenerTexto(
            SqlDataReader reader,
            string nombreColumna)
        {
            int posicion =
                reader.GetOrdinal(nombreColumna);

            return reader.IsDBNull(posicion)
                ? string.Empty
                : reader.GetString(posicion);
        }

        private static string? ObtenerTextoNullable(
            SqlDataReader reader,
            string nombreColumna)
        {
            int posicion =
                reader.GetOrdinal(nombreColumna);

            return reader.IsDBNull(posicion)
                ? null
                : reader.GetString(posicion);
        }

        private static DateTime ObtenerFecha(
            SqlDataReader reader,
            string nombreColumna)
        {
            int posicion =
                reader.GetOrdinal(nombreColumna);

            return reader.IsDBNull(posicion)
                ? DateTime.Now
                : Convert.ToDateTime(
                    reader.GetValue(posicion));
        }
    }

}
