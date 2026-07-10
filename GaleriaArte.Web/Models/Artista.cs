using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Artista
    {
        public int IdArtista { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "País")]
        public string Pais { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(100)]
        [Display(Name = "Correo electrónico")]
        public string? Correo { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Biografía")]
        public string? Biografia { get; set; }

        public bool Estado { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        // Propiedad auxiliar para mostrar nombre y apellido 
        public string NombreCompleto
        {
            get
            {
                return $"{Nombre} {Apellido}";
            }
        }
    }
}
