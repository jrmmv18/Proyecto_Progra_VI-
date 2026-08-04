using GaleriaArte.Web.Data;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    public class BitacorasController : Controller
    {
        private readonly BitacoraRepository _repository;

        public BitacorasController(BitacoraRepository repository)
        {
            _repository = repository;
        }

        // ==========================
        // Bitácora General
        // ==========================

        public async Task<IActionResult> Index(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraGeneralAsync(pagina, 15);

            return View(modelo);
        }
        // ==========================
        // Bitácoras por módulo
        // ==========================

        public async Task<IActionResult> Inventario(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraInventarioAsync(pagina, 15);

            ViewBag.Modulo = "Inventario";

            return View("Modulo", modelo);
        }

        public async Task<IActionResult> Obras(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraObrasAsync(pagina, 15);

            ViewBag.Modulo = "Obras";

            return View("Modulo", modelo);
        }

        public async Task<IActionResult> Seguridad(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraSeguridadAsync(pagina, 15);

            ViewBag.Modulo = "Seguridad";

            return View("Modulo", modelo);
        }

        public async Task<IActionResult> Ventas(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraVentasAsync(pagina, 15);

            ViewBag.Modulo = "Ventas";

            return View("Modulo", modelo);
        }

        public async Task<IActionResult> Visitas(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraVisitasAsync(pagina, 15);

            ViewBag.Modulo = "Visitas";

            return View("Modulo", modelo);
        }

        public async Task<IActionResult> Mantenimiento(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraMantenimientoAsync(pagina, 15);

            ViewBag.Modulo = "Mantenimiento";

            return View("Modulo", modelo);
        }


        // ==========================
        // Detalle
        // ==========================

        public async Task<IActionResult> Detalle(int id)
        {
            var modelo = await _repository.ObtenerBitacoraPorIdAsync(id);

            if (modelo == null)
                return NotFound();

            return View(modelo);
        }

        public async Task<IActionResult> Errores(int pagina = 1)
        {
            var modelo = await _repository.ObtenerBitacoraErroresAsync(pagina, 15);

            ViewBag.Modulo = "Errores";

            return View(modelo);
        }
    }
}