using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseConnection _databaseConnection;
        private readonly DashboardRepository _dashboardRepository;
        // NUEVO: Declaramos el repositorio de mantenimiento para los contadores en tiempo real
        private readonly IMantenimientoRepository _mantenimientoRepository;

        // El constructor ahora recibe también tu repositorio de forma transparente
        public HomeController(
            ILogger<HomeController> logger,
            DatabaseConnection databaseConnection,
            DashboardRepository dashboardRepository,
            IMantenimientoRepository mantenimientoRepository)
        {
            _logger = logger;
            _databaseConnection = databaseConnection;
            _dashboardRepository = dashboardRepository;
            _mantenimientoRepository = mantenimientoRepository ?? throw new ArgumentNullException(nameof(mantenimientoRepository));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Ejecuta la consulta de resumen asíncrona original de tus compañeros
                var dashboard = await _dashboardRepository.ObtenerResumenAsync();

                // 2. SINCRONIZACIÓN INMEDIATA: Contamos las filas reales guardadas en la base de datos
                // y sobrescribimos el valor del modelo para que pinte la cifra real en el Dashboard
                var listaMantenimientos = _mantenimientoRepository.ListarTodos();
                dashboard.TotalMantenimientos = listaMantenimientos?.Count ?? 0;

                // 3. Mapeo de identidad original de tu grupo
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
