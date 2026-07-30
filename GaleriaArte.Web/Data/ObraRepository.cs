using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GaleriaArte.Web.Data
{
    public class ObraRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public ObraRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // =====================================================
        // LISTAR TODAS LAS OBRAS
        // Procedimiento: sp_Obras_Listar
        // =====================================================

        public async Task<List<Obra>> ObtenerTodosAsync()
        {
            List<Obra> obras = new List<Obra>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Obras_Listar", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                obras.Add(MapearObra(reader));
            }

            return obras;
        }

        // =====================================================
        // OBTENER OBRA POR ID
        // Procedimiento: sp_Obras_ObtenerPorId
        // =====================================================

        public async Task<Obra?> ObtenerPorIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Obras_ObtenerPorId", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdObra",
                SqlDbType.Int
            ).Value = id;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearObra(reader);
            }

            return null;
        }

        // =====================================================
        // CREAR OBRA
        // Procedimiento: sp_Obras_Crear
        // =====================================================

        public async Task CrearAsync(
            Obra obra,
            int? idUsuario = null)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Obras_Crear", connection);

            command.CommandType = CommandType.StoredProcedure;

            AgregarParametrosObra(command, obra);

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
        // ACTUALIZAR OBRA
        // Procedimiento: sp_Obras_Actualizar
        // =====================================================

        public async Task ActualizarAsync(
            Obra obra,
            int? idUsuario = null)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Obras_Actualizar", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdObra",
                SqlDbType.Int
            ).Value = obra.IdObra;

            AgregarParametrosObra(command, obra);

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
        // CAMBIAR ESTADO
        // Procedimiento: sp_Obras_CambiarEstado
        // =====================================================

        public async Task CambiarEstadoAsync(
            int idObra,
            bool estado,
            int? idUsuario = null)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Obras_CambiarEstado", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdObra",
                SqlDbType.Int
            ).Value = idObra;

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
        // =====================================================

        public async Task<List<Artista>> ObtenerArtistasActivosAsync()
        {
            List<Artista> artistas = new List<Artista>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Artistas_ListarActivos", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                artistas.Add(new Artista
                {
                    IdArtista = reader.GetInt32(
                        reader.GetOrdinal("IdArtista")
                    ),

                    Nombre = reader.GetString(
                        reader.GetOrdinal("Nombre")
                    ),

                    Apellido = reader.GetString(
                        reader.GetOrdinal("Apellido")
                    )
                });
            }

            return artistas;
        }


        // =====================================================
        // LISTAR CATEGORÍAS ACTIVAS
        // Procedimiento: sp_Categorias_ListarActivas
        // =====================================================

        public async Task<List<Categoria>> ObtenerCategoriasActivasAsync()
        {
            List<Categoria> categorias = new List<Categoria>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand("sp_Categorias_ListarActivas", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categorias.Add(new Categoria
                {
                    IdCategoria = reader.GetInt32(
                        reader.GetOrdinal("IdCategoria")
                    ),

                    Nombre = reader.GetString(
                        reader.GetOrdinal("Nombre")
                    )
                });
            }

            return categorias;
        }



        // =====================================================
        // PARÁMETROS COMUNES PARA CREAR Y ACTUALIZAR
        // =====================================================

        private static void AgregarParametrosObra(
            SqlCommand command,
            Obra obra)
        {
            command.Parameters.Add(
                "@IdArtista",
                SqlDbType.Int
            ).Value = obra.IdArtista;

            command.Parameters.Add(
                "@IdCategoria",
                SqlDbType.Int
            ).Value = obra.IdCategoria;

            command.Parameters.Add(
                "@Codigo",
                SqlDbType.VarChar,
                30
            ).Value = obra.Codigo;

            command.Parameters.Add(
                "@Nombre",
                SqlDbType.NVarChar,
                100
            ).Value = obra.Nombre;

            command.Parameters.Add(
                "@Descripcion",
                SqlDbType.NVarChar,
                500
            ).Value = string.IsNullOrWhiteSpace(obra.Descripcion)
                ? DBNull.Value
                : obra.Descripcion;

            command.Parameters.Add(
                "@FechaCreacion",
                SqlDbType.Date
            ).Value = obra.FechaCreacion.HasValue
                ? obra.FechaCreacion.Value
                : DBNull.Value;

            SqlParameter valorEstimado =
                command.Parameters.Add(
                    "@ValorEstimado",
                    SqlDbType.Decimal
                );

            valorEstimado.Precision = 18;
            valorEstimado.Scale = 2;
            valorEstimado.Value = obra.ValorEstimado;

            command.Parameters.Add(
                "@EstadoConservacion",
                SqlDbType.NVarChar,
                20
            ).Value = obra.EstadoConservacion;
        }

        // =====================================================
        // MAPEAR RESULTADO SQL A MODELO OBRA
        // =====================================================

        private static Obra MapearObra(SqlDataReader reader)
        {
            return new Obra
            {
                IdObra = reader.GetInt32(
                    reader.GetOrdinal("IdObra")
                ),

                IdArtista = reader.GetInt32(
                    reader.GetOrdinal("IdArtista")
                ),

                IdCategoria = reader.GetInt32(
                    reader.GetOrdinal("IdCategoria")
                ),

                Codigo = reader.GetString(
                    reader.GetOrdinal("Codigo")
                ),

                Nombre = reader.GetString(
                    reader.GetOrdinal("Nombre")
                ),

                Descripcion =
                    reader["Descripcion"] == DBNull.Value
                        ? null
                        : reader["Descripcion"].ToString(),

                FechaCreacion =
                    reader["FechaCreacion"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["FechaCreacion"]
                        ),

                FechaIngresoGaleria =
                    Convert.ToDateTime(
                        reader["FechaIngresoGaleria"]
                    ),

                ValorEstimado =
                    Convert.ToDecimal(
                        reader["ValorEstimado"]
                    ),

                EstadoConservacion =
                    reader.GetString(
                        reader.GetOrdinal(
                            "EstadoConservacion"
                        )
                    ),

                Estado =
                    Convert.ToBoolean(
                        reader["Estado"]
                    ),

                NombreArtista =
                    reader["NombreArtista"].ToString()
                    ?? string.Empty,

                NombreCategoria =
                    reader["NombreCategoria"].ToString()
                    ?? string.Empty
            };
        }
    }
}