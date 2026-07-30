using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GaleriaArte.Web.Data
{
    public class ArtistaRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public ArtistaRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // =====================================================
        // LISTAR TODOS LOS ARTISTAS
        // Procedimiento: sp_Artistas_Listar
        // =====================================================

        public async Task<List<Artista>> ObtenerTodosAsync()
        {
            List<Artista> artistas = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Artistas_Listar", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                artistas.Add(MapearArtista(reader));
            }

            return artistas;
        }

        // =====================================================
        // OBTENER ARTISTA POR ID
        // Procedimiento: sp_Artistas_ObtenerPorId
        // =====================================================

        public async Task<Artista?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Artistas_ObtenerPorId",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdArtista",
                SqlDbType.Int
            ).Value = id;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearArtista(reader);
            }

            return null;
        }

        // =====================================================
        // CREAR ARTISTA
        // Procedimiento: sp_Artistas_Crear
        // =====================================================

        public async Task<int> CrearAsync(
            Artista artista,
            int? idUsuario = null)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Artistas_Crear",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            AgregarParametrosArtista(command, artista);

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario.HasValue
                ? idUsuario.Value
                : DBNull.Value;

            await connection.OpenAsync();

            object? resultado =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(resultado);
        }

        // =====================================================
        // ACTUALIZAR ARTISTA
        // Procedimiento: sp_Artistas_Actualizar
        // =====================================================

        public async Task<bool> ActualizarAsync(
            Artista artista,
            int? idUsuario = null)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Artistas_Actualizar",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdArtista",
                SqlDbType.Int
            ).Value = artista.IdArtista;

            AgregarParametrosArtista(command, artista);

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario.HasValue
                ? idUsuario.Value
                : DBNull.Value;

            await connection.OpenAsync();

            object? resultado =
                await command.ExecuteScalarAsync();

            return resultado != null &&
                   Convert.ToBoolean(resultado);
        }

        // =====================================================
        // CAMBIAR ESTADO DEL ARTISTA
        // Procedimiento: sp_Artistas_CambiarEstado
        // =====================================================

        public async Task CambiarEstadoAsync(
            int idArtista,
            bool estado,
            int? idUsuario = null)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Artistas_CambiarEstado",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdArtista",
                SqlDbType.Int
            ).Value = idArtista;

            command.Parameters.Add(
                "@Estado",
                SqlDbType.Bit
            ).Value = estado;

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario.HasValue
                ? idUsuario.Value
                : DBNull.Value;

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // LISTAR ARTISTAS ACTIVOS
        // Procedimiento: sp_Artistas_ListarActivos
        // Usado por el CRUD de Obras
        // =====================================================

        public async Task<List<Artista>> ObtenerActivosAsync()
        {
            List<Artista> artistas = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Artistas_ListarActivos",
                    connection
                );

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                artistas.Add(
                    new Artista
                    {
                        IdArtista =
                            reader.GetInt32(
                                reader.GetOrdinal("IdArtista")
                            ),

                        Nombre =
                            reader.GetString(
                                reader.GetOrdinal("Nombre")
                            ),

                        Apellido =
                            reader.GetString(
                                reader.GetOrdinal("Apellido")
                            )
                    }
                );
            }

            return artistas;
        }

        // =====================================================
        // PARÁMETROS COMUNES
        // CREAR Y ACTUALIZAR
        // =====================================================

        private static void AgregarParametrosArtista(
            SqlCommand command,
            Artista artista)
        {
            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                50
            ).Value = artista.Nombre;

            command.Parameters.Add(
                "@Apellido",
                SqlDbType.NVarChar,
                50
            ).Value = artista.Apellido;

            command.Parameters.Add(
                "@Pais",
                SqlDbType.NVarChar,
                50
            ).Value = artista.Pais;

            command.Parameters.Add(
                "@FechaNacimiento",
                SqlDbType.Date
            ).Value = artista.FechaNacimiento.HasValue
                ? artista.FechaNacimiento.Value
                : DBNull.Value;

            command.Parameters.Add(
                "@Correo",
                SqlDbType.VarChar,
                100
            ).Value = string.IsNullOrWhiteSpace(artista.Correo)
                ? DBNull.Value
                : artista.Correo;

            command.Parameters.Add(
                "@Telefono",
                SqlDbType.VarChar,
                20
            ).Value = string.IsNullOrWhiteSpace(artista.Telefono)
                ? DBNull.Value
                : artista.Telefono;

            command.Parameters.Add(
                "@Biografia",
                SqlDbType.NVarChar
            ).Value = string.IsNullOrWhiteSpace(artista.Biografia)
                ? DBNull.Value
                : artista.Biografia;

            command.Parameters.Add(
                "@Estado",
                SqlDbType.Bit
            ).Value = artista.Estado;
        }

        // =====================================================
        // MAPEAR RESULTADO SQL AL MODELO ARTISTA
        // =====================================================

        private static Artista MapearArtista(
            SqlDataReader reader)
        {
            return new Artista
            {
                IdArtista =
                    reader.GetInt32(
                        reader.GetOrdinal("IdArtista")
                    ),

                Nombre =
                    reader.GetString(
                        reader.GetOrdinal("Nombre")
                    ),

                Apellido =
                    reader.GetString(
                        reader.GetOrdinal("Apellido")
                    ),

                Pais =
                    reader.GetString(
                        reader.GetOrdinal("Pais")
                    ),

                FechaNacimiento =
                    reader["FechaNacimiento"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["FechaNacimiento"]
                        ),

                Correo =
                    reader["Correo"] == DBNull.Value
                        ? null
                        : reader["Correo"].ToString(),

                Telefono =
                    reader["Telefono"] == DBNull.Value
                        ? null
                        : reader["Telefono"].ToString(),

                Biografia =
                    reader["Biografia"] == DBNull.Value
                        ? null
                        : reader["Biografia"].ToString(),

                Estado =
                    Convert.ToBoolean(
                        reader["Estado"]
                    ),

                FechaRegistro =
                    Convert.ToDateTime(
                        reader["FechaRegistro"]
                    )
            };
        }
    }
}