using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Mantenimiento
    {
        // Propiedades requeridas por las Vistas e Historial
        [Key]
        public int IdMantenimiento { get; set; }

        // ACTUALIZACIÓN: Retorna a 'int' para coincidir perfectamente con la PK_Obras y evitar errores de conversión
        [Required(ErrorMessage = "Debe seleccionar una obra de arte.")]
        public int IdObra { get; set; }

        [Required(ErrorMessage = "La descripción del trabajo es obligatoria.")]
        public string DescripcionTrabajo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaMantenimiento { get; set; } = DateTime.Now;

        public int IdUsuario { get; set; }

        // Propiedad agregada para evitar errores si la vista Crear/Index usa colecciones de insumos
        public List<MantenimientoDetalle> Detalles { get; set; } = new List<MantenimientoDetalle>();
    }

    public class MantenimientoDetalle
    {
        public int IdMantenimientoDetalle { get; set; }
        public int IdMantenimiento { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }
}
