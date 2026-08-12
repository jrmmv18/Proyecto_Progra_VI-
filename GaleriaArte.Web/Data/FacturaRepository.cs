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
            // El procedimiento toma el precio actual de cada producto,
            // aqui solo viaja que se lleva y cuanto.
            string productosJson =
                JsonSerializer.Serialize(
                    venta.Productos
                        .Where(producto => producto.Cantidad > 0)
                        .Select(producto => new
                        {
                            producto.IdProducto,
                            producto.Cantidad
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
                "@ProductosJson",
                SqlDbType.NVarChar,
                -1
            ).Value = productosJson;

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
        // LISTAR PRODUCTOS DISPONIBLES
        // Procedimiento: sp_Productos_ListarDisponibles
        // =====================================================

        public async Task<List<Producto>>
            ObtenerProductosDisponiblesAsync()
        {
            List<Producto> productos =
                new List<Producto>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            await using SqlCommand command =
                new SqlCommand(
                    "sp_Productos_ListarDisponibles",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                productos.Add(new Producto
                {
                    IdProducto = reader.GetInt32(
                        reader.GetOrdinal("IdProducto")
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

                    Stock = reader.GetInt32(
                        reader.GetOrdinal("Stock")
                    ),

                    StockMinimo = reader.GetInt32(
                        reader.GetOrdinal("StockMinimo")
                    ),

                    Precio =
                        Convert.ToDecimal(
                            reader["Precio"]
                        ),

                    Estado = true,

                    NombreProveedor =
                        reader["NombreProveedor"]?.ToString()
                        ?? string.Empty
                });
            }

            return productos;
        }

        // =====================================================
        // CLIENTES QUE ESTAN DENTRO DE LA GALERIA
        // Solo ellos pueden comprar, porque la factura
        // pertenece a una visita en curso.
        // =====================================================

        public async Task<List<Cliente>>
            ObtenerClientesEnGaleriaAsync()
        {
            List<Cliente> clientes =
                new List<Cliente>();

            await using SqlConnection connection =
                _databaseConnection.CreateConnection();

            const string sql = @"
                SELECT DISTINCT
                    C.IdCliente,
                    C.Nombre,
                    C.Apellido,
                    C.Telefono,
                    C.Correo,
                    C.Direccion,
                    C.Estado,
                    C.FechaRegistro
                FROM Clientes AS C
                INNER JOIN Visitas AS V
                    ON V.IdCliente = C.IdCliente
                WHERE C.Estado = 1
                  AND V.FechaSalida IS NULL
                ORDER BY C.Nombre, C.Apellido;";

            await using SqlCommand command =
                new SqlCommand(sql, connection);

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

                            IdProducto =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "IdProducto"
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

                            CodigoProducto =
                                reader["CodigoProducto"]
                                    ?.ToString()
                                ?? string.Empty,

                            NombreProducto =
                                reader["NombreProducto"]
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

                IdVisita = reader.GetInt32(
                    reader.GetOrdinal("IdVisita")
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
                    ?? string.Empty,

                FechaIngresoVisita =
                    Convert.ToDateTime(
                        reader["FechaIngresoVisita"]
                    ),

                FechaSalidaVisita =
                    reader["FechaSalidaVisita"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["FechaSalidaVisita"]
                        )
            };
        }
    }
}