using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class ProductoUsadoMantenimiento
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Debe seleccionar un producto.")]
        public int IdProducto { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad utilizada debe ser mayor que cero.")]
        public int CantidadUtilizada { get; set; }
    }
}