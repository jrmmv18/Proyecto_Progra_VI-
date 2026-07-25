using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string NombreUsuario { get; set; } = string.Empty;

        // Solo se usa cuando se crea o cambia la contraseña
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // Se almacena en la base de datos
        public string PasswordHash { get; set; } = string.Empty;

        // Para el combo de roles
        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public int IdRol { get; set; }

        // Para mostrar el nombre del rol en el Index
        public string Rol { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }
}