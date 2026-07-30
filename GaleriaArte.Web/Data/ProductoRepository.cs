using System.Data;
using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;

namespace GaleriaArte.Web.Data
{
    public class ProductoRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public ProductoRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // =====================================================
        // LISTAR PRODUCTOS
        // Procedimiento: sp_Productos_Listar
        // =====================================================

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            List<Producto> productos = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Productos_Listar", connection);

            command.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                productos.Add(MapearProducto(reader));
            }

            return productos;
        }

        // =====================================================
        // OBTENER PRODUCTO POR ID
        // Procedimiento: sp_Productos_ObtenerPorId
        // =====================================================

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Productos_ObtenerPorId", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdProducto",
                SqlDbType.Int
            ).Value = id;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearProducto(reader);
            }

            return null;
        }

        // =====================================================
        // CREAR PRODUCTO
        // Procedimiento: sp_Productos_Crear
        // =====================================================

        public async Task<int> CrearAsync(
            Producto producto,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Productos_Crear", connection);

            command.CommandType = CommandType.StoredProcedure;

            AgregarParametros(command, producto);

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(resultado);
        }

        // =====================================================
        // ACTUALIZAR PRODUCTO
        // Procedimiento: sp_Productos_Actualizar
        // =====================================================

        public async Task<bool> ActualizarAsync(
            Producto producto,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Productos_Actualizar", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdProducto",
                SqlDbType.Int
            ).Value = producto.IdProducto;

            AgregarParametros(command, producto);

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return resultado != null &&
                   Convert.ToBoolean(resultado);
        }
        // =====================================================
        // CAMBIAR ESTADO
        // Procedimiento: sp_Productos_CambiarEstado
        // =====================================================

        public async Task<bool> CambiarEstadoAsync(
            int id,
            bool estado,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Productos_CambiarEstado",
                    connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdProducto",
                SqlDbType.Int
            ).Value = id;

            command.Parameters.Add(
                "@Estado",
                SqlDbType.Bit
            ).Value = estado;

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return resultado != null &&
                   Convert.ToBoolean(resultado);
        }

        // =====================================================
        // LISTAR PROVEEDORES ACTIVOS
        // =====================================================

        public async Task<List<Proveedor>> ObtenerProveedoresActivosAsync()
        {
            List<Proveedor> proveedores = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sql = @"
                SELECT
                    IdProveedor,
                    Nombre
                FROM Proveedores
                WHERE Estado = 1
                ORDER BY Nombre;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                proveedores.Add(new Proveedor
                {
                    IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty
                });
            }

            return proveedores;
        }

        // =====================================================
        // PARÁMETROS COMUNES
        // =====================================================

        private static void AgregarParametros(
            SqlCommand command,
            Producto producto)
        {
            command.Parameters.Add(
                "@IdProveedor",
                SqlDbType.Int
            ).Value = producto.IdProveedor;

            command.Parameters.Add(
                "@Codigo",
                SqlDbType.VarChar,
                30
            ).Value = producto.Codigo;

            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                200
            ).Value = producto.Nombre;

            command.Parameters.Add(
                "@Descripcion",
                SqlDbType.NVarChar,
                500
            ).Value =
                string.IsNullOrWhiteSpace(producto.Descripcion)
                    ? DBNull.Value
                    : producto.Descripcion;

            command.Parameters.Add(
                "@Stock",
                SqlDbType.Int
            ).Value = producto.Stock;

            command.Parameters.Add(
                "@StockMinimo",
                SqlDbType.Int
            ).Value = producto.StockMinimo;

            SqlParameter precio =
                command.Parameters.Add(
                    "@Precio",
                    SqlDbType.Decimal
                );

            precio.Precision = 18;
            precio.Scale = 2;
            precio.Value = producto.Precio;
        }

        // =====================================================
        // MAPEAR RESULTADO SQL
        // =====================================================

        private static Producto MapearProducto(
            SqlDataReader reader)
        {
            return new Producto
            {
                IdProducto =
                    Convert.ToInt32(reader["IdProducto"]),

                IdProveedor =
                    Convert.ToInt32(reader["IdProveedor"]),

                Codigo =
                    reader["Codigo"].ToString() ?? string.Empty,

                Nombre =
                    reader["Nombre"].ToString() ?? string.Empty,

                Descripcion =
                    reader["Descripcion"] == DBNull.Value
                        ? null
                        : reader["Descripcion"].ToString(),

                Stock =
                    Convert.ToInt32(reader["Stock"]),

                StockMinimo =
                    Convert.ToInt32(reader["StockMinimo"]),

                Precio =
                    Convert.ToDecimal(reader["Precio"]),

                Estado =
                    Convert.ToBoolean(reader["Estado"]),

                FechaRegistro =
                    Convert.ToDateTime(reader["FechaRegistro"]),

                NombreProveedor =
                    reader["NombreProveedor"] == DBNull.Value
                        ? string.Empty
                        : reader["NombreProveedor"].ToString() ?? string.Empty
            };
        }
    }
}