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

        public async Task<IActionResult> Index(int? dobavljacId, int? robnaGrupaId)
        {
            var client = _httpFactory.CreateClient("GomexApi");

            var queryParts = new List<string>();
            if (dobavljacId.HasValue) queryParts.Add($"dobavljacId={dobavljacId.Value}");
            if (robnaGrupaId.HasValue) queryParts.Add($"robnaGrupaId={robnaGrupaId.Value}");
            var query = queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;

            List<ArtikalViewItem> artikli = new();
            try
            {
                var result = await client.GetFromJsonAsync<List<ArtikalViewItem>>($"api/artikli/search{query}");
                artikli = result ?? new List<ArtikalViewItem>();
            }
            catch
            {
            }

            return View(artikli);
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
