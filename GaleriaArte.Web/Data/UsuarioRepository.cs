using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;

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
                    IdUsuario,
                    Nombre,
                    Apellido,
                    Correo,
                    Usuario,
                    PasswordHash,
                    Estado,
                    FechaRegistro,
                    UltimoAcceso
                FROM Usuarios
                WHERE Usuario = @Usuario;";

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
    }
}