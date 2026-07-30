using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class ArtistasController : Controller
    {
        private readonly ArtistaRepository _artistaRepository;

        public ArtistasController(ArtistaRepository artistaRepository)
        {
            _artistaRepository = artistaRepository;
        }

        // LISTAR ARTISTAS
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Artista> artistas =
                await _artistaRepository.ObtenerTodosAsync();

            return View(artistas);
        }

        // MOSTRAR FORMULARIO DE CREACIÓN
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Artista
            {
                Estado = true
            });
        }

        // GUARDAR NUEVO ARTISTA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artista artista)
        {
            if (!ModelState.IsValid)
            {
                return View(artista);
            }

            await _artistaRepository.CrearAsync(
             artista,
            ObtenerIdUsuario()
            );

            TempData["Mensaje"] =
                "Artista registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // MOSTRAR FORMULARIO DE EDICIÓN
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Artista? artista =
                await _artistaRepository.ObtenerPorIdAsync(id);

            if (artista == null)
            {
                return NotFound();
            }

            return View(artista);
        }

        // GUARDAR CAMBIOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Artista artista)
        {
            if (id != artista.IdArtista)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(artista);
            }

            bool actualizado =
            await _artistaRepository.ActualizarAsync(
             artista,
             ObtenerIdUsuario()
            );

            if (!actualizado)
            {
                return NotFound();
            }

            TempData["Mensaje"] =
                "Artista actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }



        // ACTIVAR O DESACTIVAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            Artista? artista =
                await _artistaRepository.ObtenerPorIdAsync(id);

            if (artista == null)
            {
                return NotFound();
            }

            bool nuevoEstado = !artista.Estado;

            await _artistaRepository.CambiarEstadoAsync(
                id,
                nuevoEstado,
                ObtenerIdUsuario()
            );

            TempData["Mensaje"] = nuevoEstado
                ? "Artista activado correctamente."
                : "Artista desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private int? ObtenerIdUsuario()
        {
            string? idUsuario =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;

            if (int.TryParse(idUsuario, out int id))
            {
                return id;
            }

            return null;
        }
    }
}