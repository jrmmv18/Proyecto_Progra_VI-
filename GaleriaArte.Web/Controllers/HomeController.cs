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
        private readonly DashboardRepository _dashboardRepository;

        public HomeController(
     ILogger<HomeController> logger,
     DatabaseConnection databaseConnection,
     DashboardRepository dashboardRepository)
        {
            _logger = logger;
            _databaseConnection = databaseConnection;
            _dashboardRepository = dashboardRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboard = await _dashboardRepository.ObtenerResumenAsync();

                dashboard.Usuario =
                    User.FindFirst("NombreCompleto")?.Value
                    ?? User.Identity?.Name
                    ?? "";

                dashboard.Rol =
                    User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? "";

                return View(dashboard);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorConexion = ex.Message;

                return View(new DashboardViewModel());
            }
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