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

            var sw =
                Stopwatch.StartNew();

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
                                    THEN kr.MPBezPDV
                                ELSE 0
                            END
                        ) AS ActualPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN kr.MPBezPDV
                                ELSE 0
                            END
                        ) AS PlanPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN kr.RUC12
                                ELSE 0
                            END
                        ) AS ActualRuc,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN kr.RUC12
                                ELSE 0
                            END
                        ) AS PlanRuc,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN 1
                                ELSE 0
                            END
                        ) AS ActualBrojRedova,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN 1
                                ELSE 0
                            END
                        ) AS PlanBrojRedova

                    FROM dbo.KomercijalniRezultat kr

                    INNER JOIN dbo.Artikal a
                        ON a.ArtikalId = kr.ArtikalId

                    INNER JOIN dbo.RobnaGrupa rg
                        ON rg.RobnaGrupaId = a.RobnaGrupaId

                    INNER JOIN dbo.Kategorija k
                        ON k.KategorijaId = rg.KategorijaId

                    {where}

                    GROUP BY
                        kr.ArtikalId
                ),

                Izracunato AS
                (
                    SELECT
                        ArtikalId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ActualBrojRedova,
                        PlanBrojRedova,

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
                        END AS PlanRucProcenat

                    FROM Agregirano
                ),

                Kriticni AS
                (
                    SELECT
                        ArtikalId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ActualBrojRedova,
                        PlanBrojRedova,
                        ActualRucProcenat,
                        PlanRucProcenat,

                        ActualRucProcenat
                        -
                        PlanRucProcenat
                            AS OdstupanjeProcentniPoeni,

                        CASE
                            WHEN
                                PlanBrojRedova = 0
                                OR PlanPromet = 0
                            THEN 0

                            ELSE
                                (
                                    ActualRucProcenat
                                    -
                                    PlanRucProcenat
                                )
                                *
                                PlanPromet
                        END AS MarginEffect

                    FROM Izracunato

                    WHERE
                        (
                            ActualBrojRedova > 0
                            AND ActualRuc <= 0
                        )
                        OR
                        (
                            ActualBrojRedova > 0
                            AND PlanBrojRedova > 0
                            AND PlanPromet <> 0
                            AND
                            (
                                ActualRucProcenat
                                -
                                PlanRucProcenat
                            ) < @PragOdstupanja
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

                        MarginEffect ASC,

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
                            t.OdstupanjeProcentniPoeni <= -0.020
                            THEN 'Visok'

                        WHEN
                            t.OdstupanjeProcentniPoeni < 0
                            THEN 'Srednji'

                        ELSE 'Nizak'
                    END AS Severnost,

                    CASE
                        WHEN
                            t.ActualRuc <= 0
                            AND t.MarginEffect = 0
                            THEN t.ActualRuc

                        ELSE t.MarginEffect
                    END AS ProcenjeniUticaj

                FROM TopPet t

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = t.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                INNER JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId

                ORDER BY
                    CASE
                        WHEN t.ActualRuc <= 0
                            THEN 0
                        ELSE 1
                    END ASC,

                    t.MarginEffect ASC,

                    t.OdstupanjeProcentniPoeni ASC,

                    t.ArtikalId ASC

                OPTION (RECOMPILE);
                """;

            using var connection =
                _connection.CreateConnection();

            connection.Open();

            var rezultat =
                await connection
                    .QueryAsync<CriticalProductsDTO>(
                        sql,
                        parametri,
                        commandTimeout: 30
                    );

            sw.Stop();

            Console.WriteLine(
                $"CRITICAL [{requestId}] END " +
                $"{sw.ElapsedMilliseconds} ms"
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
            var requestId =
                Guid.NewGuid()
                    .ToString("N")[..8];

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

            var aggWhere =
                new StringBuilder();

            aggWhere.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @DatumOd
                    AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                    AND kr.TipProdajeId IN (6, 7)
                """
            );

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

            if (filter.DobavljacId.HasValue)
            {
                aggWhere.AppendLine(
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

            var outerWhere =
                new StringBuilder();

            outerWhere.AppendLine(
                """
                WHERE
                    ar.Aktivan = 1
                """
            );

            if (filter.OdeljenjeId.HasValue)
            {
                outerWhere.AppendLine(
                    """
                    AND kat.OdeljenjeId = @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filter.OdeljenjeId.Value
                );
            }

            if (filter.KategorijaId.HasValue)
            {
                outerWhere.AppendLine(
                    """
                    AND kat.KategorijaId = @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filter.KategorijaId.Value
                );
            }

            if (!canViewAllCategories)
            {
                outerWhere.AppendLine(
                    """
                    AND kat.KategorijaId IN @KategorijaIds
                    """
                );

                parametri.Add(
                    "KategorijaIds",
                    kategorijaIds
                );
            }

            var offset =
                (pagination.Page - 1)
                *
                pagination.PageSize;

            parametri.Add(
                "Offset",
                offset
            );

            parametri.Add(
                "PageSize",
                pagination.PageSize
            );

            string buildTempTableSql =
                $"""
                IF OBJECT_ID('tempdb..#Kriticni') IS NOT NULL
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
                        ) AS DobavljacId,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN kr.MPBezPDV
                                ELSE 0
                            END
                        ) AS ActualPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN kr.MPBezPDV
                                ELSE 0
                            END
                        ) AS PlanPromet,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN kr.RUC12
                                ELSE 0
                            END
                        ) AS ActualRuc,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN kr.RUC12
                                ELSE 0
                            END
                        ) AS PlanRuc,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN 1
                                ELSE 0
                            END
                        ) AS ActualBrojRedova,

                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN 1
                                ELSE 0
                            END
                        ) AS PlanBrojRedova

                    FROM dbo.KomercijalniRezultat kr

                    INNER JOIN dbo.Artikal a
                        ON a.ArtikalId = kr.ArtikalId

                    {aggWhere}

                    GROUP BY
                        kr.ArtikalId
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
                        ActualBrojRedova,
                        PlanBrojRedova,

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
                        END AS PlanRucProcenat

                    FROM Agregirano
                ),

                Finalno AS
                (
                    SELECT
                        ArtikalId,
                        DobavljacId,
                        ActualPromet,
                        PlanPromet,
                        ActualRuc,
                        PlanRuc,
                        ActualBrojRedova,
                        PlanBrojRedova,
                        ActualRucProcenat,
                        PlanRucProcenat,

                        ActualRucProcenat
                        -
                        PlanRucProcenat
                            AS OdstupanjeProcentniPoeni,

                        CASE
                            WHEN
                                PlanBrojRedova = 0
                                OR PlanPromet = 0
                            THEN 0

                            ELSE
                                (
                                    ActualRucProcenat
                                    -
                                    PlanRucProcenat
                                )
                                *
                                PlanPromet
                        END AS MarginEffect

                    FROM Izracunato
                )

                SELECT
                    ArtikalId,
                    DobavljacId,

                    ActualPromet
                        AS Promet,

                    PlanPromet,

                    ActualRuc
                        AS RUC12,

                    PlanRuc,

                    ActualRucProcenat
                        AS RUC12Procenat,

                    PlanRucProcenat,

                    OdstupanjeProcentniPoeni,

                    MarginEffect
                        AS NedostatakMargine,

                    CASE
                        WHEN
                            ActualRuc <= 0
                            AND MarginEffect = 0
                            THEN ActualRuc

                        ELSE MarginEffect
                    END AS ProcenjeniUticaj

                INTO #Kriticni

                FROM Finalno

                WHERE
                    (
                        ActualBrojRedova > 0
                        AND ActualRuc <= 0
                    )
                    OR
                    (
                        ActualBrojRedova > 0
                        AND PlanBrojRedova > 0
                        AND PlanPromet <> 0
                        AND OdstupanjeProcentniPoeni
                            < @PragOdstupanja
                    );

                CREATE CLUSTERED INDEX IX_Temp_Kriticni_ArtikalId
                    ON #Kriticni (ArtikalId);

                CREATE NONCLUSTERED INDEX IX_Temp_Kriticni_Uticaj
                    ON #Kriticni (
                        ProcenjeniUticaj,
                        OdstupanjeProcentniPoeni,
                        ArtikalId
                    );
                """;

            string countSql =
                $"""
                SELECT
                    COUNT(*)

                FROM #Kriticni k

                INNER JOIN dbo.Artikal ar
                    ON ar.ArtikalId = k.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = ar.RobnaGrupaId

                INNER JOIN dbo.Kategorija kat
                    ON kat.KategorijaId = rg.KategorijaId

                {outerWhere};
                """;

            string pageSql =
                $"""
                SELECT
                    ar.ArtikalId,
                    ar.Sifra,
                    ar.Naziv,

                    d.Naziv
                        AS Dobavljac,

                    k.Promet,

                    k.PlanPromet,

                    k.RUC12,

                    k.PlanRuc,

                    k.RUC12Procenat,

                    k.PlanRucProcenat,

                    k.OdstupanjeProcentniPoeni,

                    k.NedostatakMargine,

                    k.ProcenjeniUticaj,

                    CASE
                        WHEN k.RUC12 <= 0
                            THEN 'Kritično'

                        WHEN k.OdstupanjeProcentniPoeni < 0
                            THEN 'Ispod plana'

                        WHEN k.OdstupanjeProcentniPoeni < @PragOdstupanja
                            THEN 'Blizu plana'

                        ELSE 'Dobro'
                    END AS Status

                FROM #Kriticni k

                INNER JOIN dbo.Artikal ar
                    ON ar.ArtikalId = k.ArtikalId

                LEFT JOIN dbo.Dobavljac d
                    ON d.DobavljacId = k.DobavljacId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = ar.RobnaGrupaId

                INNER JOIN dbo.Kategorija kat
                    ON kat.KategorijaId = rg.KategorijaId

                {outerWhere}

                ORDER BY
                    k.ProcenjeniUticaj ASC,
                    k.OdstupanjeProcentniPoeni ASC,
                    ar.ArtikalId ASC

                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
                """;

            string dropTempTableSql =
                """
                IF OBJECT_ID('tempdb..#Kriticni') IS NOT NULL
                    DROP TABLE #Kriticni;
                """;

            string combinedSql =
                buildTempTableSql
                + Environment.NewLine
                + countSql
                + Environment.NewLine
                + pageSql
                + Environment.NewLine
                + dropTempTableSql;

            using var connection =
                _connection.CreateConnection();

            var sw =
                Stopwatch.StartNew();

            connection.Open();

            int totalCount;
            List<CriticalProductsPageDTO> items;

            using (
                var multi =
                    await connection
                        .QueryMultipleAsync(
                            combinedSql,
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

            sw.Stop();

            Console.WriteLine(
                $"CRITICAL PAGE [{requestId}] " +
                $"{sw.ElapsedMilliseconds} ms | " +
                $"Total: {totalCount} | " +
                $"Page: {pagination.Page} | " +
                $"PageSize: {pagination.PageSize} | " +
                $"Returned: {items.Count}"
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