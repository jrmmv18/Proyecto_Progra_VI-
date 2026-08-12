namespace GaleriaArte.Web.Models
{
    public class BitacoraModulo
    {
        public int IdBitacora { get; set; }

        public int? IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = "";

        public string Procedimiento { get; set; } = "";

        public string Operacion { get; set; } = "";

        public int? RegistroAfectado { get; set; }

        public string? Detalle { get; set; }

        public DateTime FechaHora { get; set; }

        public string Resultado { get; set; } = "";
    }
}