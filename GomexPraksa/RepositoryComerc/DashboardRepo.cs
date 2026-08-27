using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc;

public class DashboardRepo : IDashboardRepo
{
    private readonly IConnFactory _connFactory;

    public DashboardRepo(IConnFactory connFactory)
    {
        _connFactory = connFactory;
    }

    public async Task<DashboardSummaryDTO> FillCardsAsync(
        DashboardFilterDTO filterDTO, bool isAllCategoriesVisibile, List<int> KategorijaIds)
    {
        ArgumentNullException.ThrowIfNull(filterDTO);

        bool imaDatumOd = filterDTO.DatumOd.HasValue;
        bool imaDatumDo = filterDTO.DatumDo.HasValue;

        if (imaDatumOd != imaDatumDo)
        {
            throw new ArgumentException(
                "Moraju biti uneti i DatumOd i DatumDo, ili nijedan.");
        }

        DateOnly datumOd;
        DateOnly datumDo;

        if (!imaDatumOd && !imaDatumDo)
        {
            datumDo = DateOnly.FromDateTime(DateTime.Today);
            datumOd = new DateOnly(datumDo.Year, 1, 1);
        }
        else
        {
            datumOd = filterDTO.DatumOd!.Value;
            datumDo = filterDTO.DatumDo!.Value;
        }

        if (datumOd > datumDo)
        {
            throw new ArgumentException(
                "DatumOd ne može biti posle DatumDo.");
        }

        int brojDana =
            datumDo.DayNumber - datumOd.DayNumber + 1;

        DateOnly prethodniDatumDo =
            datumOd.AddDays(-1);

        DateOnly prethodniDatumOd =
            prethodniDatumDo.AddDays(-(brojDana - 1));

        const string sql = """
            WITH Podaci AS
            (
                SELECT
                    kr.ArtikalId,
                    kr.DatumRezultata,
                    kr.DatumUnosa,
                    kr.MPBezPDV,
                    kr.RUC12,
                    kr.NedostatakMargine
                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = kr.ArtikalId

                LEFT JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                LEFT JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId

                WHERE kr.DatumRezultata >= @PrethodniDatumOd
                  AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)

                  AND (
                      @OdeljenjeId IS NULL
                      OR k.OdeljenjeId = @OdeljenjeId
                  )

                  AND (
                      @KategorijaId IS NULL
                      OR k.KategorijaId = @KategorijaId
                  )
                  AND
                  (
                @CanViewAllCategories = 1
                OR k.KategorijaId IN @KategorijaIds
                  )

                  AND (
                      @DobavljacId IS NULL
                      OR a.DobavljacId = @DobavljacId
                  )

                  AND (
                      @TipProdajeId IS NULL
                      OR kr.TipProdajeId = @TipProdajeId
                  )

                  AND a.Aktivan = 1
            ),

            PoArtiklu AS
            (
                SELECT
                    ArtikalId,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN DatumRezultata >= @DatumOd
                                 AND DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                                THEN MPBezPDV
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TrenutniPromet,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN DatumRezultata >= @DatumOd
                                 AND DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                                THEN RUC12
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TrenutniRuc,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN DatumRezultata >= @DatumOd
                                 AND DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                                THEN NedostatakMargine
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TrenutniNedostatak,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN DatumRezultata >= @PrethodniDatumOd
                                 AND DatumRezultata < DATEADD(DAY, 1, @PrethodniDatumDo)
                                THEN MPBezPDV
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PrethodniPromet,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN DatumRezultata >= @PrethodniDatumOd
                                 AND DatumRezultata < DATEADD(DAY, 1, @PrethodniDatumDo)
                                THEN RUC12
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PrethodniRuc,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN DatumRezultata >= @PrethodniDatumOd
                                 AND DatumRezultata < DATEADD(DAY, 1, @PrethodniDatumDo)
                                THEN NedostatakMargine
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PrethodniNedostatak

                FROM Podaci

                GROUP BY ArtikalId
            ),

            Ukupno AS
            (
                SELECT
                    COALESCE(SUM(TrenutniPromet), 0)
                        AS TrenutniPromet,

                    COALESCE(SUM(TrenutniRuc), 0)
                        AS TrenutniRuc,

                    COALESCE(SUM(TrenutniNedostatak), 0)
                        AS TrenutniNedostatak,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN TrenutniRuc <= 0
                                  OR TrenutniNedostatak > 0
                                THEN 1
                                ELSE 0
                            END
                        ),
                        0
                    ) AS TrenutniKriticni,

                    COALESCE(SUM(PrethodniPromet), 0)
                        AS PrethodniPromet,

                    COALESCE(SUM(PrethodniRuc), 0)
                        AS PrethodniRuc,

                    COALESCE(SUM(PrethodniNedostatak), 0)
                        AS PrethodniNedostatak,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN PrethodniRuc <= 0
                                  OR PrethodniNedostatak > 0
                                THEN 1
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PrethodniKriticni

                FROM PoArtiklu
            )

            SELECT
                TrenutniPromet AS PrometBezPdv,

                CAST(
                    CASE
                        WHEN PrethodniPromet = 0 THEN 0
                        ELSE
                            CAST(
                                TrenutniPromet - PrethodniPromet
                                AS DECIMAL(28, 6)
                            )
                            / NULLIF(PrethodniPromet, 0)
                    END
                    AS DECIMAL(18, 4)
                ) AS PrometPromenaProcenat,

                TrenutniRuc AS Ruc12,

                CAST(
                    CASE
                        WHEN PrethodniRuc = 0 THEN 0
                        ELSE
                            CAST(
                                TrenutniRuc - PrethodniRuc
                                AS DECIMAL(28, 6)
                            )
                            / NULLIF(PrethodniRuc, 0)
                    END
                    AS DECIMAL(18, 4)
                ) AS Ruc12PromenaProcenat,

                CAST(
                    CASE
                        WHEN TrenutniPromet = 0 THEN 0
                        ELSE
                            CAST(
                                TrenutniRuc
                                AS DECIMAL(28, 6)
                            )
                            / NULLIF(TrenutniPromet, 0)
                    END
                    AS DECIMAL(18, 4)
                ) AS Ruc12Procenat,

                CAST(
                    (
                        CASE
                            WHEN TrenutniPromet = 0 THEN 0
                            ELSE
                                CAST(
                                    TrenutniRuc
                                    AS DECIMAL(28, 6)
                                )
                                / NULLIF(TrenutniPromet, 0)
                        END
                    )
                    -
                    (
                        CASE
                            WHEN PrethodniPromet = 0 THEN 0
                            ELSE
                                CAST(
                                    PrethodniRuc
                                    AS DECIMAL(28, 6)
                                )
                                / NULLIF(PrethodniPromet, 0)
                        END
                    )
                    AS DECIMAL(18, 4)
                ) AS Ruc12PromenaProcentniPoeni,

                TrenutniKriticni AS KriticniArtikli,

                TrenutniKriticni
                - PrethodniKriticni
                    AS KriticniArtikliPromena,

                TrenutniNedostatak
                    AS NedostatakMarze,

                CAST(
                    CASE
                        WHEN PrethodniNedostatak = 0 THEN 0
                        ELSE
                            CAST(
                                TrenutniNedostatak
                                - PrethodniNedostatak
                                AS DECIMAL(28, 6)
                            )
                            / NULLIF(PrethodniNedostatak, 0)
                    END
                    AS DECIMAL(18, 4)
                ) AS NedostatakMarzePromenaProcenat,

                (
                    SELECT MAX(DatumUnosa)
                    FROM Podaci
                    WHERE DatumRezultata >= @DatumOd
                      AND DatumRezultata < DATEADD(DAY, 1, @DatumDo)
                ) AS PodaciOsvezeni

            FROM Ukupno;
            """;

        using var connection =
            _connFactory.CreateConnection();

        return await connection
            .QuerySingleAsync<DashboardSummaryDTO>(
                sql,
                new
                {
                    DatumOd = datumOd.ToDateTime(
        TimeOnly.MinValue),

                    DatumDo = datumDo.ToDateTime(
        TimeOnly.MinValue),

                    PrethodniDatumOd =
        prethodniDatumOd.ToDateTime(
            TimeOnly.MinValue),

                    PrethodniDatumDo =
        prethodniDatumDo.ToDateTime(
            TimeOnly.MinValue),

                    filterDTO.OdeljenjeId,
                    filterDTO.KategorijaId,
                    filterDTO.DobavljacId,
                    filterDTO.TipProdajeId,

                    CanViewAllCategories = isAllCategoriesVisibile,
                    KategorijaIds = KategorijaIds
                });
    }



}