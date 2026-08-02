using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GaleriaArte.Web.Data
{
    public class ClienteRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public ClienteRepository(
            DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // =====================================================
        // LISTAR CLIENTES
        // Procedimiento: sp_Clientes_Listar
        // =====================================================

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            List<Cliente> clientes = new List<Cliente>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Clientes_Listar",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        // =====================================================
        // OBTENER CLIENTE POR ID
        // Procedimiento: sp_Clientes_Obtener
        // =====================================================

        public async Task<Cliente?> ObtenerPorIdAsync(
            int idCliente)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Clientes_Obtener",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = idCliente;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearCliente(reader);
            }

            return null;
        }

        // =====================================================
        // CREAR CLIENTE
        // Procedimiento: sp_Clientes_Crear
        // =====================================================

        public async Task<int> CrearAsync(
            Cliente cliente)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Clientes_Crear",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            AgregarParametrosCliente(command, cliente);

            await connection.OpenAsync();

            object? resultado =
                await command.ExecuteScalarAsync();

            if (resultado == null ||
                resultado == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "No fue posible obtener el identificador del cliente."
                );
            }

            return Convert.ToInt32(resultado);
        }

        // =====================================================
        // ACTUALIZAR CLIENTE
        // Procedimiento: sp_Clientes_Actualizar
        // =====================================================

        public async Task ActualizarAsync(
            Cliente cliente)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Clientes_Actualizar",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = cliente.IdCliente;

            AgregarParametrosCliente(command, cliente);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // CAMBIAR ESTADO
        // Procedimiento: sp_Clientes_CambiarEstado
        // =====================================================

        public async Task CambiarEstadoAsync(
            int idCliente)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Clientes_CambiarEstado",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = idCliente;

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // MÉTODOS PRIVADOS
        // =====================================================

        private static void AgregarParametrosCliente(
            SqlCommand command,
            Cliente cliente)
        {
            command.Parameters.Add(
                "@Nombre",
                SqlDbType.VarChar,
                100
            ).Value = cliente.Nombre.Trim();

            command.Parameters.Add(
                "@Apellido",
                SqlDbType.VarChar,
                100
            ).Value = cliente.Apellido.Trim();

            command.Parameters.Add(
                "@Telefono",
                SqlDbType.VarChar,
                20
            ).Value =
                string.IsNullOrWhiteSpace(cliente.Telefono)
                    ? DBNull.Value
                    : cliente.Telefono.Trim();

            command.Parameters.Add(
                "@Correo",
                SqlDbType.VarChar,
                100
            ).Value =
                string.IsNullOrWhiteSpace(cliente.Correo)
                    ? DBNull.Value
                    : cliente.Correo.Trim();

            command.Parameters.Add(
                "@Direccion",
                SqlDbType.VarChar,
                400
            ).Value =
                string.IsNullOrWhiteSpace(cliente.Direccion)
                    ? DBNull.Value
                    : cliente.Direccion.Trim();
        }

        private static Cliente MapearCliente(
            SqlDataReader reader)
        {
            return new Cliente
            {
                IdCliente = reader.GetInt32(
                    reader.GetOrdinal("IdCliente")
                ),

                Nombre = reader.GetString(
                    reader.GetOrdinal("Nombre")
                ),

                Apellido = reader.GetString(
                    reader.GetOrdinal("Apellido")
                ),

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

                Estado = Convert.ToBoolean(
                    reader["Estado"]
                ),

                FechaRegistro = Convert.ToDateTime(
                    reader["FechaRegistro"]
                )
            };
        }
    }
}