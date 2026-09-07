using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.GomexMVC.Controllers
{
    public class StampanjeController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public StampanjeController(
            IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Komercijalni));
        }

        public IActionResult Komercijalni()
        {
            var danas = DateTime.Today;

            ViewData["DatumOd"] =
                danas.AddDays(-29)
                    .ToString("yyyy-MM-dd");

            ViewData["DatumDo"] =
                danas.ToString("yyyy-MM-dd");

            return View();
        }

        public async Task<IActionResult> PrintKomercijalni(
            DateTime datumOd,
            DateTime datumDo)
        {
            if (datumOd.Date > datumDo.Date)
            {
                TempData["PrintError"] =
                    "Datum od ne može biti posle datuma do.";

                return RedirectToAction(
                    nameof(Komercijalni)
                );
            }

            if (datumDo.Date > DateTime.Today)
            {
                TempData["PrintError"] =
                    "Datum do ne može biti u budućnosti.";

                return RedirectToAction(
                    nameof(Komercijalni)
                );
            }

            var client =
                _httpFactory.CreateClient(
                    "GomexApi"
                );

            RucChangeViewItem? results = null;

            try
            {
                var query =
                    $"?datumOd={datumOd:yyyy-MM-dd}" +
                    $"&datumDo={datumDo:yyyy-MM-dd}";

                results =
                    await client
                        .GetFromJsonAsync<RucChangeViewItem>(
                            $"api/RucChangeTracker{query}"
                        );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PRINT KOMERCIJALNI ERROR: {ex}"
                );

                TempData["PrintError"] =
                    "Nije moguće učitati podatke za izabrani period.";

                return RedirectToAction(
                    nameof(Komercijalni)
                );
            }

            if (results == null)
            {
                TempData["PrintError"] =
                    "Za izabrani period nema dostupnih podataka.";

                return RedirectToAction(
                    nameof(Komercijalni)
                );
            }

            ViewData["DatumOd"] =
                datumOd.ToString("dd.MM.yyyy");

            ViewData["DatumDo"] =
                datumDo.ToString("dd.MM.yyyy");

            return View(results);
        }
    }
}