namespace GaleriaArte.Web.Models
{
    public class BitacoraDetalle
    {
        public int IdBitacora { get; set; }

        public int? IdUsuario { get; set; }

        public string Procedimiento { get; set; } = string.Empty;

        public string Operacion { get; set; } = string.Empty;

        public int? RegistroAfectado { get; set; }

        public string? Detalle { get; set; }

        public DateTime FechaHora { get; set; }

        public string Resultado { get; set; } = string.Empty;
    }
}