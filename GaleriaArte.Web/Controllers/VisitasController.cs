using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Web.Controllers
{
    [Authorize]
    public class VisitasController : Controller
    {
        // GET: Visitas
        public IActionResult Index()
        {
            return View();
        }
    }
}
