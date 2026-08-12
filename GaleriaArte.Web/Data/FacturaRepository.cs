using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace GaleriaArte.Web.Data
{
    public class FacturaRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public FacturaRepository(
            DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // =====================================================
        // CREAR FACTURA
        // Procedimiento: sp_Facturas_Crear
        // =====================================================

        public async Task<int> CrearAsync(
            VentaCrearViewModel venta,
            int idUsuario)
        {
            string obrasJson =
                JsonSerializer.Serialize(
                    venta.Obras.Select(obra => new
                    {
                        obra.IdObra,
                        obra.PrecioUnitario
                    })
                );

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Facturas_Crear",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdCliente",
                SqlDbType.Int
            ).Value = venta.IdCliente!.Value;

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value = idUsuario;

            command.Parameters.Add(
                "@MetodoPago",
                SqlDbType.VarChar,
                20
            ).Value = venta.MetodoPago;

            SqlParameter parametroImpuesto =
                command.Parameters.Add(
                    "@PorcentajeImpuesto",
                    SqlDbType.Decimal
                );

            parametroImpuesto.Precision = 5;
            parametroImpuesto.Scale = 4;
            parametroImpuesto.Value =
                venta.PorcentajeImpuesto;

            command.Parameters.Add(
                "@ObrasJson",
                SqlDbType.NVarChar,
                -1
            ).Value = obrasJson;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(
                    reader.GetOrdinal("IdFactura")
                );
            }

            throw new InvalidOperationException(
                "No fue posible obtener el número de factura."
            );
        }

        // =====================================================
        // LISTAR CLIENTES ACTIVOS
        // Procedimiento: sp_Clientes_ListarActivos
        // =====================================================

        public async Task<List<Cliente>>
            ObtenerClientesActivosAsync()
        {
            List<Cliente> clientes =
                new List<Cliente>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Clientes_ListarActivos",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clientes.Add(new Cliente
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
                });
            }

            return clientes;
        }

        // =====================================================
        // LISTAR OBRAS DISPONIBLES
        // Procedimiento: sp_Obras_ListarDisponibles
        // =====================================================

        public async Task<List<Obra>>
            ObtenerObrasDisponiblesAsync()
        {
            List<Obra> obras =
                new List<Obra>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Obras_ListarDisponibles",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                obras.Add(new Obra
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
                        reader["NombreArtista"]?.ToString()
                        ?? string.Empty,

                    NombreCategoria =
                        reader["NombreCategoria"]?.ToString()
                        ?? string.Empty
                });
            }

            return obras;
        }

        // =====================================================
        // LISTAR FACTURAS
        // Procedimiento: sp_Facturas_Listar
        // =====================================================

        public async Task<List<Factura>>
            ObtenerFacturasAsync()
        {
            List<Factura> facturas =
                new List<Factura>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Facturas_Listar",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                facturas.Add(
                    MapearFactura(reader)
                );
            }

            return facturas;
        }

        // =====================================================
        // OBTENER FACTURA CON DETALLE
        // Procedimiento: sp_Facturas_Obtener
        // =====================================================

        public async Task<Factura?>
            ObtenerFacturaPorIdAsync(
                int idFactura)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Facturas_Obtener",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdFactura",
                SqlDbType.Int
            ).Value = idFactura;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            Factura? factura = null;

            if (await reader.ReadAsync())
            {
                factura =
                    MapearFactura(reader);
            }

            if (factura == null)
            {
                return null;
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    factura.Detalles.Add(
                        new DetalleFactura
                        {
                            IdDetalleFactura =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdDetalleFactura"
                                    )
                                ),

                            IdFactura =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdFactura"
                                    )
                                ),

                            IdObra =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdObra"
                                    )
                                ),

                            Cantidad =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "Cantidad"
                                    )
                                ),

                            PrecioUnitario =
                                Convert.ToDecimal(
                                    reader[
                                        "PrecioUnitario"
                                    ]
                                ),

                            Subtotal =
                                Convert.ToDecimal(
                                    reader["Subtotal"]
                                ),

                            CodigoObra =
                                reader["CodigoObra"]
                                    ?.ToString()
                                ?? string.Empty,

                            NombreObra =
                                reader["NombreObra"]
                                    ?.ToString()
                                ?? string.Empty,

                            NombreArtista =
                                reader["NombreArtista"]
                                    ?.ToString()
                                ?? string.Empty
                        }
                    );
                }
            }

            return factura;
        }

        // =====================================================
        // ANULAR FACTURA
        // Procedimiento: sp_Facturas_Anular
        // =====================================================

        public async Task AnularFacturaAsync(
            int idFactura,
            int? idUsuario)
        {
            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Facturas_Anular",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@IdFactura",
                SqlDbType.Int
            ).Value = idFactura;

            command.Parameters.Add(
                "@IdUsuario",
                SqlDbType.Int
            ).Value =
                idUsuario.HasValue
                    ? idUsuario.Value
                    : DBNull.Value;

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // =====================================================
        // MÉTODO PRIVADO
        // =====================================================

        private static Factura MapearFactura(
            SqlDataReader reader)
        {
            return new Factura
            {
                IdFactura = reader.GetInt32(
                    reader.GetOrdinal("IdFactura")
                ),

                IdCliente = reader.GetInt32(
                    reader.GetOrdinal("IdCliente")
                ),

                IdUsuario = reader.GetInt32(
                    reader.GetOrdinal("IdUsuario")
                ),

                IdEstadoFactura = reader.GetInt32(
                    reader.GetOrdinal(
                        "IdEstadoFactura"
                    )
                ),

                FechaFactura =
                    Convert.ToDateTime(
                        reader["FechaFactura"]
                    ),

                MetodoPago =
                    reader["MetodoPago"]?.ToString()
                    ?? string.Empty,

                Subtotal =
                    Convert.ToDecimal(
                        reader["Subtotal"]
                    ),

                Impuesto =
                    Convert.ToDecimal(
                        reader["Impuesto"]
                    ),

                Total =
                    Convert.ToDecimal(
                        reader["Total"]
                    ),

                NombreCliente =
                    reader["NombreCliente"]?.ToString()
                    ?? string.Empty,

                NombreUsuario =
                    reader["NombreUsuario"]?.ToString()
                    ?? string.Empty,

                EstadoFactura =
                    reader["EstadoFactura"]?.ToString()
                    ?? string.Empty
            };
        }
    }
}