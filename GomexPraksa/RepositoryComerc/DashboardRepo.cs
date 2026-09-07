using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.DtosComerc;
using System.Diagnostics;
using System.Text;

namespace GomexPraksa.RepositoryComerc
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly IConnFactory _connFactory;

        public DashboardRepo(
            IConnFactory connFactory)
        {
            _connFactory = connFactory;
        }

        public async Task<DashboardSummaryDTO> FillCardsAsync(
            DashboardFilterDTO filterDTO,
            bool canViewAllCategories,
            List<int> kategorijaIds)
        {
            ArgumentNullException.ThrowIfNull(filterDTO);

            bool imaDatumOd =
                filterDTO.DatumOd.HasValue;

            bool imaDatumDo =
                filterDTO.DatumDo.HasValue;

            if (imaDatumOd != imaDatumDo)
            {
                throw new ArgumentException(
                    "Moraju biti uneti i DatumOd i DatumDo, ili nijedan."
                );
            }

            DateOnly datumOd;
            DateOnly datumDo;

            if (!imaDatumOd &&
                !imaDatumDo)
            {
                datumDo =
                    DateOnly.FromDateTime(
                        DateTime.Today
                    );

                datumOd =
                    datumDo.AddDays(-29);
            }
            else
            {
                datumOd =
                    filterDTO.DatumOd!.Value;

                datumDo =
                    filterDTO.DatumDo!.Value;
            }

            if (datumOd > datumDo)
            {
                throw new ArgumentException(
                    "DatumOd ne može biti posle DatumDo."
                );
            }

            int brojDana =
                datumDo.DayNumber
                -
                datumOd.DayNumber
                +
                1;

            DateOnly prethodniDatumDo =
                datumOd.AddDays(-1);

            DateOnly prethodniDatumOd =
                prethodniDatumDo.AddDays(
                    -(brojDana - 1)
                );

            var parametri =
                new DynamicParameters();

            parametri.Add(
                "DatumOd",
                datumOd.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            parametri.Add(
                "DatumDo",
                datumDo.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            parametri.Add(
                "PrethodniDatumOd",
                prethodniDatumOd.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            parametri.Add(
                "PrethodniDatumDo",
                prethodniDatumDo.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            var baseWhere =
                new StringBuilder();

            baseWhere.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @PrethodniDatumOd
                    AND kr.DatumRezultata <
                        DATEADD(
                            DAY,
                            1,
                            @DatumDo
                        )
                    AND a.Aktivan = 1
                """
            );

            if (filterDTO.OdeljenjeId.HasValue)
            {
                baseWhere.AppendLine(
                    """
                    AND k.OdeljenjeId =
                        @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filterDTO.OdeljenjeId.Value
                );
            }

            if (filterDTO.KategorijaId.HasValue)
            {
                baseWhere.AppendLine(
                    """
                    AND k.KategorijaId =
                        @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filterDTO.KategorijaId.Value
                );
            }

            if (filterDTO.DobavljacId.HasValue)
            {
                baseWhere.AppendLine(
                    """
                    AND COALESCE(
                        kr.DobavljacId,
                        a.DobavljacId
                    ) = @DobavljacId
                    """
                );

                parametri.Add(
                    "DobavljacId",
                    filterDTO.DobavljacId.Value
                );
            }

            if (!canViewAllCategories)
            {
                if (kategorijaIds is null ||
                    kategorijaIds.Count == 0)
                {
                    throw new UnauthorizedAccessException(
                        "Korisniku nije dodeljena nijedna kategorija."
                    );
                }

                baseWhere.AppendLine(
                    """
                    AND k.KategorijaId
                        IN @KategorijaIds
                    """
                );

                parametri.Add(
                    "KategorijaIds",
                    kategorijaIds
                );
            }

            var dashboardWhere =
                new StringBuilder(
                    baseWhere.ToString()
                );

            if (filterDTO.TipProdajeId.HasValue)
            {
                dashboardWhere.AppendLine(
                    """
                    AND kr.TipProdajeId =
                        @TipProdajeId
                    """
                );

                parametri.Add(
                    "TipProdajeId",
                    filterDTO.TipProdajeId.Value
                );
            }

            string sql =
                $"""
                WITH PoArtiklu AS
                (
                    SELECT
                        kr.ArtikalId,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN
                                    kr.MPBezPDV
                                ELSE 0
                            END
                        ) AS TrenutniPromet,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN
                                    kr.RUC12
                                ELSE 0
                            END
                        ) AS TrenutniRuc,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN 1
                                ELSE 0
                            END
                        ) AS TrenutniBrojRedova,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )
                                THEN
                                    kr.MPBezPDV
                                ELSE 0
                            END
                        ) AS PrethodniPromet,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )
                                THEN
                                    kr.RUC12
                                ELSE 0
                            END
                        ) AS PrethodniRuc,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )
                                THEN 1
                                ELSE 0
                            END
                        ) AS PrethodniBrojRedova,

                        MAX(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd
                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN
                                    kr.DatumUnosa
                                ELSE NULL
                            END
                        ) AS PodaciOsvezeni

                    FROM
                        dbo.KomercijalniRezultat kr

                    INNER JOIN
                        dbo.Artikal a
                            ON a.ArtikalId =
                               kr.ArtikalId

                    LEFT JOIN
                        dbo.RobnaGrupa rg
                            ON rg.RobnaGrupaId =
                               a.RobnaGrupaId

                    LEFT JOIN
                        dbo.Kategorija k
                            ON k.KategorijaId =
                               rg.KategorijaId

                    {dashboardWhere}

                    GROUP BY
                        kr.ArtikalId
                ),

                Ukupno AS
                (
                    SELECT
                        COALESCE(
                            SUM(
                                TrenutniPromet
                            ),
                            0
                        ) AS TrenutniPromet,

                        COALESCE(
                            SUM(
                                TrenutniRuc
                            ),
                            0
                        ) AS TrenutniRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TrenutniBrojRedova > 0
                                        AND TrenutniRuc <= 0
                                    THEN 1
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS TrenutniKriticni,

                        COALESCE(
                            SUM(
                                PrethodniPromet
                            ),
                            0
                        ) AS PrethodniPromet,

                        COALESCE(
                            SUM(
                                PrethodniRuc
                            ),
                            0
                        ) AS PrethodniRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        PrethodniBrojRedova > 0
                                        AND PrethodniRuc <= 0
                                    THEN 1
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniKriticni,

                        MAX(
                            PodaciOsvezeni
                        ) AS PodaciOsvezeni

                    FROM
                        PoArtiklu
                ),

                MarzaPodaci AS
                (
                    SELECT
                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >= @DatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                        AND kr.TipProdajeId = 6
                                    THEN
                                        kr.MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS ActualPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >= @DatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                        AND kr.TipProdajeId = 7
                                    THEN
                                        kr.MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PlanPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >= @DatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                        AND kr.TipProdajeId = 6
                                    THEN
                                        kr.RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS ActualRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >= @DatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                        AND kr.TipProdajeId = 7
                                    THEN
                                        kr.RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PlanRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                        AND kr.TipProdajeId = 6
                                    THEN
                                        kr.MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniActualPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                        AND kr.TipProdajeId = 7
                                    THEN
                                        kr.MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniPlanPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                        AND kr.TipProdajeId = 6
                                    THEN
                                        kr.RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniActualRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        kr.DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND kr.DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                        AND kr.TipProdajeId = 7
                                    THEN
                                        kr.RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniPlanRuc

                    FROM
                        dbo.KomercijalniRezultat kr

                    INNER JOIN
                        dbo.Artikal a
                            ON a.ArtikalId =
                               kr.ArtikalId

                    LEFT JOIN
                        dbo.RobnaGrupa rg
                            ON rg.RobnaGrupaId =
                               a.RobnaGrupaId

                    LEFT JOIN
                        dbo.Kategorija k
                            ON k.KategorijaId =
                               rg.KategorijaId

                    {baseWhere}

                    AND kr.TipProdajeId IN (6, 7)
                ),

                MarzaProcenti AS
                (
                    SELECT
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        PrethodniActualPromet,
                        PrethodniPlanPromet,
                        PrethodniActualRuc,
                        PrethodniPlanRuc,

                        CASE
                            WHEN ActualPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    ActualRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    ActualPromet,
                                    0
                                )
                        END AS ActualRucProcenat,

                        CASE
                            WHEN PlanPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    PlanRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    PlanPromet,
                                    0
                                )
                        END AS PlanRucProcenat,

                        CASE
                            WHEN PrethodniActualPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    PrethodniActualRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    PrethodniActualPromet,
                                    0
                                )
                        END AS PrethodniActualRucProcenat,

                        CASE
                            WHEN PrethodniPlanPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    PrethodniPlanRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    PrethodniPlanPromet,
                                    0
                                )
                        END AS PrethodniPlanRucProcenat

                    FROM
                        MarzaPodaci
                ),

                MarzaEfekat AS
                (
                    SELECT
                        CAST(
                            (
                                ActualRucProcenat
                                -
                                PlanRucProcenat
                            )
                            *
                            PlanPromet
                            AS DECIMAL(28, 6)
                        ) AS TrenutnoOdstupanjeMarze,

                        CAST(
                            (
                                PrethodniActualRucProcenat
                                -
                                PrethodniPlanRucProcenat
                            )
                            *
                            PrethodniPlanPromet
                            AS DECIMAL(28, 6)
                        ) AS PrethodnoOdstupanjeMarze

                    FROM
                        MarzaProcenti
                )

                SELECT
                    u.TrenutniPromet
                        AS PrometBezPdv,

                    CAST(
                        CASE
                            WHEN u.PrethodniPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    u.TrenutniPromet
                                    -
                                    u.PrethodniPromet
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    u.PrethodniPromet,
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS PrometPromenaProcenat,

                    u.TrenutniRuc
                        AS Ruc12,

                    CAST(
                        CASE
                            WHEN u.PrethodniRuc = 0
                                THEN 0
                            ELSE
                                CAST(
                                    u.TrenutniRuc
                                    -
                                    u.PrethodniRuc
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    u.PrethodniRuc,
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS Ruc12PromenaProcenat,

                    CAST(
                        CASE
                            WHEN u.TrenutniPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    u.TrenutniRuc
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    u.TrenutniPromet,
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS Ruc12Procenat,

                    CAST(
                        (
                            CASE
                                WHEN u.TrenutniPromet = 0
                                    THEN 0
                                ELSE
                                    CAST(
                                        u.TrenutniRuc
                                        AS DECIMAL(28, 6)
                                    )
                                    /
                                    NULLIF(
                                        u.TrenutniPromet,
                                        0
                                    )
                            END
                        )
                        -
                        (
                            CASE
                                WHEN u.PrethodniPromet = 0
                                    THEN 0
                                ELSE
                                    CAST(
                                        u.PrethodniRuc
                                        AS DECIMAL(28, 6)
                                    )
                                    /
                                    NULLIF(
                                        u.PrethodniPromet,
                                        0
                                    )
                            END
                        )
                        AS DECIMAL(18, 4)
                    ) AS Ruc12PromenaProcentniPoeni,

                    u.TrenutniKriticni
                        AS KriticniArtikli,

                    u.TrenutniKriticni
                    -
                    u.PrethodniKriticni
                        AS KriticniArtikliPromena,

                    m.TrenutnoOdstupanjeMarze
                        AS NedostatakMarze,

                    CAST(
                        CASE
                            WHEN
                                m.PrethodnoOdstupanjeMarze = 0
                            THEN 0
                            ELSE
                                (
                                    m.TrenutnoOdstupanjeMarze
                                    -
                                    m.PrethodnoOdstupanjeMarze
                                )
                                /
                                NULLIF(
                                    ABS(
                                        m.PrethodnoOdstupanjeMarze
                                    ),
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS NedostatakMarzePromenaProcenat,

                    u.PodaciOsvezeni

                FROM
                    Ukupno u

                CROSS JOIN
                    MarzaEfekat m

                OPTION (RECOMPILE);
                """;

            using var connection =
                _connFactory.CreateConnection();

            var sw =
                Stopwatch.StartNew();

            var rezultat =
                await connection
                    .QuerySingleAsync<DashboardSummaryDTO>(
                        sql,
                        parametri
                    );

            sw.Stop();

            Console.WriteLine(
                $"DASHBOARD SQL: " +
                $"{sw.ElapsedMilliseconds} ms"
            );

            return rezultat;
        }
    }
}