using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public DashboardController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpFactory.CreateClient("GomexApi");

            DashboardViewModel? model = null;
            try
            {
                model = await client.GetFromJsonAsync<DashboardViewModel>("api/dashboard/summary");
            }
            catch
            {
                // swallow and render empty model on error
            }

            model ??= new DashboardViewModel();

            return View(model);
        }
    }
}
