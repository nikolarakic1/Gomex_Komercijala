using Dapper;
using GomexPraksa.AddedFunctions;
using GomexPraksa.ConnectionFactory;
using Models.Dtos;
using Models.DtosComerc;
using System.Diagnostics;
using System.Text;

namespace GomexPraksa.RepositoryComerc
{
    public class CriticalProductsRepo : ICriticalProducts
    {
        private readonly IConnFactory _connection;

        private const decimal PragOdstupanja = 0.005m;

        public CriticalProductsRepo(
            IConnFactory connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<CriticalProductsDTO>>
            CriticalProductsTop5(
                DashboardFilterDTO filter,
                DateOnly datumOd,
                DateOnly datumDo,
                bool canViewAllCategories,
                List<int> kategorijaIds)
        {
            if (!canViewAllCategories &&
                (kategorijaIds == null ||
                 kategorijaIds.Count == 0))
            {
                return Enumerable.Empty<CriticalProductsDTO>();
            }

            var requestId =
                Guid.NewGuid()
                    .ToString("N")[..8];

            var totalSw =
                Stopwatch.StartNew();

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
                "PragOdstupanja",
                PragOdstupanja
            );

            var where =
                new StringBuilder();

            where.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @DatumOd
                    AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                    AND kr.TipProdajeId IN (6, 7)
                    AND a.Aktivan = 1
                """
            );

            if (filter.OdeljenjeId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.OdeljenjeId = @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filter.OdeljenjeId.Value
                );
            }

            if (filter.KategorijaId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.KategorijaId = @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filter.KategorijaId.Value
                );
            }

            if (filter.DobavljacId.HasValue)
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
                    filter.DobavljacId.Value
                );
            }

            if (!canViewAllCategories)
            {
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
                WITH Agregirano AS
                (
                    SELECT
                        kr.ArtikalId,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                THEN COALESCE(
                                    kr.MPBezPDV,
                                    0
                                )
                                ELSE 0
                            END
                        ) AS ActualPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                THEN COALESCE(
                                    kr.MPBezPDV,
                                    0
                                )
                                ELSE 0
                            END
                        ) AS PlanPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                THEN COALESCE(
                                    kr.RUC12,
                                    0
                                )
                                ELSE 0
                            END
                        ) AS ActualRuc,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                THEN COALESCE(
                                    kr.RUC12,
                                    0
                                )
                                ELSE 0
                            END
                        ) AS PlanRuc,

                        MAX(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                THEN 1
                                ELSE 0
                            END
                        ) AS ImaActual,

                        MAX(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                THEN 1
                                ELSE 0
                            END
                        ) AS ImaPlan

                    FROM dbo.KomercijalniRezultat kr

                    INNER JOIN dbo.Artikal a
                        ON a.ArtikalId =
                           kr.ArtikalId

                    INNER JOIN dbo.RobnaGrupa rg
                        ON rg.RobnaGrupaId =
                           a.RobnaGrupaId

                    INNER JOIN dbo.Kategorija k
                        ON k.KategorijaId =
                           rg.KategorijaId

                    {where}

                    GROUP BY
                        kr.ArtikalId
                ),

                Procenti AS
                (
                    SELECT
                        ArtikalId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ImaActual,
                        ImaPlan,

                        CASE
                            WHEN ActualPromet = 0
                            THEN CAST(
                                0
                                AS DECIMAL(28, 10)
                            )

                            ELSE
                                CAST(
                                    ActualRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    CAST(
                                        ActualPromet
                                        AS DECIMAL(28, 10)
                                    ),
                                    0
                                )
                        END
                            AS ActualRucProcenat,

                        CASE
                            WHEN PlanPromet = 0
                            THEN CAST(
                                0
                                AS DECIMAL(28, 10)
                            )

                            ELSE
                                CAST(
                                    PlanRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    CAST(
                                        PlanPromet
                                        AS DECIMAL(28, 10)
                                    ),
                                    0
                                )
                        END
                            AS PlanRucProcenat

                    FROM Agregirano
                ),

                Izracunato AS
                (
                    SELECT
                        ArtikalId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ImaActual,
                        ImaPlan,
                        ActualRucProcenat,
                        PlanRucProcenat,

                        ActualRucProcenat
                        -
                        PlanRucProcenat
                            AS OdstupanjeProcentniPoeni,

                        CASE
                            WHEN
                                ImaPlan = 0
                                OR PlanPromet = 0
                            THEN CAST(
                                0
                                AS DECIMAL(28, 6)
                            )

                            ELSE
                                CAST(
                                    (
                                        ActualRucProcenat
                                        -
                                        PlanRucProcenat
                                    )
                                    *
                                    PlanPromet
                                    AS DECIMAL(28, 6)
                                )
                        END
                            AS MarginEffect

                    FROM Procenti
                ),

                Kriticni AS
                (
                    SELECT
                        ArtikalId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ActualRucProcenat,
                        PlanRucProcenat,
                        OdstupanjeProcentniPoeni,
                        MarginEffect

                    FROM Izracunato

                    WHERE
                        (
                            ImaActual = 1
                            AND ActualRuc <= 0
                        )

                        OR

                        (
                            ImaActual = 1
                            AND ImaPlan = 1
                            AND PlanPromet <> 0
                            AND OdstupanjeProcentniPoeni
                                < @PragOdstupanja
                        )
                ),

                TopPet AS
                (
                    SELECT TOP (5)
                        ArtikalId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ActualRucProcenat,
                        PlanRucProcenat,
                        OdstupanjeProcentniPoeni,
                        MarginEffect

                    FROM Kriticni

                    ORDER BY
                        CASE
                            WHEN ActualRuc <= 0
                            THEN 0
                            ELSE 1
                        END ASC,

                        CASE
                            WHEN
                                ActualRuc <= 0
                                AND MarginEffect = 0
                            THEN ActualRuc
                            ELSE MarginEffect
                        END ASC,

                        OdstupanjeProcentniPoeni ASC,

                        ArtikalId ASC
                )

                SELECT
                    t.ArtikalId,

                    a.Naziv
                        AS NazivArtikla,

                    k.Naziv
                        AS Kategorija,

                    CASE
                        WHEN t.ActualRuc <= 0
                        THEN 'Visok'

                        WHEN
                            t.OdstupanjeProcentniPoeni
                            <= -0.020
                        THEN 'Visok'

                        WHEN
                            t.OdstupanjeProcentniPoeni
                            < 0
                        THEN 'Srednji'

                        ELSE 'Nizak'
                    END
                        AS Severnost,

                    CASE
                        WHEN
                            t.ActualRuc <= 0
                            AND t.MarginEffect = 0
                        THEN t.ActualRuc

                        ELSE t.MarginEffect
                    END
                        AS ProcenjeniUticaj

                FROM TopPet t

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId =
                       t.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId =
                       a.RobnaGrupaId

                INNER JOIN dbo.Kategorija k
                    ON k.KategorijaId =
                       rg.KategorijaId

                ORDER BY
                    CASE
                        WHEN t.ActualRuc <= 0
                        THEN 0
                        ELSE 1
                    END ASC,

                    CASE
                        WHEN
                            t.ActualRuc <= 0
                            AND t.MarginEffect = 0
                        THEN t.ActualRuc
                        ELSE t.MarginEffect
                    END ASC,

                    t.OdstupanjeProcentniPoeni ASC,

                    t.ArtikalId ASC;
                """;

            using var connection =
                _connection.CreateConnection();

            var openSw =
                Stopwatch.StartNew();

            connection.Open();

            openSw.Stop();

            var querySw =
                Stopwatch.StartNew();

            var rezultat =
                (
                    await connection
                        .QueryAsync<CriticalProductsDTO>(
                            sql,
                            parametri,
                            commandTimeout: 30
                        )
                )
                .ToList();

            querySw.Stop();
            totalSw.Stop();

            Console.WriteLine(
                $"CRITICAL TOP5 [{requestId}] " +
                $"OPEN: {openSw.ElapsedMilliseconds} ms | " +
                $"QUERY: {querySw.ElapsedMilliseconds} ms | " +
                $"TOTAL: {totalSw.ElapsedMilliseconds} ms | " +
                $"ROWS: {rezultat.Count}"
            );

            return rezultat;
        }

        public async Task<
            PaginationGeneric<CriticalProductsPageDTO>>
            ShowCriticalProductsAsync(
                FilterSharedPages filter,
                PaginationParams pagination,
                bool canViewAllCategories,
                List<int> kategorijaIds)
        {
            if (pagination.Page < 1)
            {
                pagination.Page = 1;
            }

            if (pagination.PageSize < 1)
            {
                pagination.PageSize = 10;
            }

            if (pagination.PageSize > 100)
            {
                pagination.PageSize = 100;
            }

            if (!canViewAllCategories &&
                (kategorijaIds == null ||
                 kategorijaIds.Count == 0))
            {
                return new PaginationGeneric<
                    CriticalProductsPageDTO>
                {
                    Items =
                        Enumerable.Empty<
                            CriticalProductsPageDTO>(),

                    Page =
                        pagination.Page,

                    PageSize =
                        pagination.PageSize,

                    TotalCount =
                        0
                };
            }

            var requestId =
                Guid.NewGuid()
                    .ToString("N")[..8];

            var totalSw =
                Stopwatch.StartNew();

            var parametri =
                new DynamicParameters();

            parametri.Add(
                "DatumOd",
                filter.DatumOd.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            parametri.Add(
                "DatumDo",
                filter.DatumDo.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            parametri.Add(
                "PragOdstupanja",
                PragOdstupanja
            );

            parametri.Add(
                "Offset",
                (pagination.Page - 1)
                *
                pagination.PageSize
            );

            parametri.Add(
                "PageSize",
                pagination.PageSize
            );

            var where =
                new StringBuilder();

            where.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @DatumOd
                    AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                    AND kr.TipProdajeId IN (6, 7)
                    AND a.Aktivan = 1
                """
            );

            if (filter.OdeljenjeId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.OdeljenjeId = @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filter.OdeljenjeId.Value
                );
            }

            if (filter.KategorijaId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.KategorijaId = @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filter.KategorijaId.Value
                );
            }

            if (filter.DobavljacId.HasValue)
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
                    filter.DobavljacId.Value
                );
            }

            if (!canViewAllCategories)
            {
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
                SET NOCOUNT ON;

                IF OBJECT_ID(
                    'tempdb..#Kriticni'
                ) IS NOT NULL
                    DROP TABLE #Kriticni;

                WITH Agregirano AS
                (
                    SELECT
                        kr.ArtikalId,

                        MAX(
                            COALESCE(
                                kr.DobavljacId,
                                a.DobavljacId
                            )
                        )
                            AS DobavljacId,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                THEN COALESCE(
                                    kr.MPBezPDV,
                                    0
                                )
                                ELSE 0
                            END
                        )
                            AS ActualPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                THEN COALESCE(
                                    kr.MPBezPDV,
                                    0
                                )
                                ELSE 0
                            END
                        )
                            AS PlanPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                THEN COALESCE(
                                    kr.RUC12,
                                    0
                                )
                                ELSE 0
                            END
                        )
                            AS ActualRuc,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                THEN COALESCE(
                                    kr.RUC12,
                                    0
                                )
                                ELSE 0
                            END
                        )
                            AS PlanRuc,

                        MAX(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                THEN 1
                                ELSE 0
                            END
                        )
                            AS ImaActual,

                        MAX(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                THEN 1
                                ELSE 0
                            END
                        )
                            AS ImaPlan

                    FROM dbo.KomercijalniRezultat kr

                    INNER JOIN dbo.Artikal a
                        ON a.ArtikalId =
                           kr.ArtikalId

                    INNER JOIN dbo.RobnaGrupa rg
                        ON rg.RobnaGrupaId =
                           a.RobnaGrupaId

                    INNER JOIN dbo.Kategorija k
                        ON k.KategorijaId =
                           rg.KategorijaId

                    {where}

                    GROUP BY
                        kr.ArtikalId
                ),

                Procenti AS
                (
                    SELECT
                        ArtikalId,
                        DobavljacId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ImaActual,
                        ImaPlan,

                        CASE
                            WHEN ActualPromet = 0
                            THEN CAST(
                                0
                                AS DECIMAL(28, 10)
                            )

                            ELSE
                                CAST(
                                    ActualRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    CAST(
                                        ActualPromet
                                        AS DECIMAL(28, 10)
                                    ),
                                    0
                                )
                        END
                            AS ActualRucProcenat,

                        CASE
                            WHEN PlanPromet = 0
                            THEN CAST(
                                0
                                AS DECIMAL(28, 10)
                            )

                            ELSE
                                CAST(
                                    PlanRuc
                                    AS DECIMAL(28, 10)
                                )
                                /
                                NULLIF(
                                    CAST(
                                        PlanPromet
                                        AS DECIMAL(28, 10)
                                    ),
                                    0
                                )
                        END
                            AS PlanRucProcenat

                    FROM Agregirano
                ),

                Izracunato AS
                (
                    SELECT
                        ArtikalId,
                        DobavljacId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ImaActual,
                        ImaPlan,
                        ActualRucProcenat,
                        PlanRucProcenat,

                        ActualRucProcenat
                        -
                        PlanRucProcenat
                            AS OdstupanjeProcentniPoeni,

                        CASE
                            WHEN
                                ImaPlan = 0
                                OR PlanPromet = 0
                            THEN CAST(
                                0
                                AS DECIMAL(28, 6)
                            )

                            ELSE
                                CAST(
                                    (
                                        ActualRucProcenat
                                        -
                                        PlanRucProcenat
                                    )
                                    *
                                    PlanPromet
                                    AS DECIMAL(28, 6)
                                )
                        END
                            AS MarginEffect

                    FROM Procenti
                )

                SELECT
                    i.ArtikalId,

                    a.Sifra,

                    a.Naziv,

                    d.Naziv
                        AS Dobavljac,

                    i.ActualPromet
                        AS Promet,

                    i.PlanPromet,

                    i.ActualRuc
                        AS RUC12,

                    i.PlanRuc,

                    i.ActualRucProcenat
                        AS RUC12Procenat,

                    i.PlanRucProcenat,

                    i.OdstupanjeProcentniPoeni,

                    i.MarginEffect
                        AS NedostatakMargine,

                    CASE
                        WHEN
                            i.ActualRuc <= 0
                            AND i.MarginEffect = 0
                        THEN i.ActualRuc

                        ELSE i.MarginEffect
                    END
                        AS ProcenjeniUticaj

                INTO #Kriticni

                FROM Izracunato i

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId =
                       i.ArtikalId

                LEFT JOIN dbo.Dobavljac d
                    ON d.DobavljacId =
                       i.DobavljacId

                WHERE
                    (
                        i.ImaActual = 1
                        AND i.ActualRuc <= 0
                    )

                    OR

                    (
                        i.ImaActual = 1
                        AND i.ImaPlan = 1
                        AND i.PlanPromet <> 0
                        AND
                        i.OdstupanjeProcentniPoeni
                            < @PragOdstupanja
                    );

                SELECT
                    COUNT(*)
                FROM #Kriticni;

                SELECT
                    ArtikalId,
                    Sifra,
                    Naziv,
                    Dobavljac,
                    Promet,
                    PlanPromet,
                    RUC12,
                    PlanRuc,
                    RUC12Procenat,
                    PlanRucProcenat,
                    OdstupanjeProcentniPoeni,
                    NedostatakMargine,
                    ProcenjeniUticaj,

                    CASE
                        WHEN RUC12 <= 0
                        THEN 'Kritično'

                        WHEN
                            OdstupanjeProcentniPoeni
                            < 0
                        THEN 'Ispod plana'

                        WHEN
                            OdstupanjeProcentniPoeni
                            < @PragOdstupanja
                        THEN 'Blizu plana'

                        ELSE 'Dobro'
                    END
                        AS Status

                FROM #Kriticni

                ORDER BY
                    CASE
                        WHEN RUC12 <= 0
                        THEN 0
                        ELSE 1
                    END ASC,

                    ProcenjeniUticaj ASC,

                    OdstupanjeProcentniPoeni ASC,

                    ArtikalId ASC

                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                DROP TABLE #Kriticni;
                """;

            using var connection =
                _connection.CreateConnection();

            var openSw =
                Stopwatch.StartNew();

            connection.Open();

            openSw.Stop();

            var querySw =
                Stopwatch.StartNew();

            int totalCount;

            List<CriticalProductsPageDTO> items;

            using (
                var multi =
                    await connection
                        .QueryMultipleAsync(
                            sql,
                            parametri,
                            commandTimeout: 30
                        )
            )
            {
                totalCount =
                    await multi
                        .ReadSingleAsync<int>();

                items =
                    (
                        await multi
                            .ReadAsync<
                                CriticalProductsPageDTO>()
                    )
                    .ToList();
            }

            querySw.Stop();
            totalSw.Stop();

            Console.WriteLine(
                $"CRITICAL PAGE [{requestId}] " +
                $"OPEN: {openSw.ElapsedMilliseconds} ms | " +
                $"QUERY: {querySw.ElapsedMilliseconds} ms | " +
                $"TOTAL: {totalSw.ElapsedMilliseconds} ms | " +
                $"COUNT: {totalCount} | " +
                $"RETURNED: {items.Count}"
            );

            return new PaginationGeneric<
                CriticalProductsPageDTO>
            {
                Items =
                    items,

                Page =
                    pagination.Page,

                PageSize =
                    pagination.PageSize,

                TotalCount =
                    totalCount
            };
        }
    }
}