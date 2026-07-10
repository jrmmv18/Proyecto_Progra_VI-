using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class ObrasController : Controller
    {
        private readonly ObraRepository _obraRepository;

        public ObrasController(ObraRepository obraRepository)
        {
            _obraRepository = obraRepository;
        }


        private int? ObtenerIdUsuario()
        {
            string? idUsuario =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(idUsuario, out int id))
            {
                return id;
            }

            return null;
        }




        // =====================================================
        // LISTAR OBRAS
        // GET: /Obras
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Obra> obras =
                await _obraRepository.ObtenerTodosAsync();

            return View(obras);
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE CREACIÓN
        // GET: /Obras/Create
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListasAsync();

            return View(new Obra());
        }

        // =====================================================
        // GUARDAR NUEVA OBRA
        // POST: /Obras/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Obra obra)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync(
                    obra.IdArtista,
                    obra.IdCategoria
                );

                return View(obra);
            }

            await _obraRepository.CrearAsync(
     obra,
     ObtenerIdUsuario()
 );

            TempData["Mensaje"] =
                "Obra registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN
        // GET: /Obras/Edit/5
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Obra? obra =
                await _obraRepository.ObtenerPorIdAsync(id);

            if (obra == null)
            {
                return NotFound();
            }

            await CargarListasAsync(
                obra.IdArtista,
                obra.IdCategoria
            );

            return View(obra);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // POST: /Obras/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Obra obra)
        {
            if (id != obra.IdObra)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(
                    obra.IdArtista,
                    obra.IdCategoria
                );

                return View(obra);
            }

            await _obraRepository.ActualizarAsync(
    obra,
    ObtenerIdUsuario()
);

            TempData["Mensaje"] =
                "Obra actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // CAMBIAR ESTADO
        // POST: /Obras/CambiarEstado
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            Obra? obra = await _obraRepository.ObtenerPorIdAsync(id);

            if (obra == null)
            {
                return NotFound();
            }

            bool nuevoEstado = !obra.Estado;

            await _obraRepository.CambiarEstadoAsync(
                id,
                nuevoEstado,
                ObtenerIdUsuario()
            );

            TempData["Mensaje"] = nuevoEstado
                ? "Obra activada correctamente."
                : "Obra desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // CARGAR ARTISTAS Y CATEGORÍAS ACTIVAS
        // =====================================================

        private async Task CargarListasAsync(
            int? idArtistaSeleccionado = null,
            int? idCategoriaSeleccionada = null)
        {
            List<Artista> artistas =
                await _obraRepository.ObtenerArtistasActivosAsync();

            List<Categoria> categorias =
                await _obraRepository.ObtenerCategoriasActivasAsync();

            ViewBag.Artistas = new SelectList(
                artistas,
                "IdArtista",
                "NombreCompleto",
                idArtistaSeleccionado
            );

            ViewBag.Categorias = new SelectList(
                categorias,
                "IdCategoria",
                "Nombre",
                idCategoriaSeleccionada
            );
        }
    }
}