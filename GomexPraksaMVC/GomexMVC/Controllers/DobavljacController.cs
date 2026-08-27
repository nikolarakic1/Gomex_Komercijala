using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.Controllers
{
    public class DobavljacController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public DobavljacController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index(string? naziv)
        {
            var client = _httpFactory.CreateClient("GomexApi");

            var query = string.IsNullOrWhiteSpace(naziv)
                ? "api/dobavljaci"
                : $"api/dobavljaci/search?naziv={Uri.EscapeDataString(naziv)}";

            List<DobavljacViewItem> dobavljaci;
            try
            {
                dobavljaci = await client.GetFromJsonAsync<List<DobavljacViewItem>>(query)
                    ?? new List<DobavljacViewItem>();
            }
            catch
            {
                dobavljaci = new List<DobavljacViewItem>();
            }

            ViewData["Naziv"] = naziv;
            return View(dobavljaci);
        }

        public async Task<IActionResult> Detalji(int id)
        {
            if (id <= 0) return BadRequest();

            var client = _httpFactory.CreateClient("GomexApi");

            DobavljacViewItem? dobavljac;
            try
            {
                dobavljac = await client.GetFromJsonAsync<DobavljacViewItem>($"api/dobavljaci/{id}");
            }
            catch
            {
                return StatusCode(500);
            }

            if (dobavljac is null) return NotFound();

            return View(dobavljac);
        }
    }
}