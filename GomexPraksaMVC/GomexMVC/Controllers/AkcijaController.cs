using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.Controllers
{
    public class AkcijaController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public AkcijaController(
            IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        [HttpGet]
        public async Task<IActionResult> DodajAkciju(
            string? sifra)
        {
            var client =
                _httpFactory.CreateClient("GomexApi");

            var model =
                new DodajAkcijuViewModel
                {
                    SifraArtikla =
                        sifra ?? string.Empty,

                    DatumOd =
                        DateOnly.FromDateTime(
                            DateTime.Today
                        ),

                    DatumDo =
                        DateOnly.FromDateTime(
                            DateTime.Today.AddDays(7)
                        )
                };

            await UcitajTipoveAkcije(
                client,
                model
            );

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DodajAkciju(
            DodajAkcijuViewModel model)
        {
            var client =
                _httpFactory.CreateClient("GomexApi");

            if (string.IsNullOrWhiteSpace(
                model.SifraArtikla))
            {
                ModelState.AddModelError(
                    nameof(model.SifraArtikla),
                    "Šifra artikla je obavezna."
                );
            }

            if (model.DatumDo < model.DatumOd)
            {
                ModelState.AddModelError(
                    nameof(model.DatumDo),
                    "Datum do ne može biti pre datuma od."
                );
            }

            if (model.AkcijskaCena <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.AkcijskaCena),
                    "Akcijska cena mora biti veća od nule."
                );
            }

            if (model.TipAkcijeId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.TipAkcijeId),
                    "Tip akcije je obavezan."
                );
            }

            if (!ModelState.IsValid)
            {
                await UcitajTipoveAkcije(
                    client,
                    model
                );

                return View(model);
            }

            var request =
                new DodajAkcijuApiRequest
                {
                    SifraArtikla =
                        model.SifraArtikla.Trim(),

                    DatumOd =
                        model.DatumOd,

                    DatumDo =
                        model.DatumDo,

                    AkcijskaCena =
                        model.AkcijskaCena,

                    TipAkcijeId =
                        model.TipAkcijeId
                };

            try
            {
                var response =
                    await client.PostAsJsonAsync(
                        "api/akcije/dodajAkciju",
                        request
                    );

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content
                            .ReadAsStringAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        string.IsNullOrWhiteSpace(error)
                            ? "Dodavanje akcije nije uspelo."
                            : error
                    );

                    await UcitajTipoveAkcije(
                        client,
                        model
                    );

                    return View(model);
                }

                TempData["Success"] =
                    "Akcija je uspešno dodata.";

                return RedirectToAction(
                    nameof(DodajAkciju)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DODAVANJE AKCIJE: " +
                    $"{ex.Message}"
                );

                ModelState.AddModelError(
                    string.Empty,
                    "Došlo je do greške prilikom dodavanja akcije."
                );

                await UcitajTipoveAkcije(
                    client,
                    model
                );

                return View(model);
            }
        }

        public async Task<IActionResult> Index(
            string tab = "trenutne")
        {
            var akcije =
                await UcitajIPopuniAkcije(tab);

            var grupe =
                akcije
                    .GroupBy(a => a.TipAkcije)
                    .Select(g =>
                        new AkcijaGrupaViewItem
                        {
                            TipAkcije =
                                g.Key,

                            DatumOd =
                                g.Min(x => x.DatumOd),

                            DatumDo =
                                g.Max(x => x.DatumDo),

                            BrojArtikala =
                                g.Count(),

                            Artikli =
                                g.OrderByDescending(
                                        x => x.DatumOd
                                    )
                                    .ToList()
                        }
                    )
                    .OrderByDescending(
                        g => g.DatumOd
                    )
                    .ToList();

            ViewData["ActiveTab"] =
                tab;

            return View(grupe);
        }

        public async Task<IActionResult> Grupa(
            string tipAkcije,
            DateTime[]? datumOd,
            DateTime[]? datumDo,
            string? filter,
            string tab = "trenutne")
        {
            var akcije =
                await UcitajIPopuniAkcije(tab);

            var artikliUGrupi =
                akcije
                    .Where(
                        a =>
                            a.TipAkcije
                            == tipAkcije
                    )
                    .ToList();

            if (
                datumOd != null
                &&
                datumDo != null
                &&
                datumOd.Length > 0
                &&
                datumDo.Length > 0
            )
            {
                var pairs =
                    new HashSet<string>();

                var len =
                    Math.Min(
                        datumOd.Length,
                        datumDo.Length
                    );

                for (
                    int i = 0;
                    i < len;
                    i++
                )
                {
                    pairs.Add(
                        datumOd[i]
                            .Date
                            .ToString("yyyy-MM-dd")
                        +
                        "_"
                        +
                        datumDo[i]
                            .Date
                            .ToString("yyyy-MM-dd")
                    );
                }

                artikliUGrupi =
                    artikliUGrupi
                        .Where(
                            a =>
                                pairs.Contains(
                                    a.DatumOd
                                        .Date
                                        .ToString(
                                            "yyyy-MM-dd"
                                        )
                                    +
                                    "_"
                                    +
                                    a.DatumDo
                                        .Date
                                        .ToString(
                                            "yyyy-MM-dd"
                                        )
                                )
                        )
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(
                filter))
            {
                var f =
                    filter.Trim();

                artikliUGrupi =
                    artikliUGrupi
                        .Where(
                            a =>
                                (
                                    !string.IsNullOrWhiteSpace(
                                        a.ArtikalNaziv
                                    )
                                    &&
                                    a.ArtikalNaziv!
                                        .Contains(
                                            f,
                                            StringComparison
                                                .OrdinalIgnoreCase
                                        )
                                )
                                ||
                                (
                                    !string.IsNullOrWhiteSpace(
                                        a.ArtikalSifra
                                    )
                                    &&
                                    a.ArtikalSifra!
                                        .Contains(
                                            f,
                                            StringComparison
                                                .OrdinalIgnoreCase
                                        )
                                )
                        )
                        .ToList();
            }

            var grupa =
                new AkcijaGrupaViewItem
                {
                    TipAkcije =
                        tipAkcije,

                    DatumOd =
                        artikliUGrupi.Any()
                            ? artikliUGrupi
                                .Min(
                                    a => a.DatumOd
                                )
                            : DateTime.Today,

                    DatumDo =
                        artikliUGrupi.Any()
                            ? artikliUGrupi
                                .Max(
                                    a => a.DatumDo
                                )
                            : DateTime.Today,

                    BrojArtikala =
                        artikliUGrupi.Count,

                    Artikli =
                        artikliUGrupi
                };

            ViewData["Tab"] =
                tab;

            return View(grupa);
        }

        public async Task<IActionResult> Periods(
            string tipAkcije,
            string tab = "trenutne")
        {
            var akcije =
                await UcitajIPopuniAkcije(tab);

            var periods =
                akcije
                    .Where(
                        a =>
                            a.TipAkcije
                            == tipAkcije
                    )
                    .GroupBy(
                        a =>
                            new
                            {
                                Od =
                                    a.DatumOd.Date,

                                Do =
                                    a.DatumDo.Date
                            }
                    )
                    .Select(
                        g =>
                            new AkcijaPeriodViewItem
                            {
                                DatumOd =
                                    g.Key.Od,

                                DatumDo =
                                    g.Key.Do,

                                BrojArtikala =
                                    g.Count()
                            }
                    )
                    .OrderByDescending(
                        p => p.DatumOd
                    )
                    .ToList();

            ViewData["Tab"] =
                tab;

            return View(periods);
        }

        private async Task<
            List<AkcijaViewItem>
        > UcitajIPopuniAkcije(
            string tab)
        {
            var client =
                _httpFactory.CreateClient(
                    "GomexApi"
                );

            string endpoint =
                tab switch
                {
                    "buduce" =>
                        "api/akcije/buduce",

                    "trenutne" =>
                        "api/akcije/trenutne",

                    _ =>
                        "api/akcije"
                };

            List<AkcijaViewItem>
                akcije;

            try
            {
                akcije =
                    await client
                        .GetFromJsonAsync<
                            List<AkcijaViewItem>
                        >(endpoint)
                    ??
                    new List<AkcijaViewItem>();
            }
            catch
            {
                akcije =
                    new List<AkcijaViewItem>();
            }

            if (tab == "prethodne")
            {
                akcije =
                    akcije
                        .Where(
                            a =>
                                a.DatumDo
                                < DateTime.Today
                        )
                        .ToList();
            }

            if (!akcije.Any())
            {
                return akcije;
            }

            List<ArtikalViewItem>
                sviArtikli;

            try
            {
                var paged =
                    await client
                        .GetFromJsonAsync<
                            PaginationResponse<
                                ArtikalViewItem
                            >
                        >(
                            "api/artikli?page=1&pageSize=100"
                        );

                sviArtikli =
                    paged?.Items
                    ??
                    new List<ArtikalViewItem>();
            }
            catch
            {
                sviArtikli =
                    new List<ArtikalViewItem>();
            }

            var artikalCache =
                sviArtikli
                    .Where(
                        a => a != null
                    )
                    .GroupBy(
                        a => a.ArtikalId
                    )
                    .ToDictionary(
                        g => g.Key,
                        g => g.First()
                    );

            foreach (
                var akcija in akcije)
            {
                if (
                    artikalCache
                        .TryGetValue(
                            akcija.ArtikalId,
                            out var artikal
                        )
                )
                {
                    akcija.ArtikalNaziv =
                        artikal.Naziv;

                    akcija.ArtikalSifra =
                        artikal.Sifra;

                    akcija.RedovnaCena =
                        artikal.RedovnaCena;
                }
            }

            var missingIds =
                akcije
                    .Where(
                        a =>
                            string.IsNullOrWhiteSpace(
                                a.ArtikalNaziv
                            )
                            &&
                            a.ArtikalId > 0
                    )
                    .Select(
                        a => a.ArtikalId
                    )
                    .Distinct()
                    .ToList();

            if (missingIds.Any())
            {
                foreach (
                    var id in missingIds)
                {
                    try
                    {
                        var single =
                            await client
                                .GetFromJsonAsync<
                                    ArtikalViewItem
                                >(
                                    $"api/artikli/{id}"
                                );

                        if (single != null)
                        {
                            artikalCache[id] =
                                single;
                        }
                    }
                    catch
                    {
                    }
                }

                foreach (
                    var akcija in akcije)
                {
                    if (
                        string.IsNullOrWhiteSpace(
                            akcija.ArtikalNaziv
                        )
                        &&
                        artikalCache.TryGetValue(
                            akcija.ArtikalId,
                            out var art
                        )
                    )
                    {
                        akcija.ArtikalNaziv =
                            art.Naziv;

                        akcija.ArtikalSifra =
                            art.Sifra;

                        akcija.RedovnaCena =
                            art.RedovnaCena;
                    }
                }
            }

            return akcije;
        }

        private async Task UcitajTipoveAkcije(
            HttpClient client,
            DodajAkcijuViewModel model)
        {
            try
            {
                var tipovi =
                    await client
                        .GetFromJsonAsync<
                            List<TipAkcijeViewItem>
                        >(
                            "api/tipakcije"
                        );

                model.TipoviAkcija =
                    tipovi
                    ??
                    new List<TipAkcijeViewItem>();
            }
            catch
            {
                model.TipoviAkcija =
                    new List<TipAkcijeViewItem>();
            }
        }

        private class DodajAkcijuApiRequest
        {
            public string SifraArtikla { get; set; }
                = string.Empty;

            public DateOnly DatumOd { get; set; }

            public DateOnly DatumDo { get; set; }

            public decimal AkcijskaCena { get; set; }

            public int TipAkcijeId { get; set; }
        }
    }
}