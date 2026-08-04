using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;

namespace GaleriaArte.Web.Data
{
    public class BitacoraRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public BitacoraRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public async Task<List<BitacoraGeneral>> ObtenerBitacoraGeneralAsync()
        {
            List<BitacoraGeneral> lista = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sql = @"
SELECT
    B.IdBitacora,
    B.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    B.Modulo,
    B.TablaAfectada,
    B.TipoOperacion,
    B.RegistroAfectado,
    B.ValorAnterior,
    B.ValorNuevo,
    B.FechaHora,
    B.Resultado
FROM BitacoraGeneral B
LEFT JOIN Usuarios U
    ON U.IdUsuario = B.IdUsuario
ORDER BY B.FechaHora DESC;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new BitacoraGeneral
                {
                    IdBitacora = Convert.ToInt32(reader["IdBitacora"]),
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",
                    Modulo = reader["Modulo"]?.ToString() ?? "",
                    TablaAfectada = reader["TablaAfectada"]?.ToString() ?? "",
                    TipoOperacion = reader["TipoOperacion"]?.ToString() ?? "",
                    RegistroAfectado = Convert.ToInt32(reader["RegistroAfectado"]),
                    ValorAnterior = reader["ValorAnterior"] == DBNull.Value
                        ? null
                        : reader["ValorAnterior"].ToString(),
                    ValorNuevo = reader["ValorNuevo"] == DBNull.Value
                        ? null
                        : reader["ValorNuevo"].ToString(),
                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                    Resultado = reader["Resultado"]?.ToString() ?? ""
                });



            }




            return lista;
        }
        public async Task<Paginacion<BitacoraGeneral>> ObtenerBitacoraGeneralAsync(
    int pagina = 1,
    int registrosPorPagina = 15)
        {
            Paginacion<BitacoraGeneral> resultado = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sqlCount = @"
SELECT COUNT(*)
FROM BitacoraGeneral;";

            await using (SqlCommand countCommand = new SqlCommand(sqlCount, connection))
            {
                resultado.TotalRegistros =
                    Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            }

            resultado.PaginaActual = pagina;
            resultado.RegistrosPorPagina = registrosPorPagina;

            int offset = (pagina - 1) * registrosPorPagina;

            const string sql = @"
SELECT
    B.IdBitacora,
    B.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    B.Modulo,
    B.TablaAfectada,
    B.TipoOperacion,
    B.RegistroAfectado,
    B.ValorAnterior,
    B.ValorNuevo,
    B.FechaHora,
    B.Resultado
FROM BitacoraGeneral B
LEFT JOIN Usuarios U
ON U.IdUsuario = B.IdUsuario
ORDER BY B.FechaHora DESC
OFFSET @Offset ROWS
FETCH NEXT @Registros ROWS ONLY;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Offset", offset);
            command.Parameters.AddWithValue("@Registros", registrosPorPagina);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resultado.Datos.Add(new BitacoraGeneral
                {
                    IdBitacora = Convert.ToInt32(reader["IdBitacora"]),
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",
                    Modulo = reader["Modulo"]?.ToString() ?? "",
                    TablaAfectada = reader["TablaAfectada"]?.ToString() ?? "",
                    TipoOperacion = reader["TipoOperacion"]?.ToString() ?? "",
                    RegistroAfectado = Convert.ToInt32(reader["RegistroAfectado"]),
                    ValorAnterior = reader["ValorAnterior"] == DBNull.Value ? null : reader["ValorAnterior"].ToString(),
                    ValorNuevo = reader["ValorNuevo"] == DBNull.Value ? null : reader["ValorNuevo"].ToString(),
                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                    Resultado = reader["Resultado"]?.ToString() ?? ""
                });
            }

            return resultado;
        }





















        private async Task<List<BitacoraModulo>> ObtenerBitacoraModuloAsync(
    string tabla,
    string idCampo)
        {
            List<BitacoraModulo> lista = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            string sql = $@"
SELECT
    B.{idCampo} AS IdBitacora,
    B.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    B.Procedimiento,
    B.Operacion,
    B.RegistroAfectado,
    B.Detalle,
    B.FechaHora,
    B.Resultado
FROM {tabla} B
LEFT JOIN Usuarios U
    ON U.IdUsuario = B.IdUsuario
ORDER BY B.FechaHora DESC";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new BitacoraModulo
                {
                    IdBitacora = Convert.ToInt32(reader["IdBitacora"]),
                    IdUsuario = reader["IdUsuario"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["IdUsuario"]),

                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",
                    Procedimiento = reader["Procedimiento"]?.ToString() ?? "",
                    Operacion = reader["Operacion"]?.ToString() ?? "",
                    RegistroAfectado = reader["RegistroAfectado"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["RegistroAfectado"]),
                    Detalle = reader["Detalle"]?.ToString(),
                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                    Resultado = reader["Resultado"]?.ToString() ?? ""
                });
            }

            return lista;
        }




        public async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraInventarioAsync(
       int pagina = 1,
       int registrosPorPagina = 15)
        {
            return await ObtenerBitacoraModuloPaginadoAsync(
                "BitacoraInventario",
                "IdBitacoraInventario",
                pagina,
                registrosPorPagina);
        }

        public async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraObrasAsync(
     int pagina = 1,
     int registros = 15)
        {
            return await ObtenerBitacoraModuloPaginadoAsync(
                "BitacoraObras",
                "IdBitacoraObras",
                pagina,
                registros);
        }




        public async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraSeguridadAsync(
    int pagina = 1,
    int registrosPorPagina = 15)
        {
            return await ObtenerBitacoraModuloPaginadoAsync(
                 "BitacoraSeguridad",
                "IdBitacoraSeguridad",
                pagina,
                registrosPorPagina);
        }

        public async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraVentasAsync(
    int pagina = 1,
    int registrosPorPagina = 15)
        {
            return await ObtenerBitacoraModuloPaginadoAsync(
                "BitacoraVentas",
                "IdBitacoraVentas",
                pagina,
                registrosPorPagina);
        }

        public async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraVisitasAsync(
    int pagina = 1,
    int registrosPorPagina = 15)
        {
            return await ObtenerBitacoraModuloPaginadoAsync(
                "BitacoraVisitas",
                "IdBitacoraVisitas",
                pagina,
                registrosPorPagina);
        }

        public async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraMantenimientoAsync(
     int pagina = 1,
     int registrosPorPagina = 15)
        {
            return await ObtenerBitacoraModuloPaginadoAsync(
                "BitacoraMantenimiento",
                "IdBitacoraMantenimiento",
                pagina,
                registrosPorPagina);
        }

        private async Task<Paginacion<BitacoraModulo>> ObtenerBitacoraModuloPaginadoAsync(
    string tabla,
    string idCampo,
    int pagina = 1,
    int registros = 15)
        {
            Paginacion<BitacoraModulo> resultado = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            // Total de registros
            string sqlCount = $"SELECT COUNT(*) FROM {tabla}";

            await using (SqlCommand countCommand = new SqlCommand(sqlCount, connection))
            {
                resultado.TotalRegistros = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            }

            int offset = (pagina - 1) * registros;

            string sql = $@"
SELECT
    B.{idCampo} AS IdBitacora,
    B.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    B.Procedimiento,
    B.Operacion,
    B.RegistroAfectado,
    B.Detalle,
    B.FechaHora,
    B.Resultado
FROM {tabla} B
LEFT JOIN Usuarios U
    ON U.IdUsuario = B.IdUsuario
ORDER BY B.FechaHora DESC
OFFSET @Offset ROWS
FETCH NEXT @Registros ROWS ONLY;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Offset", offset);
            command.Parameters.AddWithValue("@Registros", registros);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resultado.Datos.Add(new BitacoraModulo
                {
                    IdBitacora = Convert.ToInt32(reader["IdBitacora"]),
                    IdUsuario = reader["IdUsuario"] == DBNull.Value ? null : Convert.ToInt32(reader["IdUsuario"]),
                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",
                    Procedimiento = reader["Procedimiento"]?.ToString() ?? "",
                    Operacion = reader["Operacion"]?.ToString() ?? "",
                    RegistroAfectado = reader["RegistroAfectado"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["RegistroAfectado"]),
                    Detalle = reader["Detalle"]?.ToString(),
                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                    Resultado = reader["Resultado"]?.ToString() ?? ""
                });
            }

            resultado.PaginaActual = pagina;
            resultado.RegistrosPorPagina = registros;

            return resultado;
        }














        public async Task<BitacoraGeneral?> ObtenerBitacoraPorIdAsync(int id)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sql = @"
SELECT
    B.IdBitacora,
    B.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    B.Modulo,
    B.TablaAfectada,
    B.TipoOperacion,
    B.RegistroAfectado,
    B.ValorAnterior,
    B.ValorNuevo,
    B.FechaHora,
    B.Resultado
FROM BitacoraGeneral B
LEFT JOIN Usuarios U
    ON U.IdUsuario = B.IdUsuario
WHERE B.IdBitacora = @Id;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new BitacoraGeneral
                {
                    IdBitacora = Convert.ToInt32(reader["IdBitacora"]),
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",
                    Modulo = reader["Modulo"]?.ToString() ?? "",
                    TablaAfectada = reader["TablaAfectada"]?.ToString() ?? "",
                    TipoOperacion = reader["TipoOperacion"]?.ToString() ?? "",
                    RegistroAfectado = Convert.ToInt32(reader["RegistroAfectado"]),
                    ValorAnterior = reader["ValorAnterior"] == DBNull.Value
                        ? null
                        : reader["ValorAnterior"].ToString(),
                    ValorNuevo = reader["ValorNuevo"] == DBNull.Value
                        ? null
                        : reader["ValorNuevo"].ToString(),
                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                    Resultado = reader["Resultado"]?.ToString() ?? ""
                };
            }

            return null;
        }




        

        public async Task<List<BitacoraModulo>> ObtenerBitacoraErroresAsync()
        {
            List<BitacoraModulo> lista = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sql = @"
SELECT
    E.IdError AS IdBitacora,
    E.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    E.Procedimiento,
    CAST(E.NumeroError AS VARCHAR(20)) AS Operacion,
    NULL AS RegistroAfectado,
    E.Descripcion AS Detalle,
    E.FechaHora,
    CASE
        WHEN E.RollbackEjecutado = 1 THEN 'ROLLBACK'
        ELSE 'ERROR'
    END AS Resultado
FROM BitacoraErrores E
LEFT JOIN Usuarios U
    ON U.IdUsuario = E.IdUsuario
ORDER BY E.FechaHora DESC";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new BitacoraModulo
                {
                    IdBitacora = Convert.ToInt32(reader["IdBitacora"]),
                    IdUsuario = reader["IdUsuario"] == DBNull.Value ? null : Convert.ToInt32(reader["IdUsuario"]),
                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",
                    Procedimiento = reader["Procedimiento"]?.ToString() ?? "",
                    Operacion = reader["Operacion"]?.ToString() ?? "",
                    RegistroAfectado = null,
                    Detalle = reader["Detalle"]?.ToString(),
                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                    Resultado = reader["Resultado"]?.ToString() ?? ""
                });
            }

            return lista;
        }




        public async Task<Paginacion<BitacoraError>> ObtenerBitacoraErroresAsync(
    int pagina = 1,
    int registrosPorPagina = 15)
        {
            Paginacion<BitacoraError> resultado = new();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await connection.OpenAsync();

            const string sqlTotal = @"
SELECT COUNT(*)
FROM BitacoraErrores;";

            await using (SqlCommand totalCommand =
                new SqlCommand(sqlTotal, connection))
            {
                resultado.TotalRegistros =
                    Convert.ToInt32(await totalCommand.ExecuteScalarAsync());
            }

            resultado.PaginaActual = pagina;
            resultado.RegistrosPorPagina = registrosPorPagina;

            int offset = (pagina - 1) * registrosPorPagina;

            const string sql = @"
SELECT
    B.IdError,
    B.IdUsuario,
    ISNULL(U.Nombre + ' ' + U.Apellido,'Sistema') AS NombreUsuario,
    B.Procedimiento,
    B.NumeroError,
    B.Descripcion,
    B.FechaHora,
    B.RollbackEjecutado
FROM BitacoraErrores B
LEFT JOIN Usuarios U
ON U.IdUsuario = B.IdUsuario
ORDER BY B.FechaHora DESC
OFFSET @Offset ROWS
FETCH NEXT @Registros ROWS ONLY;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Offset", offset);
            command.Parameters.AddWithValue("@Registros", registrosPorPagina);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resultado.Datos.Add(new BitacoraError
                {
                    IdError = Convert.ToInt32(reader["IdError"]),

                    IdUsuario = reader["IdUsuario"] == DBNull.Value
        ? null
        : Convert.ToInt32(reader["IdUsuario"]),

                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? "Sistema",

                    Procedimiento = reader["Procedimiento"]?.ToString() ?? "",

                    NumeroError = Convert.ToInt32(reader["NumeroError"]),

                    Descripcion = reader["Descripcion"]?.ToString() ?? "",

                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),

                    RollbackEjecutado = Convert.ToBoolean(reader["RollbackEjecutado"])
                });
            }

            return resultado;
        }









    }


    
    }