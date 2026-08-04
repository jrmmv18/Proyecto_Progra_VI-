namespace GaleriaArte.Web.Models
{
    public class BitacoraError
    {
        public int IdError { get; set; }

        public int? IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = "";

        public string Procedimiento { get; set; } = "";

        public int NumeroError { get; set; }

        public string Descripcion { get; set; } = "";

        public DateTime FechaHora { get; set; }

        public bool RollbackEjecutado { get; set; }
    }
}