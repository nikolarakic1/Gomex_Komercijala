using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.Dtos;
using Models.DtosComerc;

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
            const string sql = """
    WITH Kriticni AS
    (
        SELECT
            kr.ArtikalId,

            SUM(kr.RUC12)
                AS Ruc12,

            SUM(
                COALESCE(kr.MarginEffect, 0)
                +
                COALESCE(kr.MixEffect, 0)
            )
                AS NedostatakMargine

        FROM dbo.KomercijalniRezultat kr

        INNER JOIN dbo.Artikal a
            ON a.ArtikalId = kr.ArtikalId

        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId =
               a.RobnaGrupaId

        INNER JOIN dbo.Kategorija k
            ON k.KategorijaId =
               rg.KategorijaId

        WHERE
            kr.DatumRezultata >= @DatumOd

            AND kr.DatumRezultata
                < DATEADD(
                    DAY,
                    1,
                    @DatumDo
                )

            AND a.Aktivan = 1

            AND
            (
                @OdeljenjeId IS NULL
                OR k.OdeljenjeId =
                   @OdeljenjeId
            )

            AND
            (
                @KategorijaId IS NULL
                OR k.KategorijaId =
                   @KategorijaId
            )

            -- Novi import koristi dobavljaca
            -- sa KomercijalniRezultat.
            -- Za stare podatke fallback je Artikal.
            AND
            (
                @DobavljacId IS NULL
                OR COALESCE(
                    kr.DobavljacId,
                    a.DobavljacId
                ) = @DobavljacId
            )

            AND
            (
                @TipProdajeId IS NULL
                OR kr.TipProdajeId =
                   @TipProdajeId
            )

            AND
            (
                @CanViewAllCategories = 1
                OR k.KategorijaId
                   IN @KategorijaIds
            )

        GROUP BY
            kr.ArtikalId

        HAVING
            SUM(kr.RUC12) <= 0

            OR

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
            ) < 0
    )

    SELECT TOP 5
        a.ArtikalId,

        a.Naziv
            AS NazivArtikla,

        k.Naziv
            AS Kategorija,

        CASE
            WHEN
                ABS(kr.Ruc12) >= 5000
                OR
                ABS(
                    kr.NedostatakMargine
                ) >= 5000
                THEN 'Visok'

            WHEN
                ABS(kr.Ruc12) >= 2000
                OR
                ABS(
                    kr.NedostatakMargine
                ) >= 2000
                THEN 'Srednji'

            ELSE 'Nizak'
        END
            AS Severnost,

        (
            CASE
                WHEN kr.Ruc12 < 0
                    THEN kr.Ruc12
                ELSE 0
            END
            +
            CASE
                WHEN kr.NedostatakMargine < 0
                    THEN kr.NedostatakMargine
                ELSE 0
            END
        )
            AS ProcenjeniUticaj

    FROM Kriticni kr

    INNER JOIN dbo.Artikal a
        ON a.ArtikalId =
           kr.ArtikalId

    INNER JOIN dbo.RobnaGrupa rg
        ON rg.RobnaGrupaId =
           a.RobnaGrupaId

    INNER JOIN dbo.Kategorija k
        ON k.KategorijaId =
           rg.KategorijaId

    ORDER BY
        ProcenjeniUticaj ASC;
    """;

            using var connection =
                _connection.CreateConnection();

            return await connection
                .QueryAsync<CriticalProductsDTO>(
                    sql,
                    new
                    {
                        DatumOd =
                            datumOd.ToDateTime(
                                TimeOnly.MinValue),

                        DatumDo =
                            datumDo.ToDateTime(
                                TimeOnly.MinValue),

                        filter.OdeljenjeId,

                        filter.KategorijaId,

                        filter.DobavljacId,

                        filter.TipProdajeId,

                        CanViewAllCategories =
                            canViewAllCategories,

                        KategorijaIds =
                            kategorijaIds
                    }
                );
        }

        // =============================================
        // STRANICA SVIH KRITICNIH ARTIKALA
        // =============================================

        public async Task<IEnumerable<CriticalProductsPageDTO>>
            ShowCriticalProductsAsync(
                FilterSharedPages filter,
                bool canViewAllCategories,
                List<int> kategorijaIds)
        {
            const string sql = """
    SELECT
        ar.ArtikalId,

        ar.Sifra,

        ar.Naziv,

        d.Naziv
            AS Dobavljac,

        SUM(kr.MPBezPDV)
            AS Promet,

        SUM(kr.RUC12)
            AS RUC12,

        CASE
            WHEN SUM(kr.MPBezPDV) = 0
                THEN 0
            ELSE
                CAST(
                    SUM(kr.RUC12)
                    AS DECIMAL(18, 6)
                )
                /
                NULLIF(
                    SUM(kr.MPBezPDV),
                    0
                )
        END
            AS RUC12Procenat,

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

    INNER JOIN dbo.Artikal ar
        ON kr.ArtikalId =
           ar.ArtikalId

    -- Novi import:
    -- dobavljac dolazi sa KomercijalniRezultat.
    -- Stari redovi:
    -- fallback na Artikal.DobavljacId.
    LEFT JOIN dbo.Dobavljac d
        ON d.DobavljacId =
           COALESCE(
               kr.DobavljacId,
               ar.DobavljacId
           )

    INNER JOIN dbo.RobnaGrupa rg
        ON ar.RobnaGrupaId =
           rg.RobnaGrupaId

    INNER JOIN dbo.Kategorija k
        ON rg.KategorijaId =
           k.KategorijaId

    WHERE
        kr.DatumRezultata >=
            @DatumOd

        AND kr.DatumRezultata
            < DATEADD(
                DAY,
                1,
                @DatumDo
            )

        AND ar.Aktivan = 1

        AND
        (
            @OdeljenjeId IS NULL
            OR k.OdeljenjeId =
               @OdeljenjeId
        )

        AND
        (
            @KategorijaId IS NULL
            OR k.KategorijaId =
               @KategorijaId
        )

        AND
        (
            @DobavljacId IS NULL
            OR COALESCE(
                kr.DobavljacId,
                ar.DobavljacId
            ) = @DobavljacId
        )

        AND
        (
            @TipProdajeId IS NULL
            OR kr.TipProdajeId =
               @TipProdajeId
        )

        AND
        (
            @CanViewAllCategories = 1
            OR k.KategorijaId
               IN @KategorijaIds
        )

    GROUP BY
        ar.ArtikalId,
        ar.Sifra,
        ar.Naziv,
        d.Naziv

    HAVING
        SUM(kr.RUC12) <= 0

        OR

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
        ) < 0

    ORDER BY
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
        ) ASC;
    """;

            using var connection =
                _connection.CreateConnection();

            return await connection
                .QueryAsync<CriticalProductsPageDTO>(
                    sql,
                    new
                    {
                        DatumOd =
                            filter.DatumOd.ToDateTime(
                                TimeOnly.MinValue),

                        DatumDo =
                            filter.DatumDo.ToDateTime(
                                TimeOnly.MinValue),

                        filter.OdeljenjeId,

                        filter.KategorijaId,

                        filter.DobavljacId,

                        filter.TipProdajeId,

                        CanViewAllCategories =
                            canViewAllCategories,

                        KategorijaIds =
                            kategorijaIds
                    }
                );
        }
    }
}