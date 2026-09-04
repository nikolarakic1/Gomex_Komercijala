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

        public CriticalProductsRepo(
            IConnFactory connection)
        {
            _connection = connection;
        }

        // =============================================
        // TOP 5 KRITICNIH ARTIKALA
        // =============================================

        public async Task<IEnumerable<CriticalProductsDTO>>
            CriticalProductsTop5(
                DashboardFilterDTO filter,
                DateOnly datumOd,
                DateOnly datumDo,
                bool canViewAllCategories,
                List<int> kategorijaIds)
        {
            var requestId = Guid.NewGuid()
                .ToString("N")[..8];

            Console.WriteLine(
                $"CRITICAL [{requestId}] START " +
                $"{filter.DatumOd:yyyy-MM-dd} -> " +
                $"{filter.DatumDo:yyyy-MM-dd}"
            );

            var where =
                new StringBuilder();

            where.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @DatumOd
                    AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
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

            // =============================================
            // ODELJENJE
            // =============================================

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

            // =============================================
            // KATEGORIJA
            // =============================================

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

            // =============================================
            // DOBAVLJAC
            // =============================================

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

            // =============================================
            // TIP PRODAJE
            // =============================================

            if (filter.TipProdajeId.HasValue)
            {
                where.AppendLine(
                    """
                    AND kr.TipProdajeId = @TipProdajeId
                    """
                );

                parametri.Add(
                    "TipProdajeId",
                    filter.TipProdajeId.Value
                );
            }

            // =============================================
            // USER ACCESS
            // =============================================

            if (!canViewAllCategories)
            {
                if (kategorijaIds == null ||
                    kategorijaIds.Count == 0)
                {
                    return Enumerable.Empty<
                        CriticalProductsDTO
                    >();
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
                WITH Agregirano AS
                (
                    SELECT
                        kr.ArtikalId,

                        SUM(kr.RUC12)
                            AS Ruc12,

                        SUM(
                            COALESCE(
                                kr.MarginEffect,
                                0
                            )
                            +
                            COALESCE(
                                kr.MixEffect,
                                0
                            )
                        )
                            AS NedostatakMargine

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

                Kriticni AS
                (
                    SELECT
                        ArtikalId,
                        Ruc12,
                        NedostatakMargine,

                        (
                            CASE
                                WHEN Ruc12 < 0
                                    THEN Ruc12
                                ELSE 0
                            END

                            +

                            CASE
                                WHEN NedostatakMargine < 0
                                    THEN NedostatakMargine
                                ELSE 0
                            END
                        )
                            AS ProcenjeniUticaj

                    FROM Agregirano

                    WHERE
                        Ruc12 <= 0
                        OR NedostatakMargine < 0
                ),

                TopPet AS
                (
                    SELECT TOP (5)
                        ArtikalId,
                        Ruc12,
                        NedostatakMargine,
                        ProcenjeniUticaj

                    FROM Kriticni

                    ORDER BY
                        ProcenjeniUticaj ASC
                )

                SELECT
                    t.ArtikalId,

                    a.Naziv
                        AS NazivArtikla,

                    k.Naziv
                        AS Kategorija,

                    CASE
                        WHEN
                            ABS(t.Ruc12) >= 5000
                            OR
                            ABS(t.NedostatakMargine) >= 5000
                            THEN 'Visok'

                        WHEN
                            ABS(t.Ruc12) >= 2000
                            OR
                            ABS(t.NedostatakMargine) >= 2000
                            THEN 'Srednji'

                        ELSE 'Nizak'
                    END
                        AS Severnost,

                    t.ProcenjeniUticaj

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
                    t.ProcenjeniUticaj ASC

                OPTION (RECOMPILE);
                """;

            using var connection =
                _connection.CreateConnection();

            connection.Open();

            var rezultat =
                await connection
                    .QueryAsync<CriticalProductsDTO>(
                        sql,
                        parametri
                    );

            return rezultat;
        }

        // =============================================
        // CRITICAL PAGE
        // PAGINACIJA
        // =============================================

        public async Task<
            PaginationGeneric<CriticalProductsPageDTO>>
            ShowCriticalProductsAsync(
                FilterSharedPages filter,
                PaginationParams pagination,
                bool canViewAllCategories,
                List<int> kategorijaIds)
        {
            // =============================================
            // USER NEMA NIJEDNU KATEGORIJU
            // =============================================

            if (!canViewAllCategories &&
                (
                    kategorijaIds == null ||
                    kategorijaIds.Count == 0
                ))
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

            // =============================================
            // FILTERI PRE AGREGACIJE
            // =============================================

            var aggWhere =
                new StringBuilder();

            aggWhere.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @DatumOd
                    AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
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

            // =============================================
            // DOBAVLJAC
            // =============================================

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

            // =============================================
            // TIP PRODAJE
            // =============================================

            if (filter.TipProdajeId.HasValue)
            {
                aggWhere.AppendLine(
                    """
                    AND kr.TipProdajeId =
                        @TipProdajeId
                    """
                );

                parametri.Add(
                    "TipProdajeId",
                    filter.TipProdajeId.Value
                );
            }

            // =============================================
            // FILTERI POSLE AGREGACIJE
            // =============================================

            var outerWhere =
                new StringBuilder();

            outerWhere.AppendLine(
                """
                WHERE
                    ar.Aktivan = 1
                """
            );

            // =============================================
            // ODELJENJE
            // =============================================

            if (filter.OdeljenjeId.HasValue)
            {
                outerWhere.AppendLine(
                    """
                    AND kat.OdeljenjeId =
                        @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filter.OdeljenjeId.Value
                );
            }

            // =============================================
            // KATEGORIJA
            // =============================================

            if (filter.KategorijaId.HasValue)
            {
                outerWhere.AppendLine(
                    """
                    AND kat.KategorijaId =
                        @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filter.KategorijaId.Value
                );
            }

            // =============================================
            // MENADZEROVE KATEGORIJE
            // =============================================

            if (!canViewAllCategories)
            {
                outerWhere.AppendLine(
                    """
                    AND kat.KategorijaId
                        IN @KategorijaIds
                    """
                );

                parametri.Add(
                    "KategorijaIds",
                    kategorijaIds
                );
            }

            // =============================================
            // PAGINACIJA
            // =============================================

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

            // =============================================
            // ZAJEDNICKI SQL
            // =============================================

            string cte =
                $"""
                WITH Agregirano AS
                (
                    SELECT
                        kr.ArtikalId,

                        COALESCE(
                            kr.DobavljacId,
                            a.DobavljacId
                        )
                            AS DobavljacId,

                        SUM(
                            kr.MPBezPDV
                        )
                            AS Promet,

                        SUM(
                            kr.RUC12
                        )
                            AS RUC12,

                        SUM(
                            COALESCE(
                                kr.MarginEffect,
                                0
                            )
                            +
                            COALESCE(
                                kr.MixEffect,
                                0
                            )
                        )
                            AS NedostatakMargine

                    FROM dbo.KomercijalniRezultat kr

                    INNER JOIN dbo.Artikal a
                        ON a.ArtikalId =
                           kr.ArtikalId

                    {aggWhere}

                    GROUP BY
                        kr.ArtikalId,

                        COALESCE(
                            kr.DobavljacId,
                            a.DobavljacId
                        )
                ),

                Kriticni AS
                (
                    SELECT
                        ArtikalId,
                        DobavljacId,
                        Promet,
                        RUC12,
                        NedostatakMargine

                    FROM Agregirano

                    WHERE
                        RUC12 <= 0
                        OR NedostatakMargine < 0
                )
                """;

            // =============================================
            // COUNT QUERY
            // =============================================

            string countSql =
                $"""
                {cte}

                SELECT
                    COUNT(*)

                FROM Kriticni k

                INNER JOIN dbo.Artikal ar
                    ON ar.ArtikalId =
                       k.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId =
                       ar.RobnaGrupaId

                INNER JOIN dbo.Kategorija kat
                    ON kat.KategorijaId =
                       rg.KategorijaId

                {outerWhere}

                OPTION (RECOMPILE);
                """;

            // =============================================
            // PAGE QUERY
            // =============================================

            string pageSql =
                $"""
                {cte}

                SELECT
                    ar.ArtikalId,

                    ar.Sifra,

                    ar.Naziv,

                    d.Naziv
                        AS Dobavljac,

                    k.Promet,

                    k.RUC12,

                    CASE
                        WHEN k.Promet = 0
                            THEN 0

                        ELSE
                            CAST(
                                k.RUC12
                                AS DECIMAL(18, 6)
                            )
                            /
                            NULLIF(
                                k.Promet,
                                0
                            )
                    END
                        AS RUC12Procenat,

                    k.NedostatakMargine

                FROM Kriticni k

                INNER JOIN dbo.Artikal ar
                    ON ar.ArtikalId =
                       k.ArtikalId

                LEFT JOIN dbo.Dobavljac d
                    ON d.DobavljacId =
                       k.DobavljacId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId =
                       ar.RobnaGrupaId

                INNER JOIN dbo.Kategorija kat
                    ON kat.KategorijaId =
                       rg.KategorijaId

                {outerWhere}

                ORDER BY
                    k.NedostatakMargine ASC,
                    ar.ArtikalId ASC

                OFFSET @Offset ROWS

                FETCH NEXT
                    @PageSize ROWS ONLY

                OPTION (RECOMPILE);
                """;

            // =============================================
            // CONNECTION
            // =============================================

            using var connection =
                _connection.CreateConnection();

            var openSw =
                Stopwatch.StartNew();

            connection.Open();

            openSw.Stop();

            Console.WriteLine(
                $"CRITICAL PAGE CONNECTION OPEN: " +
                $"{openSw.ElapsedMilliseconds} ms"
            );

            // =============================================
            // COUNT
            // =============================================

            var totalSw =
                Stopwatch.StartNew();

            var totalCount =
                await connection
                    .ExecuteScalarAsync<int>(
                        countSql,
                        parametri
                    );

            totalSw.Stop();

            Console.WriteLine(
                $"CRITICAL PAGE COUNT: " +
                $"{totalSw.ElapsedMilliseconds} ms | " +
                $"Total: {totalCount}"
            );

            // =============================================
            // PAGE
            // =============================================

            var pageSw =
                Stopwatch.StartNew();

            var items =
                (
                    await connection
                        .QueryAsync<
                            CriticalProductsPageDTO>(
                            pageSql,
                            parametri
                        )
                )
                .ToList();

            pageSw.Stop();

            Console.WriteLine(
                $"CRITICAL PAGE DATA: " +
                $"{pageSw.ElapsedMilliseconds} ms | " +
                $"Page: {pagination.Page} | " +
                $"PageSize: {pagination.PageSize} | " +
                $"Returned: {items.Count}"
            );

            // =============================================
            // RESULT
            // =============================================

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