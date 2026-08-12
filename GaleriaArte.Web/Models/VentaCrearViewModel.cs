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

        public List<DetalleVentaCrearViewModel> Obras { get; set; } =
            new List<DetalleVentaCrearViewModel>();

        public List<Cliente> ClientesDisponibles { get; set; } =
            new List<Cliente>();

        public List<Obra> ObrasDisponibles { get; set; } =
            new List<Obra>();
    }

    public class DetalleVentaCrearViewModel
    {
        public int IdObra { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
