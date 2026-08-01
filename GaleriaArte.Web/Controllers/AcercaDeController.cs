using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class AcercaDeController : Controller
    {
        // =====================================================
        // INFORMACION DE LA GALERIA Y DEL SISTEMA
        // =====================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
