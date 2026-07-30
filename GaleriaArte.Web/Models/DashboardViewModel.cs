namespace GaleriaArte.Web.Models
{
    public class DashboardViewModel
    {
        // Tarjetas de resumen
        public int TotalObras { get; set; }

        public int TotalArtistas { get; set; }

        public int TotalProductos { get; set; }

        public int TotalProveedores { get; set; }

        public int TotalVentas { get; set; }

        public int TotalMantenimientos { get; set; }

        // Visitas registradas en total
        public int TotalVisitas { get; set; }

        // Visitantes que aun no registran su salida
        public int VisitasEnCurso { get; set; }

        // Usuario autenticado
        public string Usuario { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        // Información del Dashboard
        public DateTime FechaActual => DateTime.Now;

        public string Saludo
        {
            get
            {
                if (DateTime.Now.Hour < 12)
                    return "Buenos días";

                if (DateTime.Now.Hour < 18)
                    return "Buenas tardes";

                return "Buenas noches";
            }
        }
    }
}