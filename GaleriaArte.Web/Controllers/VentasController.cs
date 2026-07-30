using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        // GET: Ventas
        public IActionResult Index()
        {
            return View();
        }
    }
}
