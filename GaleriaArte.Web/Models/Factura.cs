using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public int IdEstadoFactura { get; set; }

        [Display(Name = "Fecha")]
        public DateTime FechaFactura { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
        [StringLength(20)]
        [Display(Name = "Método de pago")]
        public string MetodoPago { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string NombreUsuario { get; set; } = string.Empty;

        public string EstadoFactura { get; set; } = string.Empty;

        public List<DetalleFactura> Detalles { get; set; } =
            new List<DetalleFactura>();
    }
}
