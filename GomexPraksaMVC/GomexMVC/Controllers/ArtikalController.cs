using GomexPraksaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GomexPraksaMVC.GomexMVC.Controllers
{
    [TypeFilter(typeof(GomexPraksaMVC.Filters.RequireAuthFilter))]
    public class ArtikalController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public ArtikalController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

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
            var client = _httpFactory.CreateClient("GomexApi");

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
                Sifra = sifra,
                Page = page,
                PageSize = pageSize
            };

            try
            {
                var queryParts = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

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

                if (kategorijaId.HasValue)
                {
                    queryParts.Add(
                        $"kategorijaId={kategorijaId.Value}"
                    );
                }

                if (!string.IsNullOrWhiteSpace(sifra))
                {
                    queryParts.Add(
                        $"sifra={Uri.EscapeDataString(sifra)}"
                    );
                }

                var query =
                    "?" + string.Join("&", queryParts);

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

            try
            {
                var paged =
                    await client.GetFromJsonAsync<
                        PaginationResponse<DobavljacViewItem>
                    >(
                        "api/dobavljaci"
                    );

                model.Dobavljaci =
                    paged?.Items
                    ?? new List<DobavljacViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DOBAVLJACI: {ex.Message}"
                );

                model.Dobavljaci =
                    new List<DobavljacViewItem>();
            }

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

        public async Task<IActionResult> Kriticni(
            DateTime? datumOd,
            DateTime? datumDo,
            int? odeljenjeId,
            int? kategorijaId,
            int? dobavljacId,
            int? tipProdajeId,
            int page = 1,
            int pageSize = 10)
        {
            var client =
                _httpFactory.CreateClient(
                    "GomexApi"
                );

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

            DateOnly datumStart;
            DateOnly datumEnd;

            if (!datumOd.HasValue &&
                !datumDo.HasValue)
            {
                datumEnd =
                    DateOnly.FromDateTime(
                        DateTime.Today
                    );

                datumStart =
                    datumEnd.AddDays(-29);
            }
            else if (
                datumOd.HasValue &&
                !datumDo.HasValue)
            {
                datumStart =
                    DateOnly.FromDateTime(
                        datumOd.Value
                    );

                datumEnd =
                    DateOnly.FromDateTime(
                        DateTime.Today
                    );
            }
            else if (
                !datumOd.HasValue &&
                datumDo.HasValue)
            {
                datumEnd =
                    DateOnly.FromDateTime(
                        datumDo.Value
                    );

                datumStart =
                    datumEnd.AddDays(-29);
            }
            else
            {
                datumStart =
                    DateOnly.FromDateTime(
                        datumOd!.Value
                    );

                datumEnd =
                    DateOnly.FromDateTime(
                        datumDo!.Value
                    );
            }

            if (datumStart > datumEnd)
            {
                datumEnd =
                    DateOnly.FromDateTime(
                        DateTime.Today
                    );

                datumStart =
                    datumEnd.AddDays(-29);
            }

            var model =
                new PaginationResponse<
                    CriticalProductPageViewItem
                >
                {
                    Items =
                        new List<
                            CriticalProductPageViewItem
                        >(),

                    Page =
                        page,

                    PageSize =
                        pageSize,

                    TotalCount =
                        0,

                    TotalPages =
                        0,

                    HasPreviousPage =
                        false,

                    HasNextPage =
                        false
                };

            try
            {
                var queryParts =
                    new List<string>
                    {
                        $"datumOd={datumStart:yyyy-MM-dd}",
                        $"datumDo={datumEnd:yyyy-MM-dd}",
                        $"page={page}",
                        $"pageSize={pageSize}"
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
                    "?" +
                    string.Join(
                        "&",
                        queryParts
                    );

                var url =
                    $"api/artikli/CriticalPage{query}";

                Console.WriteLine(
                    $"KRITICNI URL: {url}"
                );

                var result =
                    await client.GetFromJsonAsync<
                        PaginationResponse<
                            CriticalProductPageViewItem
                        >
                    >(url);

                if (result != null)
                {
                    model = result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA KRITICNI ARTIKLI: {ex.Message}"
                );
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

            return View(
                "KriticniArtikli",
                model
            );
        }

        public async Task<IActionResult> Detalji(
            string sifra)
        {
            if (string.IsNullOrWhiteSpace(sifra))
            {
                return BadRequest();
            }

            var client =
                _httpFactory.CreateClient(
                    "GomexApi"
                );

            try
            {
                var artikal =
                    await client.GetFromJsonAsync<
                        ArtikalViewItem
                    >(
                        $"api/artikli/sifra/" +
                        $"{Uri.EscapeDataString(sifra)}"
                    );

                if (artikal == null)
                {
                    return NotFound();
                }

                AkcijaViewItem?
                    aktivnaAkcija = null;

                try
                {
                    var akcije =
                        await client.GetFromJsonAsync<
                            List<AkcijaViewItem>
                        >(
                            $"api/akcije/artikal/" +
                            $"{artikal.ArtikalId}"
                        );

                    aktivnaAkcija =
                        akcije?
                            .FirstOrDefault(
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
                        $"GRESKA AKCIJE DETALJI: " +
                        $"{ex.Message}"
                    );
                }

                ViewData["AktivnaAkcija"] =
                    aktivnaAkcija;

                return View(artikal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DETALJI ARTIKLA: " +
                    $"{ex.Message}"
                );

                return StatusCode(500);
            }
        }
    }
}