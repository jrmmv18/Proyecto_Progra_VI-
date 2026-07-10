using System.Security.Claims;
using GaleriaArte.Web.Data;
using GaleriaArte.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioRepository _usuarioRepository;

        public AccountController(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // Mostrar formulario de inicio de sesión
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya inició sesión, no mostrar nuevamente el login
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Procesar inicio de sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar usuario en la base de datos
            Usuario? usuario =
                await _usuarioRepository.ObtenerPorNombreUsuarioAsync(
                    model.Usuario
                );

            if (usuario == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Usuario o contraseña incorrectos."
                );

                return View(model);
            }

            // Verificar que el usuario esté activo
            if (!usuario.Estado)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El usuario se encuentra inactivo."
                );

                return View(model);
            }

            // Verificar contraseña
            bool passwordCorrecto = PasswordHasher.VerifyPassword(
                model.Password,
                usuario.PasswordHash
            );

            if (!passwordCorrecto)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Usuario o contraseña incorrectos."
                );

                return View(model);
            }

            // Crear los datos de identidad del usuario
            List<Claim> claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreUsuario
                ),

                new Claim(
                    "NombreCompleto",
                    $"{usuario.Nombre} {usuario.Apellido}"
                )
            };

            // Crear identidad
            ClaimsIdentity identity = new ClaimsIdentity(
                claims,
                "GaleriaArteCookie"
            );

            // Crear usuario autenticado
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // Crear la cookie de autenticación
            await HttpContext.SignInAsync(
                "GaleriaArteCookie",
                principal
            );

            // Entrar al sistema
            return RedirectToAction("Index", "Home");
        }

        // Cerrar sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("GaleriaArteCookie");

            return RedirectToAction("Login", "Account");
        }

        // Acceso denegado
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return Content("Acceso denegado.");
        }
    }
}