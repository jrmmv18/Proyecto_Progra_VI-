using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Mantenimiento
    {
        [Key]
        public int IdMantenimiento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una obra de arte.")]
        public int IdObra { get; set; }

        [Required(ErrorMessage = "La descripción del trabajo es obligatoria.")]
        public string DescripcionTrabajo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaMantenimiento { get; set; } = DateTime.Now;

        public int IdUsuario { get; set; }

        // Productos utilizados durante el mantenimiento
        public List<ProductoUsadoMantenimiento> ProductosUsados { get; set; }
            = new List<ProductoUsadoMantenimiento>();
    }
}