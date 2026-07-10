using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}