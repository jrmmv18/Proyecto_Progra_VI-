using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GaleriaArte.Web.Data
{
    public class UsuarioRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public UsuarioRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            const string sql = @"
    SELECT
        U.IdUsuario,
        U.Nombre,
        U.Apellido,
        U.Correo,
        U.Usuario,
        U.PasswordHash,
        U.Estado,
        U.FechaRegistro,
        U.UltimoAcceso,
        R.IdRol,
        R.Nombre AS Rol
    FROM Usuarios U
        INNER JOIN UsuarioRoles UR
            ON U.IdUsuario = UR.IdUsuario
        INNER JOIN Roles R
            ON UR.IdRol = R.IdRol
    WHERE
        U.Usuario = @Usuario
        AND UR.Estado = 1;";

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Usuario",
                nombreUsuario
            );

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Usuario
            {
                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                Correo = reader.GetString(reader.GetOrdinal("Correo")),

                NombreUsuario =
                    reader.GetString(reader.GetOrdinal("Usuario")),

                PasswordHash =
                    reader.GetString(reader.GetOrdinal("PasswordHash")),

                IdRol =
    reader.GetInt32(reader.GetOrdinal("IdRol")),

                Rol =
    reader.GetString(reader.GetOrdinal("Rol")),



                Estado = reader.GetBoolean(reader.GetOrdinal("Estado")),

                FechaRegistro =
                    reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),

                UltimoAcceso = reader.IsDBNull(
                    reader.GetOrdinal("UltimoAcceso")
                )
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("UltimoAcceso")
                    )
            };
        }

        public async Task<bool> CrearAsync(
    Usuario usuario,
    int idUsuarioEjecutor)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Usuarios_Crear", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@IdRol",
                usuario.IdRol);

            command.Parameters.AddWithValue(
                "@Nombre",
                usuario.Nombre);

            command.Parameters.AddWithValue(
                "@Apellido",
                usuario.Apellido);

            command.Parameters.AddWithValue(
                "@Correo",
                usuario.Correo);

            command.Parameters.AddWithValue(
                "@Usuario",
                usuario.NombreUsuario);

            command.Parameters.AddWithValue(
                "@PasswordHash",
                usuario.PasswordHash);

            command.Parameters.AddWithValue(
                "@Estado",
                usuario.Estado);

            command.Parameters.AddWithValue(
                "@IdUsuarioEjecutor",
                idUsuarioEjecutor);

           await command.ExecuteNonQueryAsync();

            return true;
        }


        public async Task<bool> ActualizarAsync(
          Usuario usuario,
        int idUsuarioEjecutor)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Usuarios_Actualizar", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@IdUsuario",
                usuario.IdUsuario);

            command.Parameters.AddWithValue(
                "@IdRol",
                usuario.IdRol);

            command.Parameters.AddWithValue(
                "@Nombre",
                usuario.Nombre);

            command.Parameters.AddWithValue(
                "@Apellido",
                usuario.Apellido);

            command.Parameters.AddWithValue(
                "@Correo",
                usuario.Correo);

            command.Parameters.AddWithValue(
                "@Usuario",
                usuario.NombreUsuario);

            command.Parameters.AddWithValue(
                "@PasswordHash",
                string.IsNullOrWhiteSpace(usuario.PasswordHash)
                    ? DBNull.Value
                    : usuario.PasswordHash);

            command.Parameters.AddWithValue(
                "@Estado",
                usuario.Estado);

            command.Parameters.AddWithValue(
                "@IdUsuarioEjecutor",
                idUsuarioEjecutor);

            await command.ExecuteNonQueryAsync();

            return true;
        }


        public async Task<bool> CambiarEstadoAsync(
     int idUsuario,
    bool estado,
    int idUsuarioEjecutor)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Usuarios_CambiarEstado", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@IdUsuario",
                idUsuario);

            command.Parameters.AddWithValue(
                "@Estado",
                estado);

            command.Parameters.AddWithValue(
                "@IdUsuarioEjecutor",
                idUsuarioEjecutor);

            await command.ExecuteNonQueryAsync();

            return true;
        }



        public async Task<List<Usuario>> ListarAsync()
        {
            List<Usuario> lista = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Usuarios_Listar", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),

                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),

                    Apellido = reader.GetString(reader.GetOrdinal("Apellido")),

                    Correo = reader.GetString(reader.GetOrdinal("Correo")),

                    NombreUsuario =
                        reader.GetString(reader.GetOrdinal("Usuario")),

                    IdRol =
                        reader.GetInt32(reader.GetOrdinal("IdRol")),

                    Rol =
                        reader.GetString(reader.GetOrdinal("Rol")),

                    Estado =
                        reader.GetBoolean(reader.GetOrdinal("Estado")),

                    FechaRegistro =
                        reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),

                    UltimoAcceso =
                        reader.IsDBNull(reader.GetOrdinal("UltimoAcceso"))
                            ? null
                            : reader.GetDateTime(
                                reader.GetOrdinal("UltimoAcceso"))
                });
            }

            return lista;
        }


        public async Task<Usuario?> ObtenerPorIdAsync(int idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Usuarios_ObtenerPorId", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@IdUsuario",
                idUsuario
            );

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Usuario
            {
                IdUsuario =
                    reader.GetInt32(reader.GetOrdinal("IdUsuario")),

                Nombre =
                    reader.GetString(reader.GetOrdinal("Nombre")),

                Apellido =
                    reader.GetString(reader.GetOrdinal("Apellido")),

                Correo =
                    reader.GetString(reader.GetOrdinal("Correo")),

                NombreUsuario =
                    reader.GetString(reader.GetOrdinal("Usuario")),

                PasswordHash =
                    reader.GetString(reader.GetOrdinal("PasswordHash")),

                IdRol =
                    reader.GetInt32(reader.GetOrdinal("IdRol")),

                Rol =
                    reader.GetString(reader.GetOrdinal("Rol")),

                Estado =
                    reader.GetBoolean(reader.GetOrdinal("Estado")),

                FechaRegistro =
                    reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),

                UltimoAcceso =
                    reader.IsDBNull(reader.GetOrdinal("UltimoAcceso"))
                        ? null
                        : reader.GetDateTime(
                            reader.GetOrdinal("UltimoAcceso"))
            };
        }


        public async Task<List<Rol>> ObtenerRolesActivosAsync()
        {
            var roles = new List<Rol>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand("sp_Roles_ListarActivos", connection);

            command.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                roles.Add(new Rol
                {
                    IdRol = Convert.ToInt32(reader["IdRol"]),
                    Nombre = reader["Nombre"].ToString()!
                });
            }

            return roles;
        }



    }
}