namespace GaleriaArte.Web.Models
{
    /// <summary>
    /// Estado de la paginacion y de los filtros de un listado.
    /// Se comparte con las vistas parciales _Filtros y _Paginacion.
    /// </summary>
    public class PaginacionInfo
    {
        // Cantidad de registros que se muestran en cada pagina
        public const int RegistrosPorPagina = 10;

        public int PaginaActual { get; set; } = 1;

        public int TotalRegistros { get; set; }

        public int TamanoPagina { get; set; } = RegistrosPorPagina;

        // =====================================================
        // VALORES DE LOS FILTROS
        // =====================================================

        public string? Busqueda { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        // =====================================================
        // CONFIGURACION DE LA VISTA
        // =====================================================

        public string Controlador { get; set; } = string.Empty;

        public string Accion { get; set; } = "Index";

        public bool MostrarFiltroFechas { get; set; } = true;

        public string PlaceholderBusqueda { get; set; } = "Buscar...";

        public string EtiquetaFechas { get; set; } = "Fecha";

        // =====================================================
        // VALORES CALCULADOS
        // =====================================================

        public int TotalPaginas =>
            TotalRegistros == 0
                ? 1
                : (int)Math.Ceiling(
                    TotalRegistros / (double)TamanoPagina);

        public bool TienePaginaAnterior => PaginaActual > 1;

        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        public bool HayFiltros =>
            !string.IsNullOrWhiteSpace(Busqueda) ||
            FechaInicio.HasValue ||
            FechaFin.HasValue;

        // Numero del primer registro mostrado en la pagina actual
        public int PrimerRegistro =>
            TotalRegistros == 0
                ? 0
                : ((PaginaActual - 1) * TamanoPagina) + 1;

        // Numero del ultimo registro mostrado en la pagina actual
        public int UltimoRegistro =>
            Math.Min(PaginaActual * TamanoPagina, TotalRegistros);
    }
}
