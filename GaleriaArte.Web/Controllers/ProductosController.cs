using System.Security.Claims;
using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly ProductoRepository _productoRepository;

        public ProductosController(
            ProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        // =====================================================
        // LISTAR PRODUCTOS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Producto> productos =
                await _productoRepository.ObtenerTodosAsync();

            return View(productos);
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE CREACIÓN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarProveedoresAsync();

            return View(new Producto
            {
                Estado = true
            });
        }

        // =====================================================
        // GUARDAR NUEVO PRODUCTO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Producto producto)
        {
            if (!ModelState.IsValid)
            {
                await CargarProveedoresAsync();

                return View(producto);
            }

            int idUsuario = ObtenerIdUsuario();

            await _productoRepository.CrearAsync(
                producto,
                idUsuario
            );

            TempData["Mensaje"] =
                "Producto registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Producto? producto =
                await _productoRepository.ObtenerPorIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            await CargarProveedoresAsync(producto.IdProveedor);

            return View(producto);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await CargarProveedoresAsync(producto.IdProveedor);

                return View(producto);
            }

            int idUsuario = ObtenerIdUsuario();

            bool actualizado =
                await _productoRepository.ActualizarAsync(
                    producto,
                    idUsuario
                );

            if (!actualizado)
            {
                return NotFound();
            }

            TempData["Mensaje"] =
                "Producto actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ACTIVAR / DESACTIVAR PRODUCTO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            Producto? producto =
                await _productoRepository.ObtenerPorIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            bool nuevoEstado = !producto.Estado;

            bool actualizado =
                await _productoRepository.CambiarEstadoAsync(
                    id,
                    nuevoEstado,
                    ObtenerIdUsuario()
                );

            if (!actualizado)
            {
                return NotFound();
            }

            TempData["Mensaje"] = nuevoEstado
                ? "Producto activado correctamente."
                : "Producto desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // CARGAR PROVEEDORES ACTIVOS
        // =====================================================

        private async Task CargarProveedoresAsync(
            int? proveedorSeleccionado = null)
        {
            List<Proveedor> proveedores =
                await _productoRepository.ObtenerProveedoresActivosAsync();

            ViewBag.Proveedores = new SelectList(
                proveedores,
                "IdProveedor",
                "Nombre",
                proveedorSeleccionado
            );
        }

        // =====================================================
        // OBTENER USUARIO AUTENTICADO
        // =====================================================

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