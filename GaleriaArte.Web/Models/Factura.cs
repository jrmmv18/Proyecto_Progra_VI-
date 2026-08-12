namespace GaleriaArte.Web.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }

        public int IdCliente { get; set; }

        // Visita durante la cual se hizo la compra
        public int IdVisita { get; set; }

        public int IdUsuario { get; set; }

        public int IdEstadoFactura { get; set; }

        public DateTime FechaFactura { get; set; }

        public string MetodoPago { get; set; } =
            string.Empty;

        public decimal Subtotal { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        public string NombreCliente { get; set; } =
            string.Empty;

        public string NombreUsuario { get; set; } =
            string.Empty;

        public string EstadoFactura { get; set; } =
            string.Empty;

        public string CorreoCliente { get; set; } =
            string.Empty;

        public string TelefonoCliente { get; set; } =
            string.Empty;

        // Datos de la visita en que se hizo la compra
        public DateTime FechaIngresoVisita { get; set; }

        public DateTime? FechaSalidaVisita { get; set; }

        public bool VisitaEnCurso => FechaSalidaVisita == null;

        public List<DetalleFactura> Detalles { get; set; } =
            new List<DetalleFactura>();
    }
}