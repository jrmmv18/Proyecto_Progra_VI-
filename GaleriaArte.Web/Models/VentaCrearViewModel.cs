using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class VentaCrearViewModel
    {
        [Required(
            ErrorMessage = "Debe seleccionar un cliente."
        )]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; }

        [Required(
            ErrorMessage = "Debe seleccionar un método de pago."
        )]
        [Display(Name = "Método de pago")]
        public string MetodoPago { get; set; } =
            string.Empty;

        public decimal PorcentajeImpuesto { get; set; } =
            0.13m;

        // Productos que se llevan en la compra
        public List<DetalleVentaCrearViewModel> Productos { get; set; } =
            new List<DetalleVentaCrearViewModel>();

        // Solo aparecen los clientes que estan dentro de la galeria,
        // porque para facturar hace falta una visita en curso.
        public List<Cliente> ClientesDisponibles { get; set; } =
            new List<Cliente>();

        public List<Producto> ProductosDisponibles { get; set; } =
            new List<Producto>();
    }

    public class DetalleVentaCrearViewModel
    {
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
