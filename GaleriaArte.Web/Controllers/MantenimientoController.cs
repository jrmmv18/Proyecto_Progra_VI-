using System;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class MantenimientoController : Controller
    {
        private readonly IMantenimientoRepository _mantenimientoRepository;
        private readonly string _connectionString;

        public MantenimientoController(
            IMantenimientoRepository mantenimientoRepository,
            IConfiguration configuration)
        {
            _mantenimientoRepository = mantenimientoRepository
                ?? throw new ArgumentNullException(
                    nameof(mantenimientoRepository));

            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            _connectionString =
                configuration.GetConnectionString("GaleriaArteDB")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'GaleriaArteDB'.");
        }

        // GET: Mantenimiento
        public IActionResult Index(
            string? busqueda,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1)
        {
            PaginacionInfo paginacion = new()
            {
                PaginaActual = pagina,
                Busqueda = busqueda,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Controlador = "Mantenimiento",
                EtiquetaFechas = "Mantenimiento",
                PlaceholderBusqueda = "Descripción del trabajo"
            };

            try
            {
                var listaMantenimientos =
                    _mantenimientoRepository.ListarTodos();

                var filtrados = listaMantenimientos
                    .Where(mantenimiento =>
                        FiltroBusqueda.Coincide(
                            busqueda,
                            mantenimiento.DescripcionTrabajo))
                    .Where(mantenimiento =>
                        FiltroBusqueda.EnRango(
                            mantenimiento.FechaMantenimiento,
                            fechaInicio,
                            fechaFin));

                return View(
                    ListaPaginada<Mantenimiento>.Crear(
                        filtrados,
                        paginacion));
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    $"No se pudo cargar el historial: {ex.Message}";

                return View(
                    ListaPaginada<Mantenimiento>.Crear(
                        new List<Mantenimiento>(),
                        paginacion));
            }
        }

        // GET: Mantenimiento/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            CargarListasFormulario();

            var nuevoMantenimiento = new Mantenimiento
            {
                FechaMantenimiento = DateTime.Now,

                // Crea inicialmente una fila para seleccionar producto.
                ProductosUsados = new List<ProductoUsadoMantenimiento>
                {
                    new ProductoUsadoMantenimiento()
                }
            };

            return View(nuevoMantenimiento);
        }

        // POST: Mantenimiento/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Mantenimiento mantenimiento)
        {
            try
            {
                AsignarUsuarioAutenticado(mantenimiento);

                // Eliminar filas completamente vacías creadas por el formulario.
                mantenimiento.ProductosUsados ??=
                    new List<ProductoUsadoMantenimiento>();

                mantenimiento.ProductosUsados =
                    mantenimiento.ProductosUsados
                        .Where(producto =>
                            producto.IdProducto > 0 ||
                            producto.CantidadUtilizada > 0)
                        .ToList();

                ValidarProductosUsados(mantenimiento);

                if (ModelState.IsValid)
                {
                    // Ahora el repositorio devuelve el IdMantenimiento.
                    int idMantenimiento =
                        _mantenimientoRepository
                            .RegistrarMantenimientoCompleto(
                                mantenimiento);

                    if (idMantenimiento <= 0)
                    {
                        throw new InvalidOperationException(
                            "No se pudo obtener el identificador del mantenimiento.");
                    }

                    foreach (var producto in mantenimiento.ProductosUsados)
                    {
                        _mantenimientoRepository.RegistrarProductoUsado(
                            idMantenimiento,
                            producto.IdProducto,
                            producto.CantidadUtilizada);
                    }

                    TempData["MensajeExito"] =
                        "El mantenimiento fue registrado correctamente y el inventario fue actualizado.";

                    return RedirectToAction(nameof(Index));
                }

                var errores = string.Join(
                    " | ",
                    ModelState.Values
                        .SelectMany(valor => valor.Errors)
                        .Select(error =>
                            !string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? error.ErrorMessage
                                : error.Exception?.Message)
                        .Where(mensaje =>
                            !string.IsNullOrWhiteSpace(mensaje)));

                if (!string.IsNullOrWhiteSpace(errores))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Validación fallida: {errores}");
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"No se pudo registrar el mantenimiento: {ex.Message}");
            }

            CargarListasFormulario();

            return View(mantenimiento);
        }

        private void AsignarUsuarioAutenticado(
            Mantenimiento mantenimiento)
        {
            string? userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userIdClaim) &&
                int.TryParse(
                    userIdClaim,
                    out int idUsuarioAutenticado))
            {
                mantenimiento.IdUsuario =
                    idUsuarioAutenticado;
            }
            else
            {
                // Valor temporal de respaldo.
                mantenimiento.IdUsuario = 1;
            }
        }

        private void ValidarProductosUsados(
            Mantenimiento mantenimiento)
        {
            if (mantenimiento.ProductosUsados == null)
            {
                return;
            }

            for (int i = 0;
                 i < mantenimiento.ProductosUsados.Count;
                 i++)
            {
                var producto =
                    mantenimiento.ProductosUsados[i];

                if (producto.IdProducto <= 0)
                {
                    ModelState.AddModelError(
                        $"ProductosUsados[{i}].IdProducto",
                        "Debe seleccionar un producto.");
                }

                if (producto.CantidadUtilizada <= 0)
                {
                    ModelState.AddModelError(
                        $"ProductosUsados[{i}].CantidadUtilizada",
                        "La cantidad utilizada debe ser mayor que cero.");
                }
            }

            var productosDuplicados =
                mantenimiento.ProductosUsados
                    .Where(producto =>
                        producto.IdProducto > 0)
                    .GroupBy(producto =>
                        producto.IdProducto)
                    .Where(grupo =>
                        grupo.Count() > 1)
                    .Select(grupo =>
                        grupo.Key)
                    .ToList();

            if (productosDuplicados.Any())
            {
                ModelState.AddModelError(
                    nameof(mantenimiento.ProductosUsados),
                    "No debe seleccionar el mismo producto más de una vez.");
            }
        }

        private void CargarListasFormulario()
        {
            CargarObrasDesdeBD();
            CargarProductosDisponibles();
        }

        private void CargarProductosDisponibles()
        {
            try
            {
                var productos =
                    _mantenimientoRepository
                        .ObtenerProductosDisponibles();

                ViewBag.ProductosDisponibles =
                    productos.Select(producto =>
                        new SelectListItem
                        {
                            Value =
                                producto.IdProducto.ToString(),

                            Text =
                                $"{producto.Nombre} - Stock: {producto.Stock}"
                        })
                    .ToList();
            }
            catch (Exception)
            {
                ViewBag.ProductosDisponibles =
                    new List<SelectListItem>();
            }
        }

        private void CargarObrasDesdeBD()
        {
            var selectList =
                new List<SelectListItem>();

            try
            {
                using var conn =
                    new SqlConnection(_connectionString);

                const string query = @"
                    SELECT
                        IdObra,
                        Codigo,
                        Nombre
                    FROM dbo.Obras
                    ORDER BY Nombre ASC;";

                using var cmd =
                    new SqlCommand(query, conn);

                conn.Open();

                using var reader =
                    cmd.ExecuteReader();

                while (reader.Read())
                {
                    selectList.Add(
                        new SelectListItem
                        {
                            Value =
                                reader["IdObra"].ToString(),

                            Text =
                                $"{reader["Codigo"]} - {reader["Nombre"]}"
                        });
                }
            }
            catch (Exception)
            {
                selectList =
                    new List<SelectListItem>();
            }

            ViewBag.ObrasDisponibles =
                selectList;
        }
    }
}
