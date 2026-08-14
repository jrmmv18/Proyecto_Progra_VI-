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
        // REGISTRAR VISITANTE Y SU ENTRADA
        // Procedimientos: sp_Clientes_Crear
        //                 sp_Visitas_RegistrarEntrada
        //                 sp_Visitas_RegistrarSalida
        //
        // El visitante se da de alta en el mismo paso que su
        // entrada. Si tambien se indica la salida, se sella de
        // una vez.
        // =====================================================

        public async Task<int> RegistrarVisitanteAsync(
            VisitaCrearViewModel modelo,
            int idUsuario)
        {
            int idCliente =
                await CrearClienteAsync(modelo, idUsuario);

            Visita visita = new()
            {
                IdCliente = idCliente,
                FechaIngreso = modelo.FechaIngreso,
                Observaciones = modelo.Observaciones
            };

            int idVisita =
                await RegistrarEntradaAsync(visita, idUsuario);

            if (modelo.FechaSalida.HasValue)
            {
                await RegistrarSalidaAsync(
                    idVisita,
                    modelo.FechaSalida.Value,
                    idUsuario);
            }

            return idVisita;
        }

        // =====================================================
        // CREAR EL VISITANTE
        // Procedimiento: sp_Clientes_Crear
        // =====================================================

        private async Task<int> CrearClienteAsync(
            VisitaCrearViewModel modelo,
            int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Clientes_Crear", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                50
            ).Value = modelo.Nombre;

            command.Parameters.Add(
                "@Apellido",
                SqlDbType.NVarChar,
                50
            ).Value = modelo.Apellido;

            command.Parameters.Add(
                "@Telefono",
                SqlDbType.VarChar,
                20
            ).Value =
                string.IsNullOrWhiteSpace(modelo.Telefono)
                    ? DBNull.Value
                    : modelo.Telefono;

            command.Parameters.Add(
                "@Correo",
                SqlDbType.VarChar,
                100
            ).Value =
                string.IsNullOrWhiteSpace(modelo.Correo)
                    ? DBNull.Value
                    : modelo.Correo;

            // El visitante no lleva direccion: ese espacio del
            // formulario lo ocupan las observaciones de la visita.
            command.Parameters.Add(
                "@Direccion",
                SqlDbType.NVarChar,
                200
            ).Value = DBNull.Value;

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            object? resultado =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(resultado);
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
