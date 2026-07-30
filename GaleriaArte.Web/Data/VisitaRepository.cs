using System.Data;
using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;

namespace GaleriaArte.Web.Data
{
    public class VisitaRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public VisitaRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // =====================================================
        // LISTAR VISITAS
        // Procedimiento: sp_Visitas_Listar
        // =====================================================

        public async Task<List<Visita>> ObtenerTodosAsync()
        {
            List<Visita> visitas = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Visitas_Listar", connection);

            command.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                visitas.Add(MapearVisita(reader));
            }

            return visitas;
        }

        // =====================================================
        // OBTENER VISITA POR ID
        // Procedimiento: sp_Visitas_ObtenerPorId
        // =====================================================

        public async Task<Visita?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Visitas_ObtenerPorId", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdVisita",
                SqlDbType.Int
            ).Value = id;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearVisita(reader);
            }

            return null;
        }

        // =====================================================
        // REGISTRAR ENTRADA DEL VISITANTE
        // Procedimiento: sp_Visitas_RegistrarEntrada
        // =====================================================

        public async Task<int> RegistrarEntradaAsync(
            Visita visita,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Visitas_RegistrarEntrada",
                    connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = visita.IdCliente;

            command.Parameters.Add(
                "@FechaIngreso",
                SqlDbType.DateTime2
            ).Value = visita.FechaIngreso;

            command.Parameters.Add(
                "@Observaciones",
                SqlDbType.NVarChar,
                500
            ).Value =
                string.IsNullOrWhiteSpace(visita.Observaciones)
                    ? DBNull.Value
                    : visita.Observaciones;

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(resultado);
        }

        // =====================================================
        // REGISTRAR SALIDA DEL VISITANTE
        // Procedimiento: sp_Visitas_RegistrarSalida
        // =====================================================

        public async Task<bool> RegistrarSalidaAsync(
            int idVisita,
            DateTime fechaSalida,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Visitas_RegistrarSalida",
                    connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdVisita",
                SqlDbType.Int
            ).Value = idVisita;

            command.Parameters.Add(
                "@FechaSalida",
                SqlDbType.DateTime2
            ).Value = fechaSalida;

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
        // ACTUALIZAR VISITA
        // Procedimiento: sp_Visitas_Actualizar
        // =====================================================

        public async Task<bool> ActualizarAsync(
            Visita visita,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Visitas_Actualizar", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdVisita",
                SqlDbType.Int
            ).Value = visita.IdVisita;

            command.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = visita.IdCliente;

            command.Parameters.Add(
                "@FechaIngreso",
                SqlDbType.DateTime2
            ).Value = visita.FechaIngreso;

            command.Parameters.Add(
                "@FechaSalida",
                SqlDbType.DateTime2
            ).Value = visita.FechaSalida.HasValue
                ? visita.FechaSalida.Value
                : DBNull.Value;

            command.Parameters.Add(
                "@Observaciones",
                SqlDbType.NVarChar,
                500
            ).Value =
                string.IsNullOrWhiteSpace(visita.Observaciones)
                    ? DBNull.Value
                    : visita.Observaciones;

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
        // LISTAR CLIENTES ACTIVOS
        // =====================================================

        public async Task<List<Cliente>> ObtenerClientesActivosAsync()
        {
            List<Cliente> clientes = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sql = @"
                SELECT
                    IdCliente,
                    Nombre,
                    Apellido
                FROM Clientes
                WHERE Estado = 1
                ORDER BY Nombre, Apellido;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clientes.Add(new Cliente
                {
                    IdCliente =
                        Convert.ToInt32(reader["IdCliente"]),

                    Nombre =
                        reader["Nombre"].ToString() ?? string.Empty,

                    Apellido =
                        reader["Apellido"].ToString() ?? string.Empty
                });
            }

            return clientes;
        }

        // =====================================================
        // MAPEAR RESULTADO SQL
        // =====================================================

        private static Visita MapearVisita(
            SqlDataReader reader)
        {
            return new Visita
            {
                IdVisita =
                    Convert.ToInt32(reader["IdVisita"]),

                IdCliente =
                    Convert.ToInt32(reader["IdCliente"]),

                FechaIngreso =
                    Convert.ToDateTime(reader["FechaIngreso"]),

                FechaSalida =
                    reader["FechaSalida"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["FechaSalida"]),

                Observaciones =
                    reader["Observaciones"] == DBNull.Value
                        ? null
                        : reader["Observaciones"].ToString(),

                NombreCliente =
                    reader["NombreCliente"].ToString() ?? string.Empty,

                ApellidoCliente =
                    reader["ApellidoCliente"].ToString() ?? string.Empty,

                CorreoCliente =
                    reader["CorreoCliente"] == DBNull.Value
                        ? string.Empty
                        : reader["CorreoCliente"].ToString() ?? string.Empty,

                TelefonoCliente =
                    reader["TelefonoCliente"] == DBNull.Value
                        ? string.Empty
                        : reader["TelefonoCliente"].ToString() ?? string.Empty
            };
        }
    }
}
