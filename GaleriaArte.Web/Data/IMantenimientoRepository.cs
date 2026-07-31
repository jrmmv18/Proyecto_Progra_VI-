using System.Collections.Generic;
using GaleriaArte.Web.Models;

namespace GaleriaArte.Web.Data
{
    public interface IMantenimientoRepository
    {
        int RegistrarMantenimientoCompleto(
            Mantenimiento mantenimiento);

        List<Mantenimiento> ListarTodos();

        bool RegistrarProductoUsado(
            int idMantenimiento,
            int idProducto,
            int cantidadUtilizada);

        List<Producto> ObtenerProductosDisponibles();
    }
}