using GomexPraksaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GomexPraksaMVC.Controllers
{
    [TypeFilter(
        typeof(
            GomexPraksaMVC.Filters.RequireAuthFilter
        )
    )]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public DashboardController(
            IHttpClientFactory httpFactory)
        {
            _httpFactory =
                httpFactory;
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
                _httpFactory
                    .CreateClient(
                        "GomexApi"
                    );

            bool imaParametre =
                datumOd.HasValue
                ||
                datumDo.HasValue
                ||
                odeljenjeId.HasValue
                ||
                kategorijaId.HasValue
                ||
                dobavljacId.HasValue
                ||
                tipProdajeId.HasValue;

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

            DateOnly datumStart;

            DateOnly datumEnd;

            if (!datumOd.HasValue
                &&
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
                datumOd.HasValue
                &&
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
                !datumOd.HasValue
                &&
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

            SacuvajTempData(
                "DashboardDatumOd",
                datumStart.ToString(
                    "yyyy-MM-dd"
                )
            );

            SacuvajTempData(
                "DashboardDatumDo",
                datumEnd.ToString(
                    "yyyy-MM-dd"
                )
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
                        tipProdajeId,

                    CriticalTop5 =
                        new List<
                            CriticalProductViewItem>()
                };

            var analyticsQueryParts =
                new List<string>
                {
                    $"datumOd=" +
                    $"{datumStart:yyyy-MM-dd}",

                    $"datumDo=" +
                    $"{datumEnd:yyyy-MM-dd}"
                };

            DodajOpcioniFilter(
                analyticsQueryParts,
                "odeljenjeId",
                odeljenjeId
            );

            DodajOpcioniFilter(
                analyticsQueryParts,
                "kategorijaId",
                kategorijaId
            );

            DodajOpcioniFilter(
                analyticsQueryParts,
                "dobavljacId",
                dobavljacId
            );

            string analyticsQuery =
                "?"
                +
                string.Join(
                    "&",
                    analyticsQueryParts
                );

            Console.WriteLine(
                $"DASHBOARD FILTER: " +
                $"{datumStart:yyyy-MM-dd} -> " +
                $"{datumEnd:yyyy-MM-dd}"
            );

            var dobavljaciTask =
                SafeGetAsync<
                    PaginationResponse<
                        DobavljacViewItem
                    >
                >(
                    client,
                    "api/dobavljaci",
                    "DOBAVLJACI"
                );

            var odeljenjaTask =
                SafeGetAsync<
                    List<
                        OdeljenjeViewItem
                    >
                >(
                    client,
                    "api/odeljenja",
                    "ODELJENJA"
                );

            var kategorijeTask =
                SafeGetAsync<
                    List<
                        KategorijaViewItem
                    >
                >(
                    client,
                    "api/kategorije",
                    "KATEGORIJE"
                );

            var tipoviProdajeTask =
                SafeGetAsync<
                    List<
                        TipProdajeViewItem
                    >
                >(
                    client,
                    "api/tipprodaje",
                    "TIPOVI PRODAJE"
                );

            await Task.WhenAll(
                dobavljaciTask,
                odeljenjaTask,
                kategorijeTask,
                tipoviProdajeTask
            );

            var dobavljaci =
                await dobavljaciTask;

            var odeljenja =
                await odeljenjaTask;

            var kategorije =
                await kategorijeTask;

            var tipoviProdaje =
                await tipoviProdajeTask;

            Console.WriteLine(
                "START DASHBOARD SUMMARY"
            );

            var summary =
                await SafeGetAsync<
                    DashboardViewModel
                >(
                    client,
                    $"api/dashboard/summary" +
                    $"{analyticsQuery}",
                    "DASHBOARD SUMMARY"
                );

            Console.WriteLine(
                "START TOP 5"
            );

            var criticalTop =
                await SafeGetAsync<
                    List<
                        CriticalProductViewItem
                    >
                >(
                    client,
                    $"api/artikli/" +
                    $"criticalProductsTop" +
                    $"{analyticsQuery}",
                    "TOP 5 KRITIČNIH PROIZVODA"
                );

            Console.WriteLine(
                "START RUC CHANGE"
            );

            var rucChange =
                await SafeGetAsync<
                    RucChangeViewItem
                >(
                    client,
                    $"api/RucChangeTracker" +
                    $"{analyticsQuery}",
                    "RUC CHANGE TRACKER"
                );

            if (summary != null)
            {
                model.PrometBezPdv =
                    summary.PrometBezPdv;

                model.PrometPromenaProcenat =
                    summary
                        .PrometPromenaProcenat;

                model.Ruc12 =
                    summary.Ruc12;

                model.Ruc12PromenaProcenat =
                    summary
                        .Ruc12PromenaProcenat;

                model.Ruc12Procenat =
                    summary.Ruc12Procenat;

                model
                    .Ruc12PromenaProcentniPoeni =
                    summary
                        .Ruc12PromenaProcentniPoeni;

                model.KriticniArtikli =
                    summary.KriticniArtikli;

                model.KriticniArtikliPromena =
                    summary
                        .KriticniArtikliPromena;

                model.NedostatakMarze =
                    summary.NedostatakMarze;

                model
                    .NedostatakMarzePromenaProcenat =
                    summary
                        .NedostatakMarzePromenaProcenat;

                model.PodaciOsvezeni =
                    summary.PodaciOsvezeni;
            }

            model.Dobavljaci =
                dobavljaci?.Items
                ??
                new List<
                    DobavljacViewItem
                >();

            model.Odeljenja =
                odeljenja
                ??
                new List<
                    OdeljenjeViewItem
                >();

            model.Kategorije =
                kategorije
                ??
                new List<
                    KategorijaViewItem
                >();

            model.TipoviProdaje =
                tipoviProdaje
                ??
                new List<
                    TipProdajeViewItem
                >();

            model.CriticalTop5 =
                criticalTop
                ??
                new List<
                    CriticalProductViewItem
                >();

            model.RucChange =
                rucChange;

            return View(
                model
            );
        }

        private DateTime?
            ProcitajDatumIzTempData(
                string key)
        {
            var vrednost =
                TempData
                    .Peek(key)?
                    .ToString();

            if (string.IsNullOrWhiteSpace(
                vrednost
            ))
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

        private int?
            ProcitajIntIzTempData(
                string key)
        {
            var vrednost =
                TempData
                    .Peek(key)?
                    .ToString();

            if (string.IsNullOrWhiteSpace(
                vrednost
            ))
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

        private void SacuvajTempData(
            string key,
            object? vrednost)
        {
            if (vrednost == null)
            {
                TempData.Remove(
                    key
                );

                return;
            }

            TempData[key] =
                vrednost.ToString();

            TempData.Keep(
                key
            );
        }

        private static void
            DodajOpcioniFilter(
                ICollection<string>
                    queryParts,
                string naziv,
                int? vrednost)
        {
            if (!vrednost.HasValue)
            {
                return;
            }

            queryParts.Add(
                $"{naziv}=" +
                $"{vrednost.Value}"
            );
        }

        private static async Task<T?>
            SafeGetAsync<T>(
                HttpClient client,
                string url,
                string nazivPoziva)
            where T : class
        {
            var sw =
                System.Diagnostics
                    .Stopwatch
                    .StartNew();

            try
            {
                Console.WriteLine(
                    $"START {nazivPoziva}: " +
                    $"{url}"
                );

                var result =
                    await client
                        .GetFromJsonAsync<T>(
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