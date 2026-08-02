using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // LISTAR CLIENTES
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Cliente> clientes =
                await _clienteRepository.ObtenerTodosAsync();

            return View(clientes);
        }

        // MOSTRAR FORMULARIO DE CREACIÓN
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Cliente
            {
                Estado = true
            });
        }

        // GUARDAR NUEVO CLIENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            await _clienteRepository.CrearAsync(cliente);

            TempData["Mensaje"] =
                "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // MOSTRAR FORMULARIO DE EDICIÓN
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Cliente? cliente =
                await _clienteRepository.ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GUARDAR CAMBIOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            Cliente? clienteExistente =
                await _clienteRepository.ObtenerPorIdAsync(id);

            if (clienteExistente == null)
            {
                return NotFound();
            }

            await _clienteRepository.ActualizarAsync(cliente);

            TempData["Mensaje"] =
                "Cliente actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ACTIVAR O DESACTIVAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            Cliente? cliente =
                await _clienteRepository.ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            bool nuevoEstado = !cliente.Estado;

            await _clienteRepository.CambiarEstadoAsync(id);

            TempData["Mensaje"] = nuevoEstado
                ? "Cliente activado correctamente."
                : "Cliente desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}