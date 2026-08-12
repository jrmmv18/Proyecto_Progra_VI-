namespace GaleriaArte.Web.Models
{
    public class Paginacion<T>
    {
        public List<T> Datos { get; set; } = new();

        public int PaginaActual { get; set; }

        public int RegistrosPorPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas =>
            (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

        public bool HayAnterior =>
            PaginaActual > 1;

        public bool HaySiguiente =>
            PaginaActual < TotalPaginas;
    }
}