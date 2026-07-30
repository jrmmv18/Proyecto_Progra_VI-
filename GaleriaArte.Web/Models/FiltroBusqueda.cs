using System.Globalization;
using System.Text;

namespace GaleriaArte.Web.Models
{
    /// <summary>
    /// Comparaciones usadas por los filtros de los listados.
    /// La busqueda ignora mayusculas y tildes para que "Nunez"
    /// tambien encuentre "Nunez" escrito con acento.
    /// </summary>
    public static class FiltroBusqueda
    {
        // =====================================================
        // BUSCAR EL TEXTO EN CUALQUIERA DE LOS CAMPOS
        // =====================================================

        public static bool Coincide(
            string? busqueda,
            params string?[] campos)
        {
            if (string.IsNullOrWhiteSpace(busqueda))
            {
                return true;
            }

            string texto = Normalizar(busqueda);

            return campos.Any(campo =>
                Normalizar(campo).Contains(texto));
        }

        // =====================================================
        // VERIFICAR QUE LA FECHA ESTE DENTRO DEL RANGO
        // =====================================================

        public static bool EnRango(
            DateTime? fecha,
            DateTime? inicio,
            DateTime? fin)
        {
            if (!inicio.HasValue && !fin.HasValue)
            {
                return true;
            }

            if (!fecha.HasValue)
            {
                return false;
            }

            DateTime dia = fecha.Value.Date;

            if (inicio.HasValue && dia < inicio.Value.Date)
            {
                return false;
            }

            // El limite superior incluye todo el dia seleccionado
            if (fin.HasValue && dia > fin.Value.Date)
            {
                return false;
            }

            return true;
        }

        // =====================================================
        // QUITAR TILDES Y PASAR A MINUSCULAS
        // =====================================================

        private static string Normalizar(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            string descompuesto =
                valor.Trim().Normalize(NormalizationForm.FormD);

            StringBuilder limpio = new(descompuesto.Length);

            foreach (char caracter in descompuesto)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo.GetUnicodeCategory(caracter);

                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    limpio.Append(caracter);
                }
            }

            return limpio
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
        }
    }
}
