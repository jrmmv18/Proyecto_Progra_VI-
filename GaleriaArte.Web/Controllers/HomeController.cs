using System.Diagnostics;
using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseConnection _databaseConnection;

        public HomeController(
            ILogger<HomeController> logger,
            DatabaseConnection databaseConnection)
        {
            _logger = logger;
            _databaseConnection = databaseConnection;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                await using SqlConnection connection =
                    _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                ViewBag.EstadoConexion = "Conexión exitosa";
                ViewBag.BaseDatos = connection.Database;
                ViewBag.Servidor = connection.DataSource;
            }
            catch (Exception ex)
            {
                ViewBag.EstadoConexion = "Error de conexión";
                ViewBag.ErrorConexion = ex.Message;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                                ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}