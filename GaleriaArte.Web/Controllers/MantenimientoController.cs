using System;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Data;
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

        // Constructor del Controlador
        public MantenimientoController(IMantenimientoRepository mantenimientoRepository, IConfiguration configuration)
        {
            _mantenimientoRepository = mantenimientoRepository ?? throw new ArgumentNullException(nameof(mantenimientoRepository));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _connectionString = configuration.GetConnectionString("GaleriaArteDB") ?? string.Empty;
        }

        // T-16.8: GET: Mantenimiento (Vista del historial)
        public IActionResult Index()
        {
            try
            {
                var listaMantenimientos = _mantenimientoRepository.ListarTodos();
                return View(listaMantenimientos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"No se pudo cargar el historial: {ex.Message}";
                return View(new List<Mantenimiento>());
            }
        }

        // T-16.8: GET: Mantenimiento/Crear (Muestra el formulario)
        [HttpGet]
        public IActionResult Crear()
        {
            CargarObrasDesdeBD();
            var nuevoMantenimiento = new Mantenimiento();
            return View(nuevoMantenimiento);
        }

        // T-16.7 & T-16.9: POST: Mantenimiento/Crear (Procesa la transacción del formulario)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Mantenimiento mantenimiento)
        {
            try
            {
                // T-16.6: Integración automática del usuario autenticado
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int idUsuarioAutenticado))
                {
                    mantenimiento.IdUsuario = idUsuarioAutenticado;
                }
                else
                {
                    mantenimiento.IdUsuario = 1;
                }

                if (ModelState.IsValid)
                {
                    bool registroExitoso = _mantenimientoRepository.RegistrarMantenimientoCompleto(mantenimiento);

                    if (registroExitoso)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "La base de datos rechazó el registro. Verifique que el procedimiento almacenado esté respondiendo.");
                    }
                }
                else
                {
                    // DETECTOR DE ERRORES OCULTOS: Si falta un campo requerido, esto nos dirá cuál es
                    var errores = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    ModelState.AddModelError(string.Empty, $"Validación fallida en el servidor: {errores}");
                }
            }
            catch (Exception ex)
            {
                // Muestra el mensaje real de SQL Server (Como errores de Foreign Key o Triggers)
                ModelState.AddModelError(string.Empty, $"Error crítico: {ex.Message}");
            }

            CargarObrasDesdeBD(); // Recarga obligatoria para que el dropdown no se borre
            return View(mantenimiento);
        }

        // METODO AUTÓNOMO: Consulta directa a SQL para alimentar el Dropdown
        private void CargarObrasDesdeBD()
        {
            var selectList = new List<SelectListItem>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT IdObra, Codigo, Nombre FROM Obras ORDER BY Nombre ASC";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                selectList.Add(new SelectListItem
                                {
                                    Value = reader["IdObra"].ToString(),
                                    Text = $"{reader["Codigo"]} - {reader["Nombre"]}"
                                });
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception)
            {
                // Respaldo de lista vacía en caso de fallo
            }

            ViewBag.ObrasDisponibles = selectList;
        }
    }
}
