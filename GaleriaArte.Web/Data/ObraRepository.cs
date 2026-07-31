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

            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Obras_Listar", connection);
            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

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
            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Obras_ObtenerPorId", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@IdObra", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

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
        public async Task CrearAsync(Obra obra, int? idUsuario = null)
        {
            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Obras_Crear", connection);
            command.CommandType = CommandType.StoredProcedure;

            AgregarParametrosCrearObra(command, obra);

            command.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.HasValue
                ? idUsuario.Value
                : DBNull.Value;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // CREAR OBRA CON CÓDIGO HEREDADO DE MANTENIMIENTO
        // Procedimiento: sp_InsertarObraAutogenerada
        // =====================================================

        public async Task CrearConCodigoAutogeneradaAsync(Obra obra, int? idUsuario = null)
        {
            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_InsertarObraAutogenerada", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@IdArtista", SqlDbType.Int).Value = obra.IdArtista;
            command.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = obra.IdCategoria;
            command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = obra.Nombre;
            command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 1000).Value = (object)obra.Descripcion ?? DBNull.Value;
            command.Parameters.Add("@FechaCreacion", SqlDbType.Date).Value = (object)obra.FechaCreacion ?? DBNull.Value;
            command.Parameters.Add("@ValorEstimado", SqlDbType.Decimal).Value = obra.ValorEstimado;
            command.Parameters.Add("@EstadoConservacion", SqlDbType.NVarChar, 40).Value = obra.EstadoConservacion;
            command.Parameters.Add("@CodigoObra", SqlDbType.VarChar, 30).Value = obra.Codigo ?? string.Empty;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        } 

        // =====================================================
        // ACTUALIZAR OBRA
        // Procedimiento: sp_Obras_Actualizar
        // =====================================================
        public async Task ActualizarAsync(Obra obra, int? idUsuario = null)
        {
            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Obras_Actualizar", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@IdObra", SqlDbType.Int).Value = obra.IdObra;

            AgregarParametrosObra(command, obra);

            command.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.HasValue
                ? idUsuario.Value
                : DBNull.Value;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // REBAJAR STOCK DE PRODUCTO EN EL INVENTARIO
        // Procedimiento: sp_RegistrarGastoProducto
        // =====================================================
        public async Task RegistrarConsumoProductoAsync(int idObra, int idProducto, int cantidadUsada)
        {
            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_RegistrarGastoProducto", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@IdObra", SqlDbType.Int).Value = idObra;
            command.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;
            command.Parameters.Add("@CantidadUsada", SqlDbType.Int).Value = cantidadUsada;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // CAMBIAR ESTADO
        // Procedimiento: sp_Obras_CambiarEstado
        // =====================================================
        public async Task CambiarEstadoAsync(int idObra, bool estado, int? idUsuario = null)
        {
            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Obras_CambiarEstado", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@IdObra", SqlDbType.Int).Value = idObra;
            command.Parameters.Add("@Estado", SqlDbType.Bit).Value = estado;

            command.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.HasValue
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

            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Artistas_ListarActivos", connection);
            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                artistas.Add(new Artista
                {
                    IdArtista = reader.GetInt32(reader.GetOrdinal("IdArtista")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre"))!,
                    Apellido = reader.GetString(reader.GetOrdinal("Apellido"))!
                });
            }

            return artistas;
        }

        // =====================================================
        // LISTAR CATEGORÍAS ACTIVAS
        // =====================================================
        public async Task<List<Categoria>> ObtenerCategoriasActivasAsync()
        {
            List<Categoria> categorias = new List<Categoria>();

            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("sp_Categorias_ListarActivas", connection);
            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categorias.Add(new Categoria
                {
                    IdCategoria = reader.GetInt32(reader.GetOrdinal("IdCategoria")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre"))!
                });
            }

            return categorias;
        }

        // =====================================================
        // LISTAR PRODUCTOS CON STOCK DISPONIBLE
        // =====================================================
        public async Task<List<Producto>> ObtenerProductosInventarioAsync()
        {
            List<Producto> productos = new List<Producto>();

            await using SqlConnection connection = _databaseConnection.CreateConnection();
            await using SqlCommand command = new SqlCommand("SELECT IdProducto, Nombre, Stock FROM Productos WHERE Estado = 1", connection);
            command.CommandType = CommandType.Text;

            await connection.OpenAsync();
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                productos.Add(new Producto
                {
                    IdProducto = reader.GetInt32(reader.GetOrdinal("IdProducto")),
                    Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nombre"))!,
                    Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
                });
            }

            return productos;
        }

        // =====================================================
        // MÉTODOS DE MAPEADO Y PARÁMETROS AUXILIARES
        // =====================================================
        private Obra MapearObra(SqlDataReader reader)
        {
            return new Obra
            {
                IdObra = reader.GetInt32(reader.GetOrdinal("IdObra")),
                IdArtista = reader.GetInt32(reader.GetOrdinal("IdArtista")),
                IdCategoria = reader.GetInt32(reader.GetOrdinal("IdCategoria")),
                Codigo = reader.IsDBNull(reader.GetOrdinal("Codigo")) ? string.Empty : reader.GetString(reader.GetOrdinal("Codigo"))!,
                Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nombre"))!,
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                FechaCreacion = reader.IsDBNull(reader.GetOrdinal("FechaCreacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                FechaIngresoGaleria = reader.GetDateTime(reader.GetOrdinal("FechaIngresoGaleria")),
                ValorEstimado = reader.GetDecimal(reader.GetOrdinal("ValorEstimado")),
                EstadoConservacion = reader.IsDBNull(reader.GetOrdinal("EstadoConservacion")) ? string.Empty : reader.GetString(reader.GetOrdinal("EstadoConservacion"))!,
                Estado = reader.GetBoolean(reader.GetOrdinal("Estado")),
                NombreArtista = reader.IsDBNull(reader.GetOrdinal("NombreArtista")) ? string.Empty : reader.GetString(reader.GetOrdinal("NombreArtista"))!,
                NombreCategoria = reader.IsDBNull(reader.GetOrdinal("NombreCategoria")) ? string.Empty : reader.GetString(reader.GetOrdinal("NombreCategoria"))!
            };
        }


        private static void AgregarParametrosCrearObra(SqlCommand command, Obra obra)
        {
            command.Parameters.Add("@IdArtista", SqlDbType.Int).Value = obra.IdArtista;
            command.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = obra.IdCategoria;
            command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = obra.Nombre ?? string.Empty;
            command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(obra.Descripcion) ? DBNull.Value : obra.Descripcion;
            command.Parameters.Add("@FechaCreacion", SqlDbType.Date).Value =
                obra.FechaCreacion.HasValue ? obra.FechaCreacion.Value : DBNull.Value;

            SqlParameter valorEstimado = command.Parameters.Add("@ValorEstimado", SqlDbType.Decimal);
            valorEstimado.Precision = 18;
            valorEstimado.Scale = 2;
            valorEstimado.Value = obra.ValorEstimado;

            command.Parameters.Add("@EstadoConservacion", SqlDbType.NVarChar, 20).Value =
                obra.EstadoConservacion ?? string.Empty;
        }

        private void AgregarParametrosObra(SqlCommand command, Obra obra)
        {
            command.Parameters.Add("@IdArtista", SqlDbType.Int).Value = obra.IdArtista;
            command.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = obra.IdCategoria;
            command.Parameters.Add("@Codigo", SqlDbType.VarChar, 30).Value = obra.Codigo ?? string.Empty;
            command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = obra.Nombre ?? string.Empty;
            command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 1000).Value = (object)obra.Descripcion ?? DBNull.Value;
            command.Parameters.Add("@FechaCreacion", SqlDbType.Date).Value = (object)obra.FechaCreacion ?? DBNull.Value;
            command.Parameters.Add("@ValorEstimado", SqlDbType.Decimal).Value = obra.ValorEstimado;
            command.Parameters.Add("@EstadoConservacion", SqlDbType.NVarChar, 40).Value = obra.EstadoConservacion ?? string.Empty;
        }
    }
}
