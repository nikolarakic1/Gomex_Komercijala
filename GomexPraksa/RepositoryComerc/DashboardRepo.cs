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
        DashboardFilterDTO filterDTO)
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
            WITH TrenutniPeriod AS
            (
                SELECT
                    kr.ArtikalId,
                    kr.MPBezPDV,
                    kr.RUC12,
                    kr.NedostatakMargine,
                    kr.DatumUnosa
                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = kr.ArtikalId

                LEFT JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                LEFT JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId

                WHERE kr.DatumUnosa >= @DatumOd
                  AND kr.DatumUnosa < DATEADD(DAY, 1, @DatumDo)

                  AND (
                      @OdeljenjeId IS NULL
                      OR k.OdeljenjeId = @OdeljenjeId
                  )

                  AND (
                      @KategorijaId IS NULL
                      OR k.KategorijaId = @KategorijaId
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

            PrethodniPeriod AS
            (
                SELECT
                    kr.ArtikalId,
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

                WHERE kr.DatumUnosa >= @PrethodniDatumOd
                  AND kr.DatumUnosa < DATEADD(DAY, 1, @PrethodniDatumDo)

                  AND (
                      @OdeljenjeId IS NULL
                      OR k.OdeljenjeId = @OdeljenjeId
                  )

                  AND (
                      @KategorijaId IS NULL
                      OR k.KategorijaId = @KategorijaId
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

            TrenutniPoArtiklu AS
            (
                SELECT
                    ArtikalId,
                    COALESCE(SUM(MPBezPDV), 0) AS PrometBezPdv,
                    COALESCE(SUM(RUC12), 0) AS Ruc12,
                    COALESCE(
                        SUM(NedostatakMargine),
                        0
                    ) AS NedostatakMarze
                FROM TrenutniPeriod
                GROUP BY ArtikalId
            ),

            PrethodniPoArtiklu AS
            (
                SELECT
                    ArtikalId,
                    COALESCE(SUM(MPBezPDV), 0) AS PrometBezPdv,
                    COALESCE(SUM(RUC12), 0) AS Ruc12,
                    COALESCE(
                        SUM(NedostatakMargine),
                        0
                    ) AS NedostatakMarze
                FROM PrethodniPeriod
                GROUP BY ArtikalId
            ),

            TrenutniUkupno AS
            (
                SELECT
                    COALESCE(
                        SUM(PrometBezPdv),
                        0
                    ) AS PrometBezPdv,

                    COALESCE(
                        SUM(Ruc12),
                        0
                    ) AS Ruc12,

                    COALESCE(
                        SUM(NedostatakMarze),
                        0
                    ) AS NedostatakMarze,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN Ruc12 <= 0
                                  OR NedostatakMarze > 0
                                THEN 1
                                ELSE 0
                            END
                        ),
                        0
                    ) AS KriticniArtikli
                FROM TrenutniPoArtiklu
            ),

            PrethodniUkupno AS
            (
                SELECT
                    COALESCE(
                        SUM(PrometBezPdv),
                        0
                    ) AS PrometBezPdv,

                    COALESCE(
                        SUM(Ruc12),
                        0
                    ) AS Ruc12,

                    COALESCE(
                        SUM(NedostatakMarze),
                        0
                    ) AS NedostatakMarze,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN Ruc12 <= 0
                                  OR NedostatakMarze > 0
                                THEN 1
                                ELSE 0
                            END
                        ),
                        0
                    ) AS KriticniArtikli
                FROM PrethodniPoArtiklu
            )

            SELECT
                t.PrometBezPdv,

                CASE
                    WHEN p.PrometBezPdv = 0 THEN 0
                    ELSE
                        CAST(
                            t.PrometBezPdv - p.PrometBezPdv
                            AS DECIMAL(18, 6)
                        )
                        / NULLIF(p.PrometBezPdv, 0)
                END AS PrometPromenaProcenat,

                t.Ruc12,

                CASE
                    WHEN p.Ruc12 = 0 THEN 0
                    ELSE
                        CAST(
                            t.Ruc12 - p.Ruc12
                            AS DECIMAL(18, 6)
                        )
                        / NULLIF(p.Ruc12, 0)
                END AS Ruc12PromenaProcenat,

                CASE
                    WHEN t.PrometBezPdv = 0 THEN 0
                    ELSE
                        CAST(t.Ruc12 AS DECIMAL(18, 6))
                        / NULLIF(t.PrometBezPdv, 0)
                END AS Ruc12Procenat,

                (
                    CASE
                        WHEN t.PrometBezPdv = 0 THEN 0
                        ELSE
                            CAST(t.Ruc12 AS DECIMAL(18, 6))
                            / NULLIF(t.PrometBezPdv, 0)
                    END
                    -
                    CASE
                        WHEN p.PrometBezPdv = 0 THEN 0
                        ELSE
                            CAST(p.Ruc12 AS DECIMAL(18, 6))
                            / NULLIF(p.PrometBezPdv, 0)
                    END
                ) AS Ruc12PromenaProcentniPoeni,

                t.KriticniArtikli,

                t.KriticniArtikli
                - p.KriticniArtikli
                    AS KriticniArtikliPromena,

                t.NedostatakMarze,

                CASE
                    WHEN p.NedostatakMarze = 0 THEN 0
                    ELSE
                        CAST(
                            t.NedostatakMarze
                            - p.NedostatakMarze
                            AS DECIMAL(18, 6)
                        )
                        / NULLIF(p.NedostatakMarze, 0)
                END AS NedostatakMarzePromenaProcenat,

                (
                    SELECT MAX(DatumUnosa)
                    FROM TrenutniPeriod
                ) AS PodaciOsvezeni

            FROM TrenutniUkupno t
            CROSS JOIN PrethodniUkupno p;
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
                    filterDTO.TipProdajeId
                });
    }
}