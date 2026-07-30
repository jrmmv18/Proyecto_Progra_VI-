using GaleriaArte.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GaleriaArte.Web.Data
{
    public class DashboardRepository
    {
        private readonly DatabaseConnection _databaseConnection;

        public DashboardRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public async Task<DashboardViewModel> ObtenerResumenAsync()
        {
            var dashboard = new DashboardViewModel();

            using SqlConnection connection =
                _databaseConnection.CreateConnection();

            using SqlCommand command =
                new SqlCommand("sp_Dashboard_Resumen", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                dashboard.TotalObras =
                    Convert.ToInt32(reader["TotalObras"]);

                dashboard.TotalArtistas =
                    Convert.ToInt32(reader["TotalArtistas"]);

                dashboard.TotalProductos =
                    Convert.ToInt32(reader["TotalProductos"]);

                dashboard.TotalProveedores =
                    Convert.ToInt32(reader["TotalProveedores"]);

                dashboard.TotalVentas =
                    Convert.ToInt32(reader["TotalVentas"]);

                dashboard.TotalMantenimientos =
                    Convert.ToInt32(reader["TotalMantenimientos"]);

                dashboard.TotalVisitas =
                    Convert.ToInt32(reader["TotalVisitas"]);

                dashboard.VisitasEnCurso =
                    Convert.ToInt32(reader["VisitasEnCurso"]);
            }

            return dashboard;
        }
    }
}