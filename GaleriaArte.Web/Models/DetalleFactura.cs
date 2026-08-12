namespace GaleriaArte.Web.Models
{
    public class DetalleFactura
    {
        public int IdDetalleFactura { get; set; }

        public int IdFactura { get; set; }

        public int IdObra { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public string CodigoObra { get; set; } =
            string.Empty;

        public string NombreObra { get; set; } =
            string.Empty;

        public string NombreArtista { get; set; } =
            string.Empty;
    }
}