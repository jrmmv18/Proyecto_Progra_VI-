using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly ClienteRepository _clienteRepository;

        public ClientesController(
            ClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        // =====================================================
        // LISTAR CLIENTES
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Cliente> clientes =
                await _clienteRepository.ObtenerTodosAsync();

            return View(clientes);
        }

        // =====================================================
        // CREAR CLIENTE
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Cliente model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int? idUsuario =
                    ObtenerIdUsuario();

                await _clienteRepository.CrearAsync(
                    model,
                    idUsuario
                );

                TempData["Mensaje"] =
                    "Cliente registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ObtenerMensajeSql(ex)
                );

                return View(model);
            }
        }

        // =====================================================
        // EDITAR CLIENTE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            Cliente? cliente =
                await _clienteRepository
                    .ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Cliente model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int? idUsuario =
                    ObtenerIdUsuario();

                await _clienteRepository
                    .ActualizarAsync(
                        model,
                        idUsuario
                    );

                TempData["Mensaje"] =
                    "Cliente actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ObtenerMensajeSql(ex)
                );

                return View(model);
            }
        }

        // =====================================================
        // CAMBIAR ESTADO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id)
        {
            Cliente? cliente =
                await _clienteRepository
                    .ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            try
            {
                int? idUsuario =
                    ObtenerIdUsuario();

                await _clienteRepository
                    .CambiarEstadoAsync(
                        id,
                        !cliente.Estado,
                        idUsuario
                    );

                TempData["Mensaje"] =
                    "Estado del cliente actualizado.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["Error"] =
                    ObtenerMensajeSql(ex);

                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // OBTENER USUARIO
        // =====================================================

        private int? ObtenerIdUsuario()
        {
            string? idUsuario =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (int.TryParse(idUsuario, out int id))
            {
                return id;
            }

            return null;
        }

        // =====================================================
        // MENSAJES SQL
        // =====================================================

        private static string ObtenerMensajeSql(
            SqlException exception)
        {
            if (exception.Number >= 50000)
            {
                return exception.Message;
            }

            return
                "No fue posible completar la operación.";
        }
    }
}