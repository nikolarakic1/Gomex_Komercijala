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

        public async Task<IActionResult> Index(
            int? dobavljacId,
            int? robnaGrupaId,
            int? odeljenjeId,
            int? kategorijaId,
            string? naziv,
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
            // ==============================

            try
            {
                var url =
                    $"api/artikli?page={page}&pageSize={pageSize}";

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
            }


            // ==============================
            // DOBAVLJACI
            // ==============================

            try
            {
                var dobavljaci =
                    await client.GetFromJsonAsync<
                        List<DobavljacViewItem>
                    >(
                        "api/dobavljaci"
                    );

                model.Dobavljaci =
                    dobavljaci
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
                catch
                {
                }


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
                catch
                {
                }


                CriticalProductPageViewItem?
                    rucPodaci = null;

                try
                {
                    var datumDo =
                        DateTime.Today;

                    var datumOd =
                        new DateTime(
                            datumDo.Year,
                            1,
                            1
                        );

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
                catch
                {
                }


                ViewData["DobavljacNaziv"] =
                    dobavljac?.Naziv;

                ViewData["AktivnaAkcija"] =
                    aktivnaAkcija;

                ViewData["RucPodaci"] =
                    rucPodaci;


                return View(artikal);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}