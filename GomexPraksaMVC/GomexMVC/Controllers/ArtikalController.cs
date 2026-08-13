using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.GomexMVC.Controllers
{
    public class ArtikalController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public ArtikalController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index(int? dobavljacId, int? robnaGrupaId, int? odeljenjeId, int? kategorijaId, string? naziv)
        {
            var client = _httpFactory.CreateClient("GomexApi");

            var model = new ArtikalIndexViewModel();

            // pass selected filters back to view
            model.SelectedDobavljacId = dobavljacId;
            model.SelectedRobnaGrupaId = robnaGrupaId;
            model.SelectedOdeljenjeId = odeljenjeId;
            model.SelectedKategorijaId = kategorijaId;
            model.Naziv = naziv;

            // load artikli
            try
            {
                var queryParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(naziv)) queryParts.Add($"naziv={Uri.EscapeDataString(naziv)}");
                if (dobavljacId.HasValue) queryParts.Add($"dobavljacId={dobavljacId.Value}");
                if (robnaGrupaId.HasValue) queryParts.Add($"robnaGrupaId={robnaGrupaId.Value}");
                if (kategorijaId.HasValue) queryParts.Add($"kategorijaId={kategorijaId.Value}");
                var query = queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;

                var result = await client.GetFromJsonAsync<List<ArtikalViewItem>>($"api/artikli/search{query}");
                model.Artikli = result ?? new List<ArtikalViewItem>();
            }
            catch
            {
                model.Artikli = new List<ArtikalViewItem>();
            }

            // load lookups
            try
            {
                var dobavljaci = await client.GetFromJsonAsync<List<DobavljacViewItem>>("api/dobavljaci");
                model.Dobavljaci = dobavljaci ?? new List<DobavljacViewItem>();
            }
            catch { model.Dobavljaci = new List<DobavljacViewItem>(); }

            try
            {
                var odeljenja = await client.GetFromJsonAsync<List<OdeljenjeViewItem>>("api/odeljenja");
                model.Odeljenja = odeljenja ?? new List<OdeljenjeViewItem>();
            }
            catch { model.Odeljenja = new List<OdeljenjeViewItem>(); }

            try
            {
                var kategorije = await client.GetFromJsonAsync<List<KategorijaViewItem>>("api/kategorije");
                model.Kategorije = kategorije ?? new List<KategorijaViewItem>();
            }
            catch { model.Kategorije = new List<KategorijaViewItem>(); }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var client = _httpFactory.CreateClient("GomexApi");
            try
            {
                var artikal = await client.GetFromJsonAsync<ArtikalViewItem>($"api/artikli/{id}");
                if (artikal is null) return NotFound();
                return View(artikal);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
