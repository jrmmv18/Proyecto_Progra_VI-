using System.Data;
using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;

namespace GaleriaArte.Web.Data
{
    public class ProveedorRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public ProveedorRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // LISTAR PROVEEDORES
        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            List<Proveedor> proveedores = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Proveedores_Listar", connection);

            command.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                proveedores.Add(MapearProveedor(reader));
            }

            return proveedores;
        }

        // OBTENER PROVEEDOR POR ID
        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Proveedores_ObtenerPorId", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdProveedor",
                SqlDbType.Int
            ).Value = id;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearProveedor(reader);
            }

            return null;
        }

        // CREAR PROVEEDOR
        public async Task<int> CrearAsync(
            Proveedor proveedor,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Proveedores_Crear", connection);

            command.CommandType = CommandType.StoredProcedure;

            AgregarParametros(command, proveedor);

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(resultado);
        }

        // ACTUALIZAR PROVEEDOR
        public async Task<bool> ActualizarAsync(
            Proveedor proveedor,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Proveedores_Actualizar", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdProveedor",
                SqlDbType.Int
            ).Value = proveedor.IdProveedor;

            AgregarParametros(command, proveedor);

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return resultado != null &&
                   Convert.ToBoolean(resultado);
        }

        // CAMBIAR ESTADO
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
                    "sp_Proveedores_CambiarEstado",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdProveedor",
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

        // PARÁMETROS COMUNES
        private static void AgregarParametros(
            SqlCommand command,
            Proveedor proveedor)
        {
            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                100
            ).Value = proveedor.Nombre;

            command.Parameters.Add(
                "@Telefono",
                SqlDbType.VarChar,
                20
            ).Value =
                (object?)proveedor.Telefono ?? DBNull.Value;

            command.Parameters.Add(
                "@Correo",
                SqlDbType.VarChar,
                100
            ).Value =
                (object?)proveedor.Correo ?? DBNull.Value;

            command.Parameters.Add(
                "@Direccion",
                SqlDbType.NVarChar,
                200
            ).Value =
                (object?)proveedor.Direccion ?? DBNull.Value;
        }

        // MAPEAR RESULTADO SQL
        private static Proveedor MapearProveedor(
            SqlDataReader reader)
        {
            return new Proveedor
            {
                IdProveedor =
                    Convert.ToInt32(reader["IdProveedor"]),

                Nombre =
                    reader["Nombre"].ToString() ?? string.Empty,

                Telefono =
                    reader["Telefono"] == DBNull.Value
                        ? null
                        : reader["Telefono"].ToString(),

                Correo =
                    reader["Correo"] == DBNull.Value
                        ? null
                        : reader["Correo"].ToString(),

                Direccion =
                    reader["Direccion"] == DBNull.Value
                        ? null
                        : reader["Direccion"].ToString(),

                Estado =
                    Convert.ToBoolean(reader["Estado"]),

                FechaRegistro =
                    Convert.ToDateTime(reader["FechaRegistro"])
            };
        }
    }
}