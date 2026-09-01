using GomexPraksaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GomexPraksaMVC.GomexMVC.Controllers
{
    [TypeFilter(typeof(GomexPraksaMVC.Filters.RequireAuthFilter))]
    public class ArtikalController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public ArtikalController(
            IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        // ==============================
        // ARTIKLI
        // ==============================

        public async Task<IActionResult> Index(
            int? dobavljacId,
            int? robnaGrupaId,
            int? odeljenjeId,
            int? kategorijaId,
            string? naziv,
            string? sifra,
            int page = 1,
            int pageSize = 10)
        {
            var client =
                _httpFactory.CreateClient("GomexApi");

            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var model = new ArtikalIndexViewModel
            {
                SelectedDobavljacId = dobavljacId,
                SelectedRobnaGrupaId = robnaGrupaId,
                SelectedOdeljenjeId = odeljenjeId,
                SelectedKategorijaId = kategorijaId,
                Naziv = naziv,

                Page = page,
                PageSize = pageSize
            };

            // ==============================
            // ARTIKLI
            // filter + pagination
            // ==============================

            try
            {
                var queryParts =
                    new List<string>();

                queryParts.Add(
                    $"page={page}"
                );

                queryParts.Add(
                    $"pageSize={pageSize}"
                );

                if (!string.IsNullOrWhiteSpace(naziv))
                {
                    queryParts.Add(
                        $"naziv={Uri.EscapeDataString(naziv)}"
                    );
                }

                if (dobavljacId.HasValue)
                {
                    queryParts.Add(
                        $"dobavljacId={dobavljacId.Value}"
                    );
                }

                if (robnaGrupaId.HasValue)
                {
                    queryParts.Add(
                        $"robnaGrupaId={robnaGrupaId.Value}"
                    );
                }

                if (odeljenjeId.HasValue)
                {
                    queryParts.Add(
                        $"odeljenjeId={odeljenjeId.Value}"
                    );
                }

                if (!string.IsNullOrWhiteSpace(sifra))
                {
                    queryParts.Add($"sifra={Uri.EscapeDataString(sifra)}");
                }

                var query =
                    "?" + string.Join(
                        "&",
                        queryParts
                    );

                var url =
                    $"api/artikli/search{query}";

                Console.WriteLine(
                    $"ARTIKLI URL: {url}"
                );

                var result =
                    await client.GetFromJsonAsync<
                        PaginationResponse<ArtikalViewItem>
                    >(url);

                if (result != null)
                {
                    model.Artikli =
                        result.Items;

                    model.Page =
                        result.Page;

                    model.PageSize =
                        result.PageSize;

                    model.TotalCount =
                        result.TotalCount;

                    model.TotalPages =
                        result.TotalPages;

                    model.HasPreviousPage =
                        result.HasPreviousPage;

                    model.HasNextPage =
                        result.HasNextPage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA ARTIKLI: {ex.Message}"
                );

                model.Artikli =
                    new List<ArtikalViewItem>();

                model.TotalCount = 0;
                model.TotalPages = 0;
                model.HasPreviousPage = false;
                model.HasNextPage = false;
            }

            // ==============================
            // DOBAVLJACI
            // ==============================

            try
            {
                var paged = await client.GetFromJsonAsync<PaginationResponse<DobavljacViewItem>>("api/dobavljaci");
                model.Dobavljaci = paged?.Items ?? new List<DobavljacViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DOBAVLJACI: {ex.Message}"
                );

                model.Dobavljaci =
                    new List<DobavljacViewItem>();
            }

            // ==============================
            // ODELJENJA
            // ==============================

            try
            {
                var odeljenja =
                    await client.GetFromJsonAsync<
                        List<OdeljenjeViewItem>
                    >(
                        "api/odeljenja"
                    );

                model.Odeljenja =
                    odeljenja
                    ?? new List<OdeljenjeViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA ODELJENJA: {ex.Message}"
                );

                model.Odeljenja =
                    new List<OdeljenjeViewItem>();
            }

            // ==============================
            // KATEGORIJE
            // ==============================

            try
            {
                var kategorije =
                    await client.GetFromJsonAsync<
                        List<KategorijaViewItem>
                    >(
                        "api/kategorije"
                    );

                model.Kategorije =
                    kategorije
                    ?? new List<KategorijaViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA KATEGORIJE: {ex.Message}"
                );

                model.Kategorije =
                    new List<KategorijaViewItem>();
            }

            return View(model);
        }

        // ==============================
        // KRITICNI ARTIKLI
        // ==============================

        public async Task<IActionResult> Kriticni(
            DateTime? datumOd,
            DateTime? datumDo,
            int? odeljenjeId,
            int? kategorijaId,
            int? dobavljacId,
            int? tipProdajeId)
        {
            var client =
                _httpFactory.CreateClient("GomexApi");

            DateOnly datumStart;
            DateOnly datumEnd;

            if (!datumOd.HasValue && !datumDo.HasValue)
            {
                datumEnd =
                    DateOnly.FromDateTime(DateTime.Today);

                datumStart =
                    datumEnd.AddDays(-29);
            }
            else if (datumOd.HasValue && !datumDo.HasValue)
            {
                datumStart =
                    DateOnly.FromDateTime(datumOd.Value);

                datumEnd =
                    DateOnly.FromDateTime(DateTime.Today);
            }
            else if (!datumOd.HasValue && datumDo.HasValue)
            {
                datumEnd =
                    DateOnly.FromDateTime(datumDo.Value);

                datumStart =
                    datumEnd.AddDays(-29);
            }
            else
            {
                datumStart =
                    DateOnly.FromDateTime(datumOd!.Value);

                datumEnd =
                    DateOnly.FromDateTime(datumDo!.Value);
            }

            if (datumStart > datumEnd)
            {
                datumEnd =
                    DateOnly.FromDateTime(DateTime.Today);

                datumStart =
                    datumEnd.AddDays(-29);
            }

            var model =
                new List<CriticalProductPageViewItem>();

            try
            {
                var queryParts = new List<string>
                {
                    $"datumOd={datumStart:yyyy-MM-dd}",
                    $"datumDo={datumEnd:yyyy-MM-dd}"
                };

                if (odeljenjeId.HasValue)
                {
                    queryParts.Add(
                        $"odeljenjeId={odeljenjeId.Value}"
                    );
                }

                if (kategorijaId.HasValue)
                {
                    queryParts.Add(
                        $"kategorijaId={kategorijaId.Value}"
                    );
                }

                if (dobavljacId.HasValue)
                {
                    queryParts.Add(
                        $"dobavljacId={dobavljacId.Value}"
                    );
                }

                if (tipProdajeId.HasValue)
                {
                    queryParts.Add(
                        $"tipProdajeId={tipProdajeId.Value}"
                    );
                }

                var query =
                    "?" + string.Join(
                        "&",
                        queryParts
                    );

                var url =
                    $"api/artikli/CriticalPage{query}";

                Console.WriteLine(
                    $"KRITICNI URL: {url}"
                );

                model =
                    await client.GetFromJsonAsync<
                        List<CriticalProductPageViewItem>
                    >(url)
                    ?? new List<CriticalProductPageViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA KRITICNI ARTIKLI: {ex.Message}"
                );

                model =
                    new List<CriticalProductPageViewItem>();
            }

            ViewData["DatumOd"] =
                datumStart;

            ViewData["DatumDo"] =
                datumEnd;

            ViewData["OdeljenjeId"] =
                odeljenjeId;

            ViewData["KategorijaId"] =
                kategorijaId;

            ViewData["DobavljacId"] =
                dobavljacId;

            ViewData["TipProdajeId"] =
                tipProdajeId;

            // BITNO:
            // View fajl se zove KriticniArtikli.cshtml
            return View(
                "KriticniArtikli",
                model
            );
        }

        // ==============================
        // DETALJI
        // ==============================

        public async Task<IActionResult> Detalji(
            string sifra)
        {
            if (string.IsNullOrWhiteSpace(sifra))
            {
                return BadRequest();
            }

            var client =
                _httpFactory.CreateClient("GomexApi");

            try
            {
                var artikal =
                    await client.GetFromJsonAsync<
                        ArtikalViewItem
                    >(
                        $"api/artikli/sifra/{sifra}"
                    );

                if (artikal is null)
                {
                    return NotFound();
                }

                // ==============================
                // DOBAVLJAC
                // ==============================

                DobavljacViewItem? dobavljac = null;

                try
                {
                    dobavljac =
                        await client.GetFromJsonAsync<
                            DobavljacViewItem
                        >(
                            $"api/dobavljaci/{artikal.DobavljacId}"
                        );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"GRESKA DOBAVLJAC DETALJI: {ex.Message}"
                    );
                }

                // ==============================
                // AKTIVNA AKCIJA
                // ==============================

                AkcijaViewItem? aktivnaAkcija = null;

                try
                {
                    var akcije =
                        await client.GetFromJsonAsync<
                            List<AkcijaViewItem>
                        >(
                            $"api/akcije/artikal/{artikal.ArtikalId}"
                        );

                    aktivnaAkcija =
                        akcije?.FirstOrDefault(
                            a =>
                                a.DatumOd.Date
                                    <= DateTime.Today
                                &&
                                a.DatumDo.Date
                                    >= DateTime.Today
                        );

                    if (aktivnaAkcija != null)
                    {
                        aktivnaAkcija.RedovnaCena =
                            artikal.RedovnaCena;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"GRESKA AKCIJE DETALJI: {ex.Message}"
                    );
                }

                // ==============================
                // RUC PODACI ARTIKLA
                // ==============================

                CriticalProductPageViewItem?
                    rucPodaci = null;

                try
                {
                    var datumDo =
                        DateTime.Today;

                    var datumOd =
                        datumDo.AddDays(-29);

                    var sviPodaci =
                        await client.GetFromJsonAsync<
                            List<CriticalProductPageViewItem>
                        >(
                            $"api/artikli/CriticalPage" +
                            $"?datumOd={datumOd:yyyy-MM-dd}" +
                            $"&datumDo={datumDo:yyyy-MM-dd}"
                        );

                    rucPodaci =
                        sviPodaci?.FirstOrDefault(
                            p =>
                                p.ArtikalId
                                == artikal.ArtikalId
                        );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"GRESKA RUC DETALJI: {ex.Message}"
                    );
                }

                ViewData["DobavljacNaziv"] =
                    dobavljac?.Naziv;

                ViewData["AktivnaAkcija"] =
                    aktivnaAkcija;

                ViewData["RucPodaci"] =
                    rucPodaci;

                return View(artikal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DETALJI ARTIKLA: {ex.Message}"
                );

                return StatusCode(500);
            }
        }
    }
}