using GomexPraksaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GomexPraksaMVC.Controllers
{
    [TypeFilter(typeof(GomexPraksaMVC.Filters.RequireAuthFilter))]
    public class DobavljacController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public DobavljacController(
            IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        // =====================================================
        // LISTA DOBAVLJACA
        // =====================================================

        public async Task<IActionResult> Index(
            string? naziv,
            int page = 1,
            int pageSize = 10)
        {
            var client =
                _httpFactory.CreateClient("GomexApi");

            // -----------------------------
            // VALIDACIJA PAGINACIJE
            // -----------------------------

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

            var model =
                new DobavljacIndexViewModel
                {
                    Naziv = naziv,
                    Page = page,
                    PageSize = pageSize
                };

            try
            {
                // -----------------------------
                // QUERY
                // -----------------------------

                var queryParts =
                    new List<string>
                    {
                        $"page={page}",
                        $"pageSize={pageSize}"
                    };

                string url;

                if (string.IsNullOrWhiteSpace(naziv))
                {
                    url =
                        $"api/dobavljaci?" +
                        string.Join("&", queryParts);
                }
                else
                {
                    queryParts.Add(
                        $"naziv={Uri.EscapeDataString(naziv.Trim())}"
                    );

                    url =
                        $"api/dobavljaci/search?" +
                        string.Join("&", queryParts);
                }

                Console.WriteLine(
                    $"DOBAVLJACI URL: {url}"
                );

                var result =
                    await client.GetFromJsonAsync<
                        PaginationResponse<DobavljacViewItem>
                    >(url);

                if (result != null)
                {
                    model.Dobavljaci =
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
                    $"GRESKA DOBAVLJACI: {ex.Message}"
                );

                model.Dobavljaci =
                    new List<DobavljacViewItem>();

                model.TotalCount = 0;
                model.TotalPages = 0;
                model.HasPreviousPage = false;
                model.HasNextPage = false;
            }

            return View(model);
        }

        // =====================================================
        // DETALJI DOBAVLJACA
        // =====================================================

        public async Task<IActionResult> Detalji(
            int id,
            int page = 1,
            int pageSize = 10)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

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

            var client =
                _httpFactory.CreateClient("GomexApi");

            // =================================================
            // DOBAVLJAC
            // =================================================

            DobavljacViewItem? dobavljac;

            try
            {
                dobavljac =
                    await client.GetFromJsonAsync<
                        DobavljacViewItem
                    >(
                        $"api/dobavljaci/{id}"
                    );
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"GRESKA DOBAVLJAC DETALJI: {ex.Message}"
                );

                return StatusCode(500);
            }

            if (dobavljac is null)
            {
                return NotFound();
            }

            var model =
                new DobavljacDetaljiViewModel
                {
                    Dobavljac = dobavljac,

                    Page = page,

                    PageSize = pageSize
                };

            // =================================================
            // ARTIKLI DOBAVLJACA
            // =================================================
            //
            // Koristimo vec postojeci endpoint:
            //
            // api/artikli/search
            //
            // i filtriramo preko dobavljacId.
            // =================================================

            try
            {
                var url =
                    $"api/artikli/search" +
                    $"?dobavljacId={id}" +
                    $"&page={page}" +
                    $"&pageSize={pageSize}";

                Console.WriteLine(
                    $"ARTIKLI DOBAVLJACA URL: {url}"
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
                // Dobavljac i dalje moze da se prikaze
                // čak i ako artikli puknu.

                Console.WriteLine(
                    $"GRESKA ARTIKLI DOBAVLJACA: {ex.Message}"
                );

                model.Artikli =
                    new List<ArtikalViewItem>();

                model.TotalCount = 0;
                model.TotalPages = 0;
                model.HasPreviousPage = false;
                model.HasNextPage = false;
            }

            return View(model);
        }
    }
}