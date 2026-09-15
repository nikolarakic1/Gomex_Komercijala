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

            if (!imaDatumOd && !imaDatumDo)
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
                - datumOd.DayNumber
                + 1;

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

            var where =
                new StringBuilder();

            where.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @PrethodniDatumOd
                    AND kr.DatumRezultata <
                        DATEADD(DAY, 1, @DatumDo)

                    AND kr.TipProdajeId IN (6, 7)

                    AND a.Aktivan = 1
                """
            );

            if (filterDTO.OdeljenjeId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.OdeljenjeId = @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filterDTO.OdeljenjeId.Value
                );
            }

            if (filterDTO.KategorijaId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.KategorijaId = @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filterDTO.KategorijaId.Value
                );
            }

            if (filterDTO.DobavljacId.HasValue)
            {
                where.AppendLine(
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

                where.AppendLine(
                    """
                    AND k.KategorijaId IN @KategorijaIds
                    """
                );

                parametri.Add(
                    "KategorijaIds",
                    kategorijaIds
                );
            }

            string sql =
                $"""
                SELECT
                    kr.ArtikalId,
                    kr.TipProdajeId,
                    kr.DatumRezultata,
                    kr.DatumUnosa,
                    COALESCE(kr.MPBezPDV, 0) AS MPBezPDV,
                    COALESCE(kr.RUC12, 0) AS RUC12

                INTO #DashboardData

                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = kr.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                INNER JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId

                {where}

                OPTION (MAXDOP 1);

                WITH Totali AS
                (
                    SELECT
                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 6
                                        AND DatumRezultata >= @DatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                    THEN MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS ActualPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 6
                                        AND DatumRezultata >= @DatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                    THEN RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS ActualRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 6
                                        AND DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                    THEN MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniActualPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 6
                                        AND DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                    THEN RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniActualRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 7
                                        AND DatumRezultata >= @DatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                    THEN MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PlanPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 7
                                        AND DatumRezultata >= @DatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @DatumDo
                                            )
                                    THEN RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PlanRuc,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 7
                                        AND DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                    THEN MPBezPDV
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniPlanPromet,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TipProdajeId = 7
                                        AND DatumRezultata >=
                                            @PrethodniDatumOd
                                        AND DatumRezultata <
                                            DATEADD(
                                                DAY,
                                                1,
                                                @PrethodniDatumDo
                                            )
                                    THEN RUC12
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniPlanRuc,

                        MAX(
                            CASE
                                WHEN
                                    TipProdajeId = 6
                                    AND DatumRezultata >= @DatumOd
                                    AND DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN DatumUnosa
                                ELSE NULL
                            END
                        ) AS PodaciOsvezeni

                    FROM #DashboardData
                ),

                KriticniPoArtiklu AS
                (
                    SELECT
                        ArtikalId,

                        SUM(
                            CASE
                                WHEN
                                    TipProdajeId = 6
                                    AND DatumRezultata >= @DatumOd
                                    AND DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN RUC12
                                ELSE 0
                            END
                        ) AS TrenutniRuc,

                        SUM(
                            CASE
                                WHEN
                                    TipProdajeId = 6
                                    AND DatumRezultata >= @DatumOd
                                    AND DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )
                                THEN 1
                                ELSE 0
                            END
                        ) AS TrenutniRedovi,

                        SUM(
                            CASE
                                WHEN
                                    TipProdajeId = 6
                                    AND DatumRezultata >=
                                        @PrethodniDatumOd
                                    AND DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )
                                THEN RUC12
                                ELSE 0
                            END
                        ) AS PrethodniRuc,

                        SUM(
                            CASE
                                WHEN
                                    TipProdajeId = 6
                                    AND DatumRezultata >=
                                        @PrethodniDatumOd
                                    AND DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )
                                THEN 1
                                ELSE 0
                            END
                        ) AS PrethodniRedovi

                    FROM #DashboardData

                    WHERE TipProdajeId = 6

                    GROUP BY ArtikalId
                ),

                Kriticni AS
                (
                    SELECT
                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TrenutniRedovi > 0
                                        AND TrenutniRuc <= 0
                                    THEN 1
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS TrenutniKriticni,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        PrethodniRedovi > 0
                                        AND PrethodniRuc <= 0
                                    THEN 1
                                    ELSE 0
                                END
                            ),
                            0
                        ) AS PrethodniKriticni

                    FROM KriticniPoArtiklu
                ),

                Procenti AS
                (
                    SELECT
                        t.*,

                        CASE
                            WHEN t.ActualPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    t.ActualRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    t.ActualPromet,
                                    0
                                )
                        END AS ActualRucProcenat,

                        CASE
                            WHEN t.PrethodniActualPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    t.PrethodniActualRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    t.PrethodniActualPromet,
                                    0
                                )
                        END AS PrethodniActualRucProcenat,

                        CASE
                            WHEN t.PlanPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    t.PlanRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    t.PlanPromet,
                                    0
                                )
                        END AS PlanRucProcenat,

                        CASE
                            WHEN t.PrethodniPlanPromet = 0
                                THEN 0
                            ELSE
                                CAST(
                                    t.PrethodniPlanRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    t.PrethodniPlanPromet,
                                    0
                                )
                        END AS PrethodniPlanRucProcenat

                    FROM Totali t
                ),

                Efekti AS
                (
                    SELECT
                        p.*,

                        CAST(
                            (
                                p.ActualRucProcenat
                                -
                                p.PlanRucProcenat
                            )
                            *
                            p.PlanPromet
                            AS DECIMAL(28, 6)
                        ) AS TrenutnaMarza,

                        CAST(
                            (
                                p.PrethodniActualRucProcenat
                                -
                                p.PrethodniPlanRucProcenat
                            )
                            *
                            p.PrethodniPlanPromet
                            AS DECIMAL(28, 6)
                        ) AS PrethodnaMarza

                    FROM Procenti p
                )

                SELECT
                    e.ActualPromet
                        AS PrometBezPdv,

                    CAST(
                        CASE
                            WHEN e.PrethodniActualPromet = 0
                                THEN 0
                            ELSE
                                (
                                    e.ActualPromet
                                    -
                                    e.PrethodniActualPromet
                                )
                                /
                                NULLIF(
                                    e.PrethodniActualPromet,
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS PrometPromenaProcenat,

                    e.ActualRuc
                        AS Ruc12,

                    CAST(
                        CASE
                            WHEN e.PrethodniActualRuc = 0
                                THEN 0
                            ELSE
                                (
                                    e.ActualRuc
                                    -
                                    e.PrethodniActualRuc
                                )
                                /
                                NULLIF(
                                    e.PrethodniActualRuc,
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS Ruc12PromenaProcenat,

                    CAST(
                        e.ActualRucProcenat
                        AS DECIMAL(18, 4)
                    ) AS Ruc12Procenat,

                    CAST(
                        e.ActualRucProcenat
                        -
                        e.PrethodniActualRucProcenat
                        AS DECIMAL(18, 4)
                    ) AS Ruc12PromenaProcentniPoeni,

                    k.TrenutniKriticni
                        AS KriticniArtikli,

                    k.TrenutniKriticni
                    -
                    k.PrethodniKriticni
                        AS KriticniArtikliPromena,

                    e.TrenutnaMarza
                        AS NedostatakMarze,

                    CAST(
                        CASE
                            WHEN e.PrethodnaMarza = 0
                                THEN 0
                            ELSE
                                (
                                    e.TrenutnaMarza
                                    -
                                    e.PrethodnaMarza
                                )
                                /
                                NULLIF(
                                    ABS(
                                        e.PrethodnaMarza
                                    ),
                                    0
                                )
                        END
                        AS DECIMAL(18, 4)
                    ) AS NedostatakMarzePromenaProcenat,

                    e.PodaciOsvezeni

                FROM Efekti e

                CROSS JOIN Kriticni k

                OPTION (MAXDOP 1);
                """;

            using var connection =
                _connFactory.CreateConnection();

            var sw =
                Stopwatch.StartNew();

            var rezultat =
                await connection
                    .QuerySingleAsync<DashboardSummaryDTO>(
                        sql,
                        parametri,
                        commandTimeout: 30
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