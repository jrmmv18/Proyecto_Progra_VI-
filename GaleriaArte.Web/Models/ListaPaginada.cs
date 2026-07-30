namespace GaleriaArte.Web.Models
{
    /// <summary>
    /// Listado ya filtrado y recortado a la pagina solicitada.
    /// Es el modelo que reciben las vistas Index de cada modulo.
    /// </summary>
    public class ListaPaginada<T>
    {
        public List<T> Items { get; set; } = new();

        public PaginacionInfo Paginacion { get; set; } = new();

        // =====================================================
        // CONSTRUIR LA PAGINA SOLICITADA
        // =====================================================

        public static ListaPaginada<T> Crear(
            IEnumerable<T> origen,
            PaginacionInfo paginacion)
        {
            List<T> registros = origen.ToList();

            paginacion.TotalRegistros = registros.Count;

            // Ajustar la pagina cuando el filtro deja menos resultados
            if (paginacion.PaginaActual < 1)
            {
                paginacion.PaginaActual = 1;
            }

            if (paginacion.PaginaActual > paginacion.TotalPaginas)
            {
                paginacion.PaginaActual = paginacion.TotalPaginas;
            }

            int omitir =
                (paginacion.PaginaActual - 1) * paginacion.TamanoPagina;

            return new ListaPaginada<T>
            {
                Items = registros
                    .Skip(omitir)
                    .Take(paginacion.TamanoPagina)
                    .ToList(),

                Paginacion = paginacion
            };
        }
    }
}
