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

        public async Task<IActionResult> Index(int? dobavljacId)
        {
            var client = _httpFactory.CreateClient("GomexApi");

            var query = dobavljacId.HasValue
                ? $"api/dashboard/summary?DobavljacId={dobavljacId}"
                : "api/dashboard/summary";

            DashboardViewModel? model = null;
            try
            {
                model = await client.GetFromJsonAsync<DashboardViewModel>(query);
            }
            catch
            {
                // swallow and render empty model on error
            }

            model ??= new DashboardViewModel();
            model.SelectedDobavljacId = dobavljacId;

            try
            {
                var dobavljaci = await client.GetFromJsonAsync<List<DobavljacViewItem>>("api/dobavljaci");
                model.Dobavljaci = dobavljaci ?? new List<DobavljacViewItem>();
            }
            catch
            {
                // ostaje prazna lista ako poziv ne uspe
            }

            return View(model);
        }
    }
}