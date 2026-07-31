using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using System.Data;

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
            string? idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        public async Task<IActionResult> Index(
            string? busqueda,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1)
        {
            List<Obra> obras = await _obraRepository.ObtenerTodosAsync();

            IEnumerable<Obra> filtradas = obras
                .Where(obra => FiltroBusqueda.Coincide(
                    busqueda,
                    obra.Nombre,
                    obra.Codigo,
                    obra.NombreArtista,
                    obra.NombreCategoria))
                .Where(obra => FiltroBusqueda.EnRango(
                    obra.FechaIngresoGaleria,
                    fechaInicio,
                    fechaFin));

            PaginacionInfo paginacion = new()
            {
                PaginaActual = pagina,
                Busqueda = busqueda,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Controlador = "Obras",
                EtiquetaFechas = "Ingreso",
                PlaceholderBusqueda = "Nombre de obra, código, artista o categoría"
            };

            return View(ListaPaginada<Obra>.Crear(filtradas, paginacion));
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE CREACIÓN / MANTENIMIENTO
        // GET: /Obras/Create
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create(int? id = null, string? codigoMantenimiento = null)
        {
            await CargarListasAsync();

            Obra modeloObra = new Obra();

            // Si en la URL viaja un ID y un Código, significa que entramos desde el botón de mantenimiento
            if (id.HasValue && !string.IsNullOrEmpty(codigoMantenimiento))
            {
                modeloObra.IdObra = id.Value;
                modeloObra.Codigo = codigoMantenimiento;

                // Opcional: Puedes precargar datos de la obra original si lo necesitas en el formulario
                Obra? original = await _obraRepository.ObtenerPorIdAsync(id.Value);
                if (original != null)
                {
                    modeloObra.Nombre = original.Nombre;
                    modeloObra.IdArtista = original.IdArtista;
                    modeloObra.IdCategoria = original.IdCategoria;
                    modeloObra.ValorEstimado = original.ValorEstimado;
                }
            }

            return View(modeloObra);
        }

        // =====================================================
        // GUARDAR REGISTRO DE NUEVA OBRA / MANTENIMIENTO
        // POST: /Obras/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Obra obra)
        {
            bool esNuevaObra = obra.IdObra == 0;

            /*
             * El código sigue siendo obligatorio en el modelo y en SQL Server,
             * pero no debe ser digitado por el usuario al registrar una obra nueva.
             * Por eso se excluye únicamente de la validación del formulario Create.
             */
            if (esNuevaObra)
            {
                ModelState.Remove(nameof(Obra.Codigo));
            }

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
                return View(obra);
            }

            try
            {
                if (esNuevaObra)
                {
                    // sp_Obras_Crear debe generar y guardar el código automáticamente.
                    await _obraRepository.CrearAsync(
                        obra,
                        ObtenerIdUsuario()
                    );

                    TempData["Mensaje"] =
                        "Obra registrada correctamente con código automático.";
                }
                else
                {
                    /*
                     * Se conserva el flujo especial de mantenimiento,
                     * donde ya existe un IdObra y un código asociado.
                     */
                    await _obraRepository.CrearConCodigoAutogeneradaAsync(
                        obra,
                        ObtenerIdUsuario()
                    );

                    TempData["Mensaje"] =
                        "Registro de mantenimiento guardado con éxito.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(obra);
            }
            catch (Exception)
            {
                await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al procesar el registro."
                );

                return View(obra);
            }
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN + CARGAR PRODUCTOS
        // GET: /Obras/Edit/5
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Obra? obra = await _obraRepository.ObtenerPorIdAsync(id);

            if (obra == null)
            {
                return NotFound();
            }

            await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
            ViewBag.Productos = await _obraRepository.ObtenerProductosInventarioAsync();

            return View(obra);
        }

        // =====================================================
        // GUARDAR CAMBIOS DE EDICIÓN
        // POST: /Obras/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Obra obra)
        {
            if (id != obra.IdObra)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
                ViewBag.Productos = await _obraRepository.ObtenerProductosInventarioAsync();
                return View(obra);
            }

            try
            {
                await _obraRepository.ActualizarAsync(obra, ObtenerIdUsuario());
                TempData["Mensaje"] = "Obra actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
                ViewBag.Productos = await _obraRepository.ObtenerProductosInventarioAsync();
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(obra);
            }
            catch (Exception)
            {
                await CargarListasAsync(obra.IdArtista, obra.IdCategoria);
                ViewBag.Productos = await _obraRepository.ObtenerProductosInventarioAsync();
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al actualizar la obra.");
                return View(obra);
            }
        }

        // =====================================================
        // ACCIÓN NUEVA: PROCESAR EL REBAJE DE INVENTARIO
        // POST: /Obras/RegistrarConsumoProducto
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarConsumoProducto(int IdObra, int IdProducto, int CantidadUsada)
        {
            try
            {
                await _obraRepository.RegistrarConsumoProductoAsync(IdObra, IdProducto, CantidadUsada);
                TempData["Mensaje"] = "¡Material rebajado del inventario correctamente!";
            }
            catch (SqlException ex)
            {
                TempData["Mensaje"] = "Error en inventario: " + ex.Message;
            }
            catch (Exception)
            {
                TempData["Mensaje"] = "Ocurrió un error inesperado al afectar el stock del producto.";
            }

            return RedirectToAction(nameof(Edit), new { id = IdObra });
        }

        // =====================================================
        // CAMBIAR ESTADO
        // POST: /Obras/CambiarEstado
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                Obra? obra = await _obraRepository.ObtenerPorIdAsync(id);
                if (obra == null)
                {
                    return NotFound();
                }

                bool nuevoEstado = !obra.Estado;
                await _obraRepository.CambiarEstadoAsync(id, nuevoEstado, ObtenerIdUsuario());

                TempData["Mensaje"] = nuevoEstado ? "Obra activada correctamente." : "Obra desactivada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["Mensaje"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Mensaje"] = "Ocurrió un error inesperado al cambiar el estado.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // MÉTODOS AUXILIARES: CARGA DE LISTAS EN SELECTS
        // =====================================================
        private async Task CargarListasAsync(int? idArtista = null, int? idCategoria = null)
        {
            var artistas = await _obraRepository.ObtenerArtistasActivosAsync();
            var categorias = await _obraRepository.ObtenerCategoriasActivasAsync();

            ViewBag.Artistas = new SelectList(artistas, "IdArtista", "Nombre", idArtista);
            ViewBag.Categorias = new SelectList(categorias, "IdCategoria", "Nombre", idCategoria);
        }
    }
}
