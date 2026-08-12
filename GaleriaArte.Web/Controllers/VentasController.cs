using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly FacturaRepository _facturaRepository;

        public VentasController(
            FacturaRepository facturaRepository)
        {
            _facturaRepository = facturaRepository;
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE VENTA
        // GET: /Ventas
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            VentaCrearViewModel model =
                new VentaCrearViewModel();

            await CargarDatosAsync(model);

            return View(model);
        }

        // =====================================================
        // REGISTRAR VENTA
        // POST: /Ventas
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            VentaCrearViewModel model)
        {
            model.Productos ??=
                new List<DetalleVentaCrearViewModel>();

            // Se descartan las lineas vacias y se suman las
            // cantidades cuando el mismo producto viene repetido.
            model.Productos = model.Productos
                .Where(producto =>
                    producto.IdProducto > 0 &&
                    producto.Cantidad > 0)
                .GroupBy(producto => producto.IdProducto)
                .Select(grupo => new DetalleVentaCrearViewModel
                {
                    IdProducto = grupo.Key,
                    Cantidad = grupo.Sum(p => p.Cantidad),
                    PrecioUnitario = grupo.First().PrecioUnitario
                })
                .ToList();

            if (model.Productos.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar al menos un producto."
                );
            }

            int? idUsuario =
                ObtenerIdUsuario();

            if (!idUsuario.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible identificar al usuario que realiza la venta."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync(model);

                return View(model);
            }

            try
            {
                int idFactura =
                    await _facturaRepository.CrearAsync(
                        model,
                        idUsuario!.Value
                    );

                TempData["Mensaje"] =
                    $"Venta registrada correctamente. Factura #{idFactura}.";

                return RedirectToAction(
                    nameof(Detalle),
                    new
                    {
                        id = idFactura
                    }
                );
            }
            catch (SqlException ex)
            {
                await CargarDatosAsync(model);

                ModelState.AddModelError(
                    string.Empty,
                    ObtenerMensajeSql(ex)
                );

                return View(model);
            }
            catch (Exception)
            {
                await CargarDatosAsync(model);

                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al registrar la venta."
                );

                return View(model);
            }
        }

        // =====================================================
        // HISTORIAL DE VENTAS
        // GET: /Ventas/Historial
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Historial()
        {
            List<Factura> facturas =
                await _facturaRepository
                    .ObtenerFacturasAsync();

            return View(facturas);
        }

        // =====================================================
        // DETALLE DE FACTURA
        // GET: /Ventas/Detalle/5
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Detalle(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            Factura? factura =
                await _facturaRepository
                    .ObtenerFacturaPorIdAsync(id);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // =====================================================
        // ANULAR FACTURA
        // POST: /Ventas/Anular/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                int? idUsuario =
                    ObtenerIdUsuario();

                await _facturaRepository
                    .AnularFacturaAsync(
                        id,
                        idUsuario
                    );

                TempData["Mensaje"] =
                    $"La factura #{id} fue anulada correctamente.";

                return RedirectToAction(
                    nameof(Detalle),
                    new
                    {
                        id
                    }
                );
            }
            catch (SqlException ex)
            {
                TempData["Error"] =
                    ObtenerMensajeSql(ex);

                return RedirectToAction(
                    nameof(Detalle),
                    new
                    {
                        id
                    }
                );
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Ocurrió un error inesperado al anular la factura.";

                return RedirectToAction(
                    nameof(Detalle),
                    new
                    {
                        id
                    }
                );
            }
        }

        // =====================================================
        // CARGAR VISITANTES EN GALERIA Y PRODUCTOS
        //
        // Solo se ofrecen los clientes con visita en curso,
        // porque la factura pertenece a una visita.
        // =====================================================

        private async Task CargarDatosAsync(
            VentaCrearViewModel model)
        {
            model.ClientesDisponibles =
                await _facturaRepository
                    .ObtenerClientesEnGaleriaAsync();

            model.ProductosDisponibles =
                await _facturaRepository
                    .ObtenerProductosDisponiblesAsync();
        }

        // =====================================================
        // OBTENER USUARIO AUTENTICADO
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
        // LIMPIAR MENSAJES DE SQL SERVER
        // =====================================================

        private static string ObtenerMensajeSql(
            SqlException exception)
        {
            if (exception.Number >= 50000)
            {
                return exception.Message;
            }

            return
                "No fue posible completar la operación. Verifique la información e inténtelo nuevamente.";
        }
    }
}