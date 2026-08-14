using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Visita
    {
        public int IdVisita { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un visitante.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un visitante.")]
        [Display(Name = "Visitante")]
        public int IdCliente { get; set; }

        // El formato deja el campo en dia, mes, año, hora y minuto.
        // Sin esto el navegador muestra tambien segundos y milesimas.
        [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
        [Display(Name = "Fecha y hora de entrada")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(
            DataFormatString = "{0:yyyy-MM-ddTHH:mm}",
            ApplyFormatInEditMode = true)]
        public DateTime FechaIngreso { get; set; }

        [Display(Name = "Fecha y hora de salida")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(
            DataFormatString = "{0:yyyy-MM-ddTHH:mm}",
            ApplyFormatInEditMode = true)]
        public DateTime? FechaSalida { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Propiedades auxiliares para mostrar datos relacionados
        public string NombreCliente { get; set; } = string.Empty;

        public string ApellidoCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public string TelefonoCliente { get; set; } = string.Empty;

        public string NombreCompletoCliente =>
            $"{NombreCliente} {ApellidoCliente}".Trim();

        // =====================================================
        // CONTROL DE TIEMPO DE LA VISITA
        // =====================================================

        // Una visita sigue en curso mientras no se registre la salida
        public bool EnCurso => FechaSalida == null;

        // Tiempo transcurrido: hasta la salida o hasta el momento actual
        public TimeSpan Duracion =>
            (FechaSalida ?? DateTime.Now) - FechaIngreso;

        [Display(Name = "Tiempo de permanencia")]
        public string DuracionTexto
        {
            get
            {
                TimeSpan duracion = Duracion;

                if (duracion.TotalSeconds < 0)
                {
                    return "—";
                }

                if (duracion.TotalDays >= 1)
                {
                    return $"{(int)duracion.TotalDays} d {duracion.Hours} h";
                }

                if (duracion.TotalHours >= 1)
                {
                    return $"{(int)duracion.TotalHours} h {duracion.Minutes} min";
                }

                return $"{duracion.Minutes} min";
            }
        }
    }
}
