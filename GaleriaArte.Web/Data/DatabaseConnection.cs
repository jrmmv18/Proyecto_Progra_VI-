using Microsoft.Data.SqlClient;

namespace GaleriaArte.Web.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GaleriaArteDB")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión GaleriaArteDB.");
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}