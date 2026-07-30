// Archivo: Data/IMantenimientoRepository.cs
using System.Collections.Generic;
using GaleriaArte.Web.Models;

namespace GaleriaArte.Web.Data
{
    public interface IMantenimientoRepository
    {
        bool RegistrarMantenimientoCompleto(Mantenimiento mantenimiento);
        List<Mantenimiento> ListarTodos();
    }
}
