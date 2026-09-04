using GomexPraksaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GomexPraksaMVC.Controllers
{
    [TypeFilter(typeof(GomexPraksaMVC.Filters.RequireAuthFilter))]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public DashboardController(
            IHttpClientFactory httpFactory)
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
            var client =
                _httpFactory.CreateClient("GomexApi");

            // =============================================
            // DA LI SU FILTERI POSLATI IZ FORME / URL-A?
            // =============================================

            bool imaParametre =
                datumOd.HasValue ||
                datumDo.HasValue ||
                odeljenjeId.HasValue ||
                kategorijaId.HasValue ||
                dobavljacId.HasValue ||
                tipProdajeId.HasValue;

            // =============================================
            // AKO NEMA PARAMETARA,
            // POKUSAJ DA VRATIS POSLEDNJE FILTERE
            // =============================================

            if (!imaParametre)
            {
                datumOd =
                    ProcitajDatumIzTempData(
                        "DashboardDatumOd"
                    );

                datumDo =
                    ProcitajDatumIzTempData(
                        "DashboardDatumDo"
                    );

                odeljenjeId =
                    ProcitajIntIzTempData(
                        "DashboardOdeljenjeId"
                    );

                kategorijaId =
                    ProcitajIntIzTempData(
                        "DashboardKategorijaId"
                    );

                dobavljacId =
                    ProcitajIntIzTempData(
                        "DashboardDobavljacId"
                    );

                tipProdajeId =
                    ProcitajIntIzTempData(
                        "DashboardTipProdajeId"
                    );
            }

            // =============================================
            // DATUMI
            // =============================================

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

            // =============================================
            // PROVERA DATUMA
            // =============================================

            if (datumStart > datumEnd)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Datum od ne može biti posle datuma do."
                );

                datumEnd =
                    DateOnly.FromDateTime(
                        DateTime.Today
                    );

                datumStart =
                    datumEnd.AddDays(-29);
            }

            // =============================================
            // SACUVAJ TRENUTNO STANJE FILTERA
            // =============================================

            SacuvajTempData(
                "DashboardDatumOd",
                datumStart.ToString("yyyy-MM-dd")
            );

            SacuvajTempData(
                "DashboardDatumDo",
                datumEnd.ToString("yyyy-MM-dd")
            );

            SacuvajTempData(
                "DashboardOdeljenjeId",
                odeljenjeId
            );

            SacuvajTempData(
                "DashboardKategorijaId",
                kategorijaId
            );

            SacuvajTempData(
                "DashboardDobavljacId",
                dobavljacId
            );

            SacuvajTempData(
                "DashboardTipProdajeId",
                tipProdajeId
            );

            // =============================================
            // MODEL
            // =============================================

            var model =
                new DashboardViewModel
                {
                    DatumOd =
                        datumStart,

                    DatumDo =
                        datumEnd,

                    OdeljenjeId =
                        odeljenjeId,

                    KategorijaId =
                        kategorijaId,

                    SelectedDobavljacId =
                        dobavljacId,

                    TipProdajeId =
                        tipProdajeId
                };

            // =============================================
            // QUERY ZA DASHBOARD
            // =============================================

            var dashboardQueryParts =
                new List<string>
                {
                    $"datumOd={datumStart:yyyy-MM-dd}",
                    $"datumDo={datumEnd:yyyy-MM-dd}"
                };

            DodajOpcioniFilter(
                dashboardQueryParts,
                "odeljenjeId",
                odeljenjeId
            );

            DodajOpcioniFilter(
                dashboardQueryParts,
                "kategorijaId",
                kategorijaId
            );

            DodajOpcioniFilter(
                dashboardQueryParts,
                "dobavljacId",
                dobavljacId
            );

            DodajOpcioniFilter(
                dashboardQueryParts,
                "tipProdajeId",
                tipProdajeId
            );

            string dashboardQuery =
                "?" +
                string.Join(
                    "&",
                    dashboardQueryParts
                );

            Console.WriteLine(
                $"DASHBOARD FILTER: " +
                $"{datumStart:yyyy-MM-dd} -> " +
                $"{datumEnd:yyyy-MM-dd}"
            );

            // =============================================
            // PARALELNO POKRETANJE API POZIVA
            // =============================================

            var summaryTask =
                SafeGetAsync<DashboardViewModel>(
                    client,
                    $"api/dashboard/summary{dashboardQuery}",
                    "DASHBOARD SUMMARY"
                );

            var dobavljaciTask =
                SafeGetAsync<
                    PaginationResponse<DobavljacViewItem>
                >(
                    client,
                    "api/dobavljaci",
                    "DOBAVLJACI"
                );

            var odeljenjaTask =
                SafeGetAsync<
                    List<OdeljenjeViewItem>
                >(
                    client,
                    "api/odeljenja",
                    "ODELJENJA"
                );

            var kategorijeTask =
                SafeGetAsync<
                    List<KategorijaViewItem>
                >(
                    client,
                    "api/kategorije",
                    "KATEGORIJE"
                );

            var tipoviProdajeTask =
                SafeGetAsync<
                    List<TipProdajeViewItem>
                >(
                    client,
                    "api/tipprodaje",
                    "TIPOVI PRODAJE"
                );

            var criticalTopTask =
                SafeGetAsync<
                    List<CriticalProductViewItem>
                >(
                    client,
                    $"api/artikli/criticalProductsTop{dashboardQuery}",
                    "TOP 5 KRITIČNIH PROIZVODA"
                );

            var rucChangeTask =
                SafeGetAsync<RucChangeViewItem>(
                    client,
                    $"api/RucChangeTracker{dashboardQuery}",
                    "RUC CHANGE TRACKER"
                );

            // =============================================
            // CEKAMO SVE
            // =============================================

            await Task.WhenAll(
                summaryTask,
                dobavljaciTask,
                odeljenjaTask,
                kategorijeTask,
                tipoviProdajeTask,
                criticalTopTask,
                rucChangeTask
            );

            // =============================================
            // REZULTATI
            // =============================================

            var summary =
                await summaryTask;

            var dobavljaci =
                await dobavljaciTask;

            var odeljenja =
                await odeljenjaTask;

            var kategorije =
                await kategorijeTask;

            var tipoviProdaje =
                await tipoviProdajeTask;

            var criticalTop =
                await criticalTopTask;

            var rucChange =
                await rucChangeTask;

            // =============================================
            // DASHBOARD SUMMARY
            // =============================================

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

            // =============================================
            // LOOKUP PODACI
            // =============================================

            model.Dobavljaci =
                dobavljaci?.Items
                ?? new List<DobavljacViewItem>();

            model.Odeljenja =
                odeljenja
                ?? new List<OdeljenjeViewItem>();

            model.Kategorije =
                kategorije
                ?? new List<KategorijaViewItem>();

            model.TipoviProdaje =
                tipoviProdaje
                ?? new List<TipProdajeViewItem>();

            // =============================================
            // TOP 5
            // =============================================

            model.CriticalTop5 =
                criticalTop
                ?? new List<CriticalProductViewItem>();

            // =============================================
            // RUC CHANGE
            // =============================================

            model.RucChange =
                rucChange;

            return View(model);
        }

        // =============================================
        // TEMP DATA - DATUM
        // =============================================

        private DateTime? ProcitajDatumIzTempData(
            string key)
        {
            var vrednost =
                TempData.Peek(key)?.ToString();

            if (string.IsNullOrWhiteSpace(vrednost))
            {
                return null;
            }

            if (DateTime.TryParse(
                vrednost,
                out var datum))
            {
                return datum;
            }

            return null;
        }

        // =============================================
        // TEMP DATA - INT FILTER
        // =============================================

        private int? ProcitajIntIzTempData(
            string key)
        {
            var vrednost =
                TempData.Peek(key)?.ToString();

            if (string.IsNullOrWhiteSpace(vrednost))
            {
                return null;
            }

            if (int.TryParse(
                vrednost,
                out var broj))
            {
                return broj;
            }

            return null;
        }

        // =============================================
        // TEMP DATA - CUVANJE
        // =============================================

        private void SacuvajTempData(
            string key,
            object? vrednost)
        {
            if (vrednost == null)
            {
                TempData.Remove(key);
                return;
            }

            TempData[key] =
                vrednost.ToString();

            TempData.Keep(key);
        }

        // =============================================
        // OPCIONI FILTER
        // =============================================

        private static void DodajOpcioniFilter(
            ICollection<string> queryParts,
            string naziv,
            int? vrednost)
        {
            if (!vrednost.HasValue)
            {
                return;
            }

            queryParts.Add(
                $"{naziv}={vrednost.Value}"
            );
        }

        // =============================================
        // SAFE API GET
        // =============================================

        private static async Task<T?> SafeGetAsync<T>(
            HttpClient client,
            string url,
            string nazivPoziva)
            where T : class
        {
            var sw =
                System.Diagnostics.Stopwatch.StartNew();

            try
            {
                Console.WriteLine(
                    $"START {nazivPoziva}: {url}"
                );

                var result =
                    await client.GetFromJsonAsync<T>(
                        url
                    );

                sw.Stop();

                Console.WriteLine(
                    $"KRAJ {nazivPoziva}: " +
                    $"{sw.ElapsedMilliseconds} ms"
                );

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();

                Console.WriteLine(
                    $"GRESKA {nazivPoziva}: " +
                    $"{sw.ElapsedMilliseconds} ms | " +
                    $"{ex.Message}"
                );

                return null;
            }
        }
    }
}