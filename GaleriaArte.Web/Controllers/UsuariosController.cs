using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace GaleriaArte.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuariosController(
            UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // =====================================================
        // LISTAR USUARIOS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1)
        {
            List<Usuario> usuarios =
                await _usuarioRepository.ListarAsync();

            IEnumerable<Usuario> filtrados = usuarios
                .Where(usuario => FiltroBusqueda.Coincide(
                    busqueda,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.NombreUsuario,
                    usuario.Correo,
                    usuario.Rol))
                .Where(usuario => FiltroBusqueda.EnRango(
                    usuario.FechaRegistro,
                    fechaInicio,
                    fechaFin));

            PaginacionInfo paginacion = new()
            {
                PaginaActual = pagina,
                Busqueda = busqueda,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Controlador = "Usuarios",
                EtiquetaFechas = "Registro",
                PlaceholderBusqueda =
                    "Nombre, apellido, usuario o correo"
            };

            return View(
                ListaPaginada<Usuario>.Crear(filtrados, paginacion));
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE CREACIÓN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarRolesAsync();

            return View(new Usuario
            {
                Estado = true
            });
        }

        // =====================================================
        // GUARDAR NUEVO USUARIO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Password))
            {
                ModelState.AddModelError(
                    nameof(usuario.Password),
                    "La contraseña es obligatoria.");
            }

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(usuario.IdRol);

                return View(usuario);
            }

            try
            {
                usuario.PasswordHash =
                    PasswordHasher.HashPassword(
                        usuario.Password!);

                bool creado =
                    await _usuarioRepository.CrearAsync(
                        usuario,
                        ObtenerIdUsuario());

                if (!creado)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "No fue posible registrar el usuario.");

                    await CargarRolesAsync(usuario.IdRol);

                    return View(usuario);
                }

                TempData["Mensaje"] =
                    "Usuario registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                await CargarRolesAsync(usuario.IdRol);

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(usuario);
            }
            catch (Exception)
            {
                await CargarRolesAsync(usuario.IdRol);

                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al registrar el usuario.");

                return View(usuario);
            }
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Usuario? usuario =
                await _usuarioRepository.ObtenerPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Nunca enviar el hash a la vista
            usuario.Password = string.Empty;
            usuario.PasswordHash = string.Empty;

            await CargarRolesAsync(usuario.IdRol);

            return View(usuario);
        }
        // =====================================================
        // GUARDAR CAMBIOS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(usuario.IdRol);

                return View(usuario);
            }

            try
            {
                // Si se escribió una nueva contraseña,
                // se genera un nuevo hash.
                if (!string.IsNullOrWhiteSpace(usuario.Password))
                {
                    usuario.PasswordHash =
                        PasswordHasher.HashPassword(
                            usuario.Password);
                }
                else
                {
                    // El procedimiento almacenado conservará
                    // el hash actual.
                    usuario.PasswordHash = string.Empty;
                }

                bool actualizado =
                    await _usuarioRepository.ActualizarAsync(
                        usuario,
                        ObtenerIdUsuario());

                if (!actualizado)
                {
                    return NotFound();
                }

                TempData["Mensaje"] =
                    "Usuario actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                await CargarRolesAsync(usuario.IdRol);

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(usuario);
            }
            catch (Exception)
            {
                await CargarRolesAsync(usuario.IdRol);

                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error inesperado al actualizar el usuario.");

                return View(usuario);
            }
        }

        // =====================================================
        // ACTIVAR / DESACTIVAR USUARIO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                Usuario? usuario =
                    await _usuarioRepository.ObtenerPorIdAsync(id);

                if (usuario == null)
                {
                    return NotFound();
                }

                bool nuevoEstado = !usuario.Estado;

                bool actualizado =
                    await _usuarioRepository.CambiarEstadoAsync(
                        id,
                        nuevoEstado,
                        ObtenerIdUsuario());

                if (!actualizado)
                {
                    return NotFound();
                }

                TempData["Mensaje"] = nuevoEstado
                    ? "Usuario activado correctamente."
                    : "Usuario desactivado correctamente.";

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
                    "Ocurrió un error al cambiar el estado del usuario.";

                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // CARGAR ROLES ACTIVOS
        // =====================================================

        private async Task CargarRolesAsync(
            int? rolSeleccionado = null)
        {
            List<Rol> roles =
                await _usuarioRepository.ObtenerRolesActivosAsync();

            ViewBag.Roles = new SelectList(
                roles,
                "IdRol",
                "Nombre",
                rolSeleccionado
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
                "No se pudo obtener el usuario autenticado.");
        }
    }
}