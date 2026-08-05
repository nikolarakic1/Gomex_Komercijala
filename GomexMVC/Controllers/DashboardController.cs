using Microsoft.AspNetCore.Mvc;

namespace GomexPraksaMVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}