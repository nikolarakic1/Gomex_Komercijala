using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc;

public class RucChangeTrackerRepo : IRucChangeTracker
{
    private readonly IConnFactory _connFactory;

    public RucChangeTrackerRepo(IConnFactory connFactory)
    {
        _connFactory = connFactory;
    }

    public async Task<RucChangeDTO> CheckInfoForChangesAsync(
        DateOnly datumOd,
        DateOnly? datumDo,
        DateOnly? prethodniDatumOd,
        DateOnly? prethodniDatumDo,
        bool canViewAllCategories,
        List<int> kategorijaIds)
    {
        const string sql = """
            WITH PrethodniPeriod AS
            (
                SELECT
                    kr.RUC12
                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = kr.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                WHERE
                    kr.DatumRezultata >= @PrethodniDatumOd
                    AND kr.DatumRezultata
                        < DATEADD(DAY, 1, @PrethodniDatumDo)

                    AND
                    (
                        @CanViewAllCategories = 1
                        OR rg.KategorijaId IN @KategorijaIds
                    )
            ),

            TrenutniPeriod AS
            (
                SELECT
                    kr.RUC12,
                    kr.MarginEffect,
                    kr.VolumeEffect,
                    kr.MixEffect

                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = kr.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                WHERE
                    kr.DatumRezultata >= @DatumOd
                    AND kr.DatumRezultata
                        < DATEADD(DAY, 1, @DatumDo)

                    AND
                    (
                        @CanViewAllCategories = 1
                        OR rg.KategorijaId IN @KategorijaIds
                    )
            ),

            Rezultat AS
            (
                SELECT
                    COALESCE(
                        (
                            SELECT SUM(RUC12)
                            FROM PrethodniPeriod
                        ),
                        0
                    ) AS PocetniRuc,

                    COALESCE(
                        (
                            SELECT SUM(RUC12)
                            FROM TrenutniPeriod
                        ),
                        0
                    ) AS KonacniRuc,

                    COALESCE(
                        (
                            SELECT SUM(MarginEffect)
                            FROM TrenutniPeriod
                        ),
                        0
                    ) AS MarginEffect,

                    COALESCE(
                        (
                            SELECT SUM(VolumeEffect)
                            FROM TrenutniPeriod
                        ),
                        0
                    ) AS VolumeEffect,

                    COALESCE(
                        (
                            SELECT SUM(MixEffect)
                            FROM TrenutniPeriod
                        ),
                        0
                    ) AS MixEffect
            )

            SELECT
                PocetniRuc,
                MarginEffect,
                VolumeEffect,
                MixEffect,

                KonacniRuc - PocetniRuc
                    AS UkupnaPromena,

                CASE
                    WHEN PocetniRuc = 0
                        THEN 0
                    ELSE
                        CAST(
                            KonacniRuc - PocetniRuc
                            AS DECIMAL(18, 6)
                        )
                        / NULLIF(PocetniRuc, 0)
                END AS UkupnaPromenaProcenat,

                KonacniRuc,

                (
                    KonacniRuc - PocetniRuc
                )
                -
                (
                    MarginEffect
                    + VolumeEffect
                    + MixEffect
                ) AS KontrolnaRazlika

            FROM Rezultat;
            """;

        using var connection =
            _connFactory.CreateConnection();

        return await connection.QuerySingleAsync<RucChangeDTO>(
            sql,
            new
            {
                DatumOd =
                    datumOd.ToDateTime(
                        TimeOnly.MinValue),

                DatumDo =
                    datumDo?.ToDateTime(
                        TimeOnly.MinValue),

                PrethodniDatumOd =
                    prethodniDatumOd?.ToDateTime(
                        TimeOnly.MinValue),

                PrethodniDatumDo =
                    prethodniDatumDo?.ToDateTime(
                        TimeOnly.MinValue),

                CanViewAllCategories =
                    canViewAllCategories,

                KategorijaIds =
                    kategorijaIds
            }
        );
    }
}