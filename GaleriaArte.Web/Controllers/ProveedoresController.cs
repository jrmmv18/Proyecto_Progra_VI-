using System.Security.Claims;
using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class ProveedoresController : Controller
    {
        private readonly ProveedorRepository _proveedorRepository;

        public ProveedoresController(
            ProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        // LISTAR PROVEEDORES
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Proveedor> proveedores =
                await _proveedorRepository.ObtenerTodosAsync();

            return View(proveedores);
        }

        // MOSTRAR FORMULARIO DE CREACIÓN
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Proveedor
            {
                Estado = true
            });
        }

        // GUARDAR NUEVO PROVEEDOR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            int idUsuario = ObtenerIdUsuario();

            await _proveedorRepository.CrearAsync(
                proveedor,
                idUsuario
            );

            TempData["Mensaje"] =
                "Proveedor registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // MOSTRAR FORMULARIO DE EDICIÓN
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Proveedor? proveedor =
                await _proveedorRepository.ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // GUARDAR CAMBIOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            int idUsuario = ObtenerIdUsuario();

            bool actualizado =
                await _proveedorRepository.ActualizarAsync(
                    proveedor,
                    idUsuario
                );

            if (!actualizado)
            {
                return NotFound();
            }

            TempData["Mensaje"] =
                "Proveedor actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ACTIVAR O DESACTIVAR PROVEEDOR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            Proveedor? proveedor =
                await _proveedorRepository.ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            bool nuevoEstado = !proveedor.Estado;

            int idUsuario = ObtenerIdUsuario();

            bool actualizado =
                await _proveedorRepository.CambiarEstadoAsync(
                    id,
                    nuevoEstado,
                    idUsuario
                );

            if (!actualizado)
            {
                return NotFound();
            }

            TempData["Mensaje"] = nuevoEstado
                ? "Proveedor activado correctamente."
                : "Proveedor desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // OBTENER ID DEL USUARIO AUTENTICADO
        private int ObtenerIdUsuario()
        {
            string? idUsuario =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value;

            if (int.TryParse(idUsuario, out int id))
            {
                return id;
            }

            throw new InvalidOperationException(
                "No se pudo obtener el usuario autenticado."
            );
        }
    }
}