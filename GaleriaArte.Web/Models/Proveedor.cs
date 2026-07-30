using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(100)]
        [Display(Name = "Correo electrónico")]
        public string? Correo { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        public bool Estado { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }
    }
}