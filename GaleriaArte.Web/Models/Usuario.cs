namespace GaleriaArte.Web.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string NombreUsuario { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }
}