using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Mantenimiento
    {
        [Key]
        public int IdMantenimiento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una obra.")]
        public int IdObra { get; set; }

        public string NombreObra { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el tipo de mantenimiento.")]
        [StringLength(40)]
        public string TipoMantenimiento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del trabajo es obligatoria.")]
        [StringLength(500)]
        public string DescripcionTrabajo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaMantenimiento { get; set; } = DateTime.Now;

        public int IdUsuario { get; set; }

        public List<MantenimientoDetalle> Detalles { get; set; } =
            new List<MantenimientoDetalle>();

        public List<ProductoUsadoMantenimiento> ProductosUsados { get; set; } =
            new List<ProductoUsadoMantenimiento>();
    }

    public class MantenimientoDetalle
    {
        public int IdDetalleMantenimiento { get; set; }

        public int IdMantenimiento { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }
    }
}