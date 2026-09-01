using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.GomexMVC.Controllers
{
    public class StampanjeController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public StampanjeController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Komercijalni()
        {
            return View();
        }

        public async Task<IActionResult> PrintKomercijalni(DateTime? datum)
        {
            var client = _httpFactory.CreateClient("GomexApi");
            Models.RucChangeViewItem? results = null;

            if (datum.HasValue)
            {
                try
                {
                    var datumOd = datum.Value.Date;
                    var datumDo = datum.Value.Date;
                    var prethodniDatumDo = datumOd.AddDays(-1);
                    var brojDana = (datumDo - datumOd).Days + 1;
                    var prethodniDatumOd = prethodniDatumDo.AddDays(-(brojDana - 1));

                    var parts = new List<string>
                    {
                        $"datumOd={datumOd:yyyy-MM-dd}",
                        $"datumDo={datumDo:yyyy-MM-dd}",
                        $"prethodniDatumOd={prethodniDatumOd:yyyy-MM-dd}",
                        $"prethodniDatumDo={prethodniDatumDo:yyyy-MM-dd}"
                    };

                    var query = "?" + string.Join("&", parts);

                    results = await client.GetFromJsonAsync<Models.RucChangeViewItem>($"api/RucChangeTracker{query}");
                }
                catch
                {
                    results = null;
                }
            }

            ViewData["Datum"] = datum;
            return View(model: results);
        }
    }
}
