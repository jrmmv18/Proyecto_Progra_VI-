using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio.")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo.")]
        [Display(Name = "Stock mínimo")]
        public int StockMinimo { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        public bool Estado { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        public string NombreProveedor { get; set; } = string.Empty;
    }
}