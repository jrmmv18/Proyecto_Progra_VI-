using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(100)]
        [Display(Name = "Correo")]
        public string? Correo { get; set; }

        [StringLength(400)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        public bool Estado { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCompleto =>
            $"{Nombre} {Apellido}".Trim();
    }
}
