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

        public async Task<IActionResult> Index(DateTime? datumOd, DateTime? datumDo, int? odeljenjeId, int? kategorijaId, int? dobavljacId, int? tipProdajeId)
        {
            var client = _httpFactory.CreateClient("GomexApi");
            // Build query string only for provided parameters
            var queryParts = new List<string>();
            if (datumOd.HasValue) queryParts.Add($"datumOd={datumOd.Value:yyyy-MM-dd}");
            if (datumDo.HasValue) queryParts.Add($"datumDo={datumDo.Value:yyyy-MM-dd}");
            if (odeljenjeId.HasValue) queryParts.Add($"odeljenjeId={odeljenjeId.Value}");
            if (kategorijaId.HasValue) queryParts.Add($"kategorijaId={kategorijaId.Value}");
            if (dobavljacId.HasValue) queryParts.Add($"dobavljacId={dobavljacId.Value}");
            if (tipProdajeId.HasValue) queryParts.Add($"tipProdajeId={tipProdajeId.Value}");

            var query = queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;

            DashboardViewModel? model = null;
            try
            {
                model = await client.GetFromJsonAsync<DashboardViewModel>($"api/dashboard/summary{query}");
            }
            catch
            {
                // swallow and render empty model on error
            }

            model ??= new DashboardViewModel();

            // Populate dobavljaci; if odeljenjeId is provided try backend filtered endpoint, otherwise get all
            try
            {
                if (odeljenjeId.HasValue)
                {
                    try
                    {
                        var filtered = await client.GetFromJsonAsync<List<DobavljacViewItem>>($"api/dobavljaci/byOdeljenje?odeljenjeId={odeljenjeId.Value}");
                        model.Dobavljaci = filtered ?? new List<DobavljacViewItem>();
                    }
                    catch
                    {
                        // fallback to generic list if filtered endpoint not available
                        var dobavljaci = await client.GetFromJsonAsync<List<DobavljacViewItem>>("api/dobavljaci");
                        model.Dobavljaci = dobavljaci ?? new List<DobavljacViewItem>();
                    }
                }
                else
                {
                    var dobavljaci = await client.GetFromJsonAsync<List<DobavljacViewItem>>("api/dobavljaci");
                    model.Dobavljaci = dobavljaci ?? new List<DobavljacViewItem>();
                }
            }
            catch
            {
                model.Dobavljaci = new List<DobavljacViewItem>();
            }

            // Try to populate Odeljenja from backend lookup endpoint; fallback to small local list if call fails
            try
            {
                var odeljenja = await client.GetFromJsonAsync<List<OdeljenjeViewItem>>("api/odeljenja");
                model.Odeljenja = odeljenja ?? new List<OdeljenjeViewItem>();
            }
            catch
            {
                model.Odeljenja = new List<OdeljenjeViewItem>
                {
                    new OdeljenjeViewItem { OdeljenjeId = 1, Naziv = "Voće" },
                    new OdeljenjeViewItem { OdeljenjeId = 2, Naziv = "Povrće" },
                    new OdeljenjeViewItem { OdeljenjeId = 3, Naziv = "Smrznuto" },
                    new OdeljenjeViewItem { OdeljenjeId = 4, Naziv = "Sirevi" },
                    new OdeljenjeViewItem { OdeljenjeId = 5, Naziv = "Meso" }
                };
            }

            // Store selected filters in model so view can render selected state
            if (datumOd.HasValue) model.DatumOd = DateOnly.FromDateTime(datumOd.Value);
            if (datumDo.HasValue) model.DatumDo = DateOnly.FromDateTime(datumDo.Value);
            model.OdeljenjeId = odeljenjeId;
            model.KategorijaId = kategorijaId;
            model.TipProdajeId = tipProdajeId;
            model.SelectedDobavljacId = dobavljacId;

            ViewData["SelectedDobavljacId"] = dobavljacId;

            return View(model);
        }
    }
}