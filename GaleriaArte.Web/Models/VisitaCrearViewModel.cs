using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    /// <summary>
    /// Datos del formulario de entrada. El visitante se registra
    /// en el mismo paso, por eso lleva sus datos personales junto
    /// con los de la visita.
    /// </summary>
    public class VisitaCrearViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(100)]
        [Display(Name = "Correo")]
        public string? Correo { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
        [Display(Name = "Fecha y hora de entrada")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        // La salida no se pide aqui: se marca despues, desde el
        // listado, con el boton Registrar salida.
    }
}
