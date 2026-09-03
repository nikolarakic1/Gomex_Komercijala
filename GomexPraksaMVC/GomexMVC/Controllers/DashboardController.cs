using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.Controllers
{
    [TypeFilter(typeof(GomexPraksaMVC.Filters.RequireAuthFilter))]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public DashboardController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index(
            DateTime? datumOd,
            DateTime? datumDo,
            int? odeljenjeId,
            int? kategorijaId,
            int? dobavljacId,
            int? tipProdajeId)
        {
            var client = _httpFactory.CreateClient("GomexApi");

            // Jedan isti period koristi ceo dashboard.
            DateOnly datumStart;
            DateOnly datumEnd;

            if (!datumOd.HasValue && !datumDo.HasValue)
            {
                // Default: poslednjih 30 dana.
                datumEnd = DateOnly.FromDateTime(DateTime.Today);
                datumStart = datumEnd.AddDays(-29);
            }
            else if (datumOd.HasValue && !datumDo.HasValue)
            {
                // Ako je unet samo DatumOd,
                // DatumDo je danas.
                datumStart = DateOnly.FromDateTime(datumOd.Value);
                datumEnd = DateOnly.FromDateTime(DateTime.Today);
            }
            else if (!datumOd.HasValue && datumDo.HasValue)
            {
                // Ako je unet samo DatumDo,
                // uzimamo 30 dana zakljucno sa tim datumom.
                datumEnd = DateOnly.FromDateTime(datumDo.Value);
                datumStart = datumEnd.AddDays(-29);
            }
            else
            {
                datumStart = DateOnly.FromDateTime(datumOd!.Value);
                datumEnd = DateOnly.FromDateTime(datumDo!.Value);
            }

            if (datumStart > datumEnd)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Datum od ne može biti posle datuma do.");

                // Ako je period neispravan,
                // vrati default poslednjih 30 dana.
                datumEnd = DateOnly.FromDateTime(DateTime.Today);
                datumStart = datumEnd.AddDays(-29);
            }

            var model = new DashboardViewModel
            {
                DatumOd = datumStart,
                DatumDo = datumEnd,
                OdeljenjeId = odeljenjeId,
                KategorijaId = kategorijaId,
                SelectedDobavljacId = dobavljacId,
                TipProdajeId = tipProdajeId
            };

            // =============================================
            // DASHBOARD SUMMARY
            // =============================================

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
                        $"odeljenjeId={odeljenjeId.Value}");
                }

                if (kategorijaId.HasValue)
                {
                    queryParts.Add(
                        $"kategorijaId={kategorijaId.Value}");
                }

                if (dobavljacId.HasValue)
                {
                    queryParts.Add(
                        $"dobavljacId={dobavljacId.Value}");
                }

                if (tipProdajeId.HasValue)
                {
                    queryParts.Add(
                        $"tipProdajeId={tipProdajeId.Value}");
                }

                var query =
                    "?" + string.Join("&", queryParts);

                var summary =
                    await client.GetFromJsonAsync<DashboardViewModel>(
                        $"api/dashboard/summary{query}");

                if (summary != null)
                {
                    model.PrometBezPdv =
                        summary.PrometBezPdv;

                    model.PrometPromenaProcenat =
                        summary.PrometPromenaProcenat;

                    model.Ruc12 =
                        summary.Ruc12;

                    model.Ruc12PromenaProcenat =
                        summary.Ruc12PromenaProcenat;

                    model.Ruc12Procenat =
                        summary.Ruc12Procenat;

                    model.Ruc12PromenaProcentniPoeni =
                        summary.Ruc12PromenaProcentniPoeni;

                    model.KriticniArtikli =
                        summary.KriticniArtikli;

                    model.KriticniArtikliPromena =
                        summary.KriticniArtikliPromena;

                    model.NedostatakMarze =
                        summary.NedostatakMarze;

                    model.NedostatakMarzePromenaProcenat =
                        summary.NedostatakMarzePromenaProcenat;

                    model.PodaciOsvezeni =
                        summary.PodaciOsvezeni;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DASHBOARD SUMMARY: {ex.Message}");
            }

            // =============================================
            // DOBAVLJACI
            // =============================================

            try
            {
                var pagedDob = await client.GetFromJsonAsync<PaginationResponse<DobavljacViewItem>>("api/dobavljaci");
                model.Dobavljaci = pagedDob?.Items ?? new List<DobavljacViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA DOBAVLJACI: {ex.Message}");

                model.Dobavljaci =
                    new List<DobavljacViewItem>();
            }

            // =============================================
            // ODELJENJA
            // =============================================

            try
            {
                model.Odeljenja =
                    await client.GetFromJsonAsync<
                        List<OdeljenjeViewItem>
                    >("api/odeljenja")
                    ?? new List<OdeljenjeViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA ODELJENJA: {ex.Message}");

                model.Odeljenja =
                    new List<OdeljenjeViewItem>();
            }

            // =============================================
            // KATEGORIJE
            // =============================================

            try
            {
                model.Kategorije =
                    await client.GetFromJsonAsync<
                        List<KategorijaViewItem>
                    >("api/kategorije")
                    ?? new List<KategorijaViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA KATEGORIJE: {ex.Message}");

                model.Kategorije =
                    new List<KategorijaViewItem>();
            }

            // =============================================
            // TIPOVI PRODAJE
            // =============================================

            try
            {
                model.TipoviProdaje =
                    await client.GetFromJsonAsync<
                        List<TipProdajeViewItem>
                    >("api/tipprodaje")
                    ?? new List<TipProdajeViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA TIPOVI PRODAJE: {ex.Message}");

                model.TipoviProdaje =
                    new List<TipProdajeViewItem>();
            }

            // =============================================
            // TOP 5 KRITICNIH PROIZVODA
            // =============================================

            try
            {
                var topParts = new List<string>
{
    $"datumOd={datumStart:yyyy-MM-dd}",
    $"datumDo={datumEnd:yyyy-MM-dd}"
};

                if (odeljenjeId.HasValue)
                {
                    topParts.Add(
                        $"odeljenjeId={odeljenjeId.Value}"
                    );
                }

                if (kategorijaId.HasValue)
                {
                    topParts.Add(
                        $"kategorijaId={kategorijaId.Value}"
                    );
                }

                if (dobavljacId.HasValue)
                {
                    topParts.Add(
                        $"dobavljacId={dobavljacId.Value}"
                    );
                }

                if (tipProdajeId.HasValue)
                {
                    topParts.Add(
                        $"tipProdajeId={tipProdajeId.Value}"
                    );
                }

                var topQuery =
                    "?" + string.Join("&", topParts);

                model.CriticalTop5 =
                    await client.GetFromJsonAsync<
                        List<CriticalProductViewItem>
                    >(
                        $"api/artikli/criticalProductsTop{topQuery}"
                    )
                    ?? new List<CriticalProductViewItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA TOP 5: {ex.Message}");

                model.CriticalTop5 =
                    new List<CriticalProductViewItem>();
            }

            // =============================================
            // RUC CHANGE TRACKER
            // =============================================

            try
            {
                int brojDana =
                    datumEnd.DayNumber
                    - datumStart.DayNumber
                    + 1;

                DateOnly prethodniDatumDo =
                    datumStart.AddDays(-1);

                DateOnly prethodniDatumOd =
                    prethodniDatumDo.AddDays(
                        -(brojDana - 1)
                    );

                var rucParts = new List<string>
                {
                    $"datumOd={datumStart:yyyy-MM-dd}",
                    $"datumDo={datumEnd:yyyy-MM-dd}",
                    $"prethodniDatumOd={prethodniDatumOd:yyyy-MM-dd}",
                    $"prethodniDatumDo={prethodniDatumDo:yyyy-MM-dd}"
                };

                if (odeljenjeId.HasValue)
                {
                    rucParts.Add(
                        $"odeljenjeId={odeljenjeId.Value}"
                    );
                }

                if (kategorijaId.HasValue)
                {
                    rucParts.Add(
                        $"kategorijaId={kategorijaId.Value}"
                    );
                }

                if (dobavljacId.HasValue)
                {
                    rucParts.Add(
                        $"dobavljacId={dobavljacId.Value}"
                    );
                }

                if (tipProdajeId.HasValue)
                {
                    rucParts.Add(
                        $"tipProdajeId={tipProdajeId.Value}"
                    );
                }

                var rucQuery =
                    "?" + string.Join("&", rucParts);

                model.RucChange =
                    await client.GetFromJsonAsync<
                        RucChangeViewItem
                    >(
                        $"api/RucChangeTracker{rucQuery}"
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GRESKA RUC TRACKER: {ex.Message}");

                model.RucChange = null;
            }

            return View(model);
        }
    }
}