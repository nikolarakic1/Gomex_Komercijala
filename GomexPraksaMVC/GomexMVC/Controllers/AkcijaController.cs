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

            // Grupisemo po tipu akcije (ime akcije). Datum opsega grupe je min/max datuma iz svih stavki
            var grupe = akcije
                .GroupBy(a => a.TipAkcije)
                .Select(g => new AkcijaGrupaViewItem
                {
                    TipAkcije = g.Key,
                    DatumOd = g.Min(x => x.DatumOd),
                    DatumDo = g.Max(x => x.DatumDo),
                    BrojArtikala = g.Count(),
                    Artikli = g.OrderByDescending(x => x.DatumOd).ToList()
                })
                .OrderByDescending(g => g.DatumOd)
                .ToList();

            ViewData["ActiveTab"] = tab;
            return View(grupe);
        }

        public async Task<IActionResult> Grupa(string tipAkcije, DateTime[]? datumOd, DateTime[]? datumDo, string? filter, string tab = "trenutne")
        {
            var akcije = await UcitajIPopuniAkcije(tab);

            var artikliUGrupi = akcije
                .Where(a => a.TipAkcije == tipAkcije)
                .ToList();

            // If specific periods were provided, filter to only those periods (matching by Date)
            if (datumOd != null && datumDo != null && datumOd.Length > 0 && datumDo.Length > 0)
            {
                var pairs = new HashSet<string>();
                var len = Math.Min(datumOd.Length, datumDo.Length);
                for (int i = 0; i < len; i++)
                {
                    pairs.Add(datumOd[i].Date.ToString("yyyy-MM-dd") + "_" + datumDo[i].Date.ToString("yyyy-MM-dd"));
                }

                artikliUGrupi = artikliUGrupi
                    .Where(a => pairs.Contains(a.DatumOd.Date.ToString("yyyy-MM-dd") + "_" + a.DatumDo.Date.ToString("yyyy-MM-dd")))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var f = filter.Trim();
                artikliUGrupi = artikliUGrupi
                    .Where(a => (!string.IsNullOrWhiteSpace(a.ArtikalNaziv) && a.ArtikalNaziv!.Contains(f, StringComparison.OrdinalIgnoreCase))
                                || (!string.IsNullOrWhiteSpace(a.ArtikalSifra) && a.ArtikalSifra!.Contains(f, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // If there are no items after filtering, return an empty group instead of 404 to avoid crashes
            var grupa = new AkcijaGrupaViewItem
            {
                TipAkcije = tipAkcije,
                DatumOd = artikliUGrupi.Any() ? artikliUGrupi.Min(a => a.DatumOd) : DateTime.Today,
                DatumDo = artikliUGrupi.Any() ? artikliUGrupi.Max(a => a.DatumDo) : DateTime.Today,
                BrojArtikala = artikliUGrupi.Count,
                Artikli = artikliUGrupi
            };

            ViewData["Tab"] = tab;
            return View(grupa);
        }

        public async Task<IActionResult> Periods(string tipAkcije, string tab = "trenutne")
        {
            var akcije = await UcitajIPopuniAkcije(tab);

            var periods = akcije
                .Where(a => a.TipAkcije == tipAkcije)
                .GroupBy(a => new { Od = a.DatumOd.Date, Do = a.DatumDo.Date })
                .Select(g => new AkcijaPeriodViewItem
                {
                    DatumOd = g.Key.Od,
                    DatumDo = g.Key.Do,
                    BrojArtikala = g.Count()
                })
                .OrderByDescending(p => p.DatumOd)
                .ToList();

            ViewData["Tab"] = tab;
            return View(periods);
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

            // Avoid exceptions if API returns duplicates or zero ids: group and pick first
            var artikalCache = sviArtikli
                .Where(a => a != null)
                .GroupBy(a => a.ArtikalId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var akcija in akcije)
            {
                if (artikalCache.TryGetValue(akcija.ArtikalId, out var artikal))
                {
                    akcija.ArtikalNaziv = artikal.Naziv;
                    akcija.ArtikalSifra = artikal.Sifra;
                    akcija.RedovnaCena = artikal.RedovnaCena;
                }
            }

            // If some artikli still have no naziv (possible JSON mismatch or filtered list),
            // attempt per-id fetch for missing ArtikalId values (skip zeros)
            var missingIds = akcije
                .Where(a => string.IsNullOrWhiteSpace(a.ArtikalNaziv) && a.ArtikalId > 0)
                .Select(a => a.ArtikalId)
                .Distinct()
                .ToList();

            if (missingIds.Any())
            {
                foreach (var id in missingIds)
                {
                    try
                    {
                        var single = await client.GetFromJsonAsync<ArtikalViewItem>($"api/artikli/{id}");
                        if (single != null)
                        {
                            artikalCache[id] = single;
                        }
                    }
                    catch
                    {
                        // ignore individual failures
                    }
                }

                // apply any newly fetched values
                foreach (var akcija in akcije)
                {
                    if (string.IsNullOrWhiteSpace(akcija.ArtikalNaziv) && artikalCache.TryGetValue(akcija.ArtikalId, out var art))
                    {
                        akcija.ArtikalNaziv = art.Naziv;
                        akcija.ArtikalSifra = art.Sifra;
                        akcija.RedovnaCena = art.RedovnaCena;
                    }
                }
            }

            return akcije;
        }
    }
}