using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.Controllers
{
    [TypeFilter(typeof(GomexPraksaMVC.Filters.RequireAuthFilter))]
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

            // normalize date range: default to last 30 days if none provided
            DateOnly? sd = datumOd.HasValue ? DateOnly.FromDateTime(datumOd.Value) : null;
            DateOnly? ed = datumDo.HasValue ? DateOnly.FromDateTime(datumDo.Value) : null;

            if (!sd.HasValue && !ed.HasValue)
            {
                ed = DateOnly.FromDateTime(DateTime.Today);
                sd = ed.Value.AddDays(-29); // last 30 days
            }
            else if (sd.HasValue && !ed.HasValue)
            {
                ed = DateOnly.FromDateTime(DateTime.Today);
            }
            else if (!sd.HasValue && ed.HasValue)
            {
                sd = ed.Value.AddDays(-29);
            }

            var queryParts = new List<string>();
            if (sd.HasValue) queryParts.Add($"datumOd={sd.Value:yyyy-MM-dd}");
            if (ed.HasValue) queryParts.Add($"datumDo={ed.Value:yyyy-MM-dd}");
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
            }

            model ??= new DashboardViewModel();

            try
            {
                var dobavljaci = await client.GetFromJsonAsync<List<DobavljacViewItem>>("api/dobavljaci");
                model.Dobavljaci = dobavljaci ?? new List<DobavljacViewItem>();
            }
            catch
            {
                model.Dobavljaci = new List<DobavljacViewItem>();
            }

            try
            {
                var odeljenja = await client.GetFromJsonAsync<List<OdeljenjeViewItem>>("api/odeljenja");
                model.Odeljenja = odeljenja ?? new List<OdeljenjeViewItem>();
            }
            catch
            {
                model.Odeljenja = new List<OdeljenjeViewItem>();
            }

            try
            {
                var kategorije = await client.GetFromJsonAsync<List<KategorijaViewItem>>("api/kategorije");
                model.Kategorije = kategorije ?? new List<KategorijaViewItem>();
            }
            catch
            {
                model.Kategorije = new List<KategorijaViewItem>();
            }

            try
            {
                var kategorije = await client.GetFromJsonAsync<List<KategorijaViewItem>>("api/kategorije");
                model.Kategorije = kategorije ?? new List<KategorijaViewItem>();
            }
            catch
            {
                model.Kategorije = new List<KategorijaViewItem>();
            }

            if (sd.HasValue) model.DatumOd = sd;
            if (ed.HasValue) model.DatumDo = ed;
            model.OdeljenjeId = odeljenjeId;
            model.KategorijaId = kategorijaId;
            model.TipProdajeId = tipProdajeId;
            model.SelectedDobavljacId = dobavljacId;

            // Top5 critical products
            try
            {
                DateOnly topDatumOd = datumOd.HasValue ? DateOnly.FromDateTime(datumOd.Value) : DateOnly.FromDateTime(DateTime.Today).AddDays(-(DateTime.Today.DayOfYear - 1));
                DateOnly topDatumDo = datumDo.HasValue ? DateOnly.FromDateTime(datumDo.Value) : DateOnly.FromDateTime(DateTime.Today);
                var topQuery = $"?datumOd={topDatumOd:yyyy-MM-dd}&datumDo={topDatumDo:yyyy-MM-dd}";
                var top5 = await client.GetFromJsonAsync<List<CriticalProductViewItem>>($"api/artikli/criticalProductsTop{topQuery}");
                model.CriticalTop5 = top5 ?? new List<CriticalProductViewItem>();
            }
            catch
            {
                model.CriticalTop5 = new List<CriticalProductViewItem>();
            }

            // RUC change (waterfall) - call backend RucChangeTracker
            try
            {
                // compute previous period same as repository logic
                DateOnly datumStart = datumOd.HasValue ? DateOnly.FromDateTime(datumOd.Value) : DateOnly.FromDateTime(DateTime.Today).AddDays(-(DateTime.Today.DayOfYear - 1));
                DateOnly datumEnd = datumDo.HasValue ? DateOnly.FromDateTime(datumDo.Value) : DateOnly.FromDateTime(DateTime.Today);

                int daysCount = datumEnd.DayNumber - datumStart.DayNumber + 1;
                DateOnly prevEnd = datumStart.AddDays(-1);
                DateOnly prevStart = prevEnd.AddDays(-(daysCount - 1));

                var rucQuery = $"?datumOd={datumStart:yyyy-MM-dd}&datumDo={datumEnd:yyyy-MM-dd}&prethodniDatumOd={prevStart:yyyy-MM-dd}&prethodniDatumDo={prevEnd:yyyy-MM-dd}";
                var ruc = await client.GetFromJsonAsync<RucChangeViewItem>($"api/RucChangeTracker{rucQuery}");
                model.RucChange = ruc;
            }
            catch
            {
                model.RucChange = null;
            }

            return View(model);
        }
    }
}
