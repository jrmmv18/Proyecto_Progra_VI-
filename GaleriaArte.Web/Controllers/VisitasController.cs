using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class VisitasController : Controller
    {
        private readonly VisitaRepository _visitaRepository;

        public VisitasController(
            VisitaRepository visitaRepository)
        {
            _visitaRepository = visitaRepository;
        }

        // =====================================================
        // LISTAR VISITAS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1)
        {
            List<Visita> visitas =
                await _visitaRepository.ObtenerTodosAsync();

            IEnumerable<Visita> filtradas = visitas
                .Where(visita => FiltroBusqueda.Coincide(
                    busqueda,
                    visita.NombreCliente,
                    visita.ApellidoCliente,
                    visita.CorreoCliente,
                    visita.Observaciones))
                .Where(visita => FiltroBusqueda.EnRango(
                    visita.FechaIngreso,
                    fechaInicio,
                    fechaFin));

            PaginacionInfo paginacion = new()
            {
                PaginaActual = pagina,
                Busqueda = busqueda,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Controlador = "Visitas",
                EtiquetaFechas = "Entrada",
                PlaceholderBusqueda =
                    "Nombre o apellido del visitante"
            };

            return View(
                ListaPaginada<Visita>.Crear(filtradas, paginacion));
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE ENTRADA
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new VisitaCrearViewModel
            {
                FechaIngreso = DateTime.Now
            });
        }

        // =====================================================
        // REGISTRAR ENTRADA DEL VISITANTE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            VisitaCrearViewModel modelo)
        {
            // La salida no puede ser anterior a la entrada
            if (modelo.FechaSalida.HasValue &&
                modelo.FechaSalida.Value < modelo.FechaIngreso)
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaSalida),
                    "La salida no puede ser anterior a la entrada.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            int idUsuario = ObtenerIdUsuario();

            try
            {
                await _visitaRepository.RegistrarVisitanteAsync(
                    modelo,
                    idUsuario);

                TempData["Mensaje"] =
                    "Visitante y entrada registrados correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(modelo);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al registrar la entrada.");

                return View(modelo);
            }
        }

        // =====================================================
        // INGRESAR UN CLIENTE QUE YA EXISTE
        //
        // Se llega desde el modulo de Clientes: el visitante
        // ya esta registrado, solo se escoge de la lista y se
        // marca su entrada.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Ingresar(int? id)
        {
            await CargarClientesAsync(id);

            return View(new Visita
            {
                IdCliente = id ?? 0,
                FechaIngreso = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ingresar(Visita visita)
        {
            if (!ModelState.IsValid)
            {
                await CargarClientesAsync(visita.IdCliente);

                return View(visita);
            }

            int idUsuario = ObtenerIdUsuario();

            try
            {
                await _visitaRepository.RegistrarEntradaAsync(
                    visita,
                    idUsuario);

                TempData["Mensaje"] =
                    "Entrada del visitante registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                await CargarClientesAsync(visita.IdCliente);

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(visita);
            }
            catch (Exception)
            {
                await CargarClientesAsync(visita.IdCliente);

                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al registrar la entrada.");

                return View(visita);
            }
        }

        // =====================================================
        // REGISTRAR SALIDA DEL VISITANTE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida(int id)
        {
            try
            {
                Visita? visita =
                    await _visitaRepository.ObtenerPorIdAsync(id);

                if (visita == null)
                {
                    return NotFound();
                }

                if (!visita.EnCurso)
                {
                    TempData["Mensaje"] =
                        "La salida de este visitante ya estaba registrada.";

                    return RedirectToAction(nameof(Index));
                }

                DateTime fechaSalida = DateTime.Now;

                // La salida nunca puede ser anterior a la entrada
                if (fechaSalida < visita.FechaIngreso)
                {
                    fechaSalida = visita.FechaIngreso;
                }

                bool registrada =
                    await _visitaRepository.RegistrarSalidaAsync(
                        id,
                        fechaSalida,
                        ObtenerIdUsuario());

                if (!registrada)
                {
                    return NotFound();
                }

                TempData["Mensaje"] =
                    "Salida del visitante registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["Mensaje"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Mensaje"] =
                    "Ocurrió un error al registrar la salida del visitante.";

                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Visita? visita =
                await _visitaRepository.ObtenerPorIdAsync(id);

            if (visita == null)
            {
                return NotFound();
            }

            await CargarClientesAsync(visita.IdCliente);

            return View(visita);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Visita visita)
        {
            if (id != visita.IdVisita)
            {
                return BadRequest();
            }

            // La salida no puede ser anterior a la entrada
            if (visita.FechaSalida.HasValue &&
                visita.FechaSalida.Value < visita.FechaIngreso)
            {
                ModelState.AddModelError(
                    nameof(visita.FechaSalida),
                    "La salida no puede ser anterior a la entrada.");
            }

            if (!ModelState.IsValid)
            {
                await CargarClientesAsync(visita.IdCliente);

                return View(visita);
            }

            int idUsuario = ObtenerIdUsuario();

            try
            {
                bool actualizada =
                    await _visitaRepository.ActualizarAsync(
                        visita,
                        idUsuario);

                if (!actualizada)
                {
                    return NotFound();
                }

                TempData["Mensaje"] =
                    "Visita actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                await CargarClientesAsync(visita.IdCliente);

                ModelState.AddModelError(string.Empty, ex.Message);

                return View(visita);
            }
            catch (Exception)
            {
                await CargarClientesAsync(visita.IdCliente);

                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al actualizar la visita.");

                return View(visita);
            }
        }

        // =====================================================
        // CARGAR CLIENTES ACTIVOS
        // =====================================================

        private async Task CargarClientesAsync(
            int? clienteSeleccionado = null)
        {
            List<Cliente> clientes =
                await _visitaRepository.ObtenerClientesActivosAsync();

            ViewBag.Clientes = new SelectList(
                clientes,
                "IdCliente",
                "NombreCompleto",
                clienteSeleccionado
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
