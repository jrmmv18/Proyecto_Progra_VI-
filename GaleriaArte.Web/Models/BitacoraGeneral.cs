namespace GaleriaArte.Web.Models
{
    public class BitacoraGeneral
    {
        public int IdBitacora { get; set; }

        public int IdUsuario { get; set; }

        // Nombre del usuario que realizó la acción
        public string NombreUsuario { get; set; } = string.Empty;

        public string Modulo { get; set; } = string.Empty;

        public string TablaAfectada { get; set; } = string.Empty;

        public string TipoOperacion { get; set; } = string.Empty;

        public int RegistroAfectado { get; set; }

        public string? ValorAnterior { get; set; }

        public string? ValorNuevo { get; set; }

        public DateTime FechaHora { get; set; }

        public string Resultado { get; set; } = string.Empty;
    }
}