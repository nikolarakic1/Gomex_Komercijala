using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
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
            const string sql = """
        WITH TrenutniPeriod AS
        (
            SELECT
                ArtikalId,
                MPBezPDV,
                RUC12,
                NedostatakMargine,
                DatumUnosa
            FROM dbo.KomercijalniRezultat
            WHERE Godina = @Godina
              OR Nedelja BETWEEN @NedeljaOd AND @NedeljaDo
        ),
        PrethodniPeriod AS
        (
            SELECT
                ArtikalId,
                MPBezPDV,
                RUC12,
                NedostatakMargine
            FROM dbo.KomercijalniRezultat
            WHERE Godina = @Godina
              OR Nedelja BETWEEN @PrethodnaNedeljaOd
                              AND @PrethodnaNedeljaDo
        ),
        TrenutniPoArtiklu AS
        (
            SELECT
                ArtikalId,
                SUM(MPBezPDV) AS PrometBezPdv,
                SUM(RUC12) AS Ruc12,
                SUM(NedostatakMargine) AS NedostatakMarze
            FROM TrenutniPeriod
            GROUP BY ArtikalId
        ),
        PrethodniPoArtiklu AS
        (
            SELECT
                ArtikalId,
                SUM(MPBezPDV) AS PrometBezPdv,
                SUM(RUC12) AS Ruc12,
                SUM(NedostatakMargine) AS NedostatakMarze
            FROM PrethodniPeriod
            GROUP BY ArtikalId
        ),
        TrenutniUkupno AS
        (
            SELECT
                COALESCE(SUM(PrometBezPdv), 0) AS PrometBezPdv,
                COALESCE(SUM(Ruc12), 0) AS Ruc12,
                COALESCE(SUM(NedostatakMarze), 0) AS NedostatakMarze,

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
                COALESCE(SUM(PrometBezPdv), 0) AS PrometBezPdv,
                COALESCE(SUM(Ruc12), 0) AS Ruc12,
                COALESCE(SUM(NedostatakMarze), 0) AS NedostatakMarze,

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
                    (t.PrometBezPdv - p.PrometBezPdv)
                    / NULLIF(p.PrometBezPdv, 0)
            END AS PrometPromenaProcenat,

            t.Ruc12,

            CASE
                WHEN p.Ruc12 = 0 THEN 0
                ELSE
                    (t.Ruc12 - p.Ruc12)
                    / NULLIF(p.Ruc12, 0)
            END AS Ruc12PromenaProcenat,

            CASE
                WHEN t.PrometBezPdv = 0 THEN 0
                ELSE
                    t.Ruc12 / NULLIF(t.PrometBezPdv, 0)
            END AS Ruc12Procenat,

            (
                CASE
                    WHEN t.PrometBezPdv = 0 THEN 0
                    ELSE
                        t.Ruc12 / NULLIF(t.PrometBezPdv, 0)
                END
                -
                CASE
                    WHEN p.PrometBezPdv = 0 THEN 0
                    ELSE
                        p.Ruc12 / NULLIF(p.PrometBezPdv, 0)
                END
            ) AS Ruc12PromenaProcentniPoeni,

            t.KriticniArtikli,

            t.KriticniArtikli
            - p.KriticniArtikli AS KriticniArtikliPromena,

            t.NedostatakMarze,

            CASE
                WHEN p.NedostatakMarze = 0 THEN 0
                ELSE
                    (t.NedostatakMarze - p.NedostatakMarze)
                    / NULLIF(p.NedostatakMarze, 0)
            END AS NedostatakMarzePromenaProcenat,

            (
                SELECT MAX(DatumUnosa)
                FROM TrenutniPeriod
            ) AS PodaciOsvezeni

        FROM TrenutniUkupno t
        CROSS JOIN PrethodniUkupno p;
        """;

            int brojNedelja =
                filterDTO.NedeljaDo - filterDTO.NedeljaOd + 1;

            int prethodnaNedeljaDo =
                filterDTO.NedeljaOd - 1;

            int prethodnaNedeljaOd =
                prethodnaNedeljaDo - brojNedelja + 1;

            using var connection = _connFactory.CreateConnection();

            return await connection.QuerySingleAsync<DashboardSummaryDTO>(
                sql,
                new
                {
                    filterDTO.Godina,
                    filterDTO.NedeljaOd,
                    filterDTO.NedeljaDo,
                    PrethodnaNedeljaOd = prethodnaNedeljaOd,
                    PrethodnaNedeljaDo = prethodnaNedeljaDo
                });
        }
    }
}
