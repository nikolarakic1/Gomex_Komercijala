using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.Controllers
{
    public class AkcijaController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public AkcijaController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index(string tab = "trenutne")
        {
            var akcije = await UcitajIPopuniAkcije(tab);

            var grupe = akcije
                .GroupBy(a => new { a.TipAkcije, a.DatumOd, a.DatumDo })
                .Select(g => new AkcijaGrupaViewItem
                {
                    TipAkcije = g.Key.TipAkcije,
                    DatumOd = g.Key.DatumOd,
                    DatumDo = g.Key.DatumDo,
                    BrojArtikala = g.Count(),
                    Artikli = g.ToList()
                })
                .OrderByDescending(g => g.DatumOd)
                .ToList();

            ViewData["ActiveTab"] = tab;
            return View(grupe);
        }

        public async Task<IActionResult> Grupa(string tipAkcije, DateTime datumOd, DateTime datumDo, string tab = "trenutne")
        {
            var akcije = await UcitajIPopuniAkcije(tab);

            var artikliUGrupi = akcije
                .Where(a => a.TipAkcije == tipAkcije
                    && a.DatumOd.Date == datumOd.Date
                    && a.DatumDo.Date == datumDo.Date)
                .ToList();

            if (!artikliUGrupi.Any())
            {
                return NotFound();
            }

            var grupa = new AkcijaGrupaViewItem
            {
                TipAkcije = tipAkcije,
                DatumOd = datumOd,
                DatumDo = datumDo,
                BrojArtikala = artikliUGrupi.Count,
                Artikli = artikliUGrupi
            };

            ViewData["Tab"] = tab;
            return View(grupa);
        }

        private async Task<List<AkcijaViewItem>> UcitajIPopuniAkcije(string tab)
        {
            var client = _httpFactory.CreateClient("GomexApi");

            string endpoint = tab switch
            {
                "buduce" => "api/akcije/buduce",
                "trenutne" => "api/akcije/trenutne",
                _ => "api/akcije"
            };

            List<AkcijaViewItem> akcije;
            try
            {
                akcije = await client.GetFromJsonAsync<List<AkcijaViewItem>>(endpoint)
                    ?? new List<AkcijaViewItem>();
            }
            catch
            {
                akcije = new List<AkcijaViewItem>();
            }

            if (tab == "prethodne")
            {
                akcije = akcije.Where(a => a.DatumDo < DateTime.Today).ToList();
            }

            if (!akcije.Any())
            {
                return akcije;
            }

            // Jedan poziv za SVE artikle, umesto po jedan poziv za svaki jedinstven ArtikalId
            List<ArtikalViewItem> sviArtikli;
            try
            {
                sviArtikli = await client.GetFromJsonAsync<List<ArtikalViewItem>>("api/artikli")
                    ?? new List<ArtikalViewItem>();
            }
            catch
            {
                sviArtikli = new List<ArtikalViewItem>();
            }

            var artikalCache = sviArtikli.ToDictionary(a => a.ArtikalId);

            foreach (var akcija in akcije)
            {
                if (artikalCache.TryGetValue(akcija.ArtikalId, out var artikal))
                {
                    akcija.ArtikalNaziv = artikal.Naziv;
                    akcija.ArtikalSifra = artikal.Sifra;
                    akcija.RedovnaCena = artikal.RedovnaCena;
                }
            }

            return akcije;
        }
    }
}