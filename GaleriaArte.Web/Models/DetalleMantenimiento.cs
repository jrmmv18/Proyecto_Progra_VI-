// Archivo: Models/DetalleMantenimiento.cs
using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class DetalleMantenimiento
    {
        public int IdDetalleMantenimiento { get; set; }
        public int IdMantenimiento { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio.")]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }
}
