using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Web.Models
{
    public class Obra
    {
        public int IdObra { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un artista.")]
        [Display(Name = "Artista")]
        public int IdArtista { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Categoría")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la obra es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre de la obra")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de creación")]
        public DateTime? FechaCreacion { get; set; }

        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaIngresoGaleria { get; set; }

        [Required(ErrorMessage = "El valor estimado es obligatorio.")]
        [Range(0, 999999999999.99,
            ErrorMessage = "Ingrese un valor estimado válido.")]
        [Display(Name = "Valor estimado")]
        public decimal ValorEstimado { get; set; }

        [Required(ErrorMessage = "El estado de conservación es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Estado de conservación")]
        public string EstadoConservacion { get; set; } = string.Empty;

        public bool Estado { get; set; }

        // Propiedades auxiliares para mostrar datos relacionados
        public string NombreArtista { get; set; } = string.Empty;

        public string NombreCategoria { get; set; } = string.Empty;
    }
}