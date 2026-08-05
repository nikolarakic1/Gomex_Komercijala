using Microsoft.AspNetCore.Mvc;

namespace GomexPraksaMVC.Controllers
{
    public class ArtikalController : Controller
    {
        public IActionResult Detalji(string sifra)
        {
            ViewData["Sifra"] = sifra;
            return View();
        }
    }
}