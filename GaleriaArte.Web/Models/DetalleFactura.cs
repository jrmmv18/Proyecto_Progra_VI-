namespace GaleriaArte.Web.Models
{
    public class DetalleFactura
    {
        public int IdDetalleFactura { get; set; }

        public int IdFactura { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        // Propiedades auxiliares para mostrar datos relacionados
        public string CodigoProducto { get; set; } = string.Empty;

        public string NombreProducto { get; set; } = string.Empty;
    }
}
