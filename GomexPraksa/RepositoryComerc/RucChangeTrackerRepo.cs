using Dapper;
using GomexPraksa.ConnectionFactory;
using Microsoft.Data.SqlClient;
using Models.DtosComerc;
using System.Diagnostics;

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
            COALESCE(SUM(kr.RUC12), 0) AS PocetniRuc
        FROM dbo.KomercijalniRezultat kr
        INNER JOIN dbo.Artikal a
            ON a.ArtikalId = kr.ArtikalId
        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId
        WHERE
            kr.DatumRezultata >= @PrethodniDatumOd
            AND kr.DatumRezultata < DATEADD(DAY, 1, @PrethodniDatumDo)
            AND
            (
                @CanViewAllCategories = 1
                OR rg.KategorijaId IN @KategorijaIds
            )
    ),
    TrenutniPeriod AS
    (
        SELECT
            COALESCE(SUM(kr.RUC12), 0) AS KonacniRuc,
            COALESCE(SUM(kr.MarginEffect), 0) AS MarginEffect,
            COALESCE(SUM(kr.VolumeEffect), 0) AS VolumeEffect,
            COALESCE(SUM(kr.MixEffect), 0) AS MixEffect
        FROM dbo.KomercijalniRezultat kr
        INNER JOIN dbo.Artikal a
            ON a.ArtikalId = kr.ArtikalId
        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId
        WHERE
            kr.DatumRezultata >= @DatumOd
            AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
            AND
            (
                @CanViewAllCategories = 1
                OR rg.KategorijaId IN @KategorijaIds
            )
    )
    SELECT
        p.PocetniRuc,
        t.MarginEffect,
        t.VolumeEffect,
        t.MixEffect,

        t.KonacniRuc - p.PocetniRuc
            AS UkupnaPromena,

        CASE
            WHEN p.PocetniRuc = 0 THEN 0
            ELSE
                CAST(
                    t.KonacniRuc - p.PocetniRuc
                    AS DECIMAL(18, 6)
                )
                / NULLIF(p.PocetniRuc, 0)
        END AS UkupnaPromenaProcenat,

        t.KonacniRuc,

        (
            t.KonacniRuc - p.PocetniRuc
        )
        -
        (
            t.MarginEffect
            + t.VolumeEffect
            + t.MixEffect
        ) AS KontrolnaRazlika

    FROM PrethodniPeriod p
    CROSS JOIN TrenutniPeriod t;
    """; 

        using var connection =
            (SqlConnection)_connFactory.CreateConnection();

        var total = Stopwatch.StartNew();

        var sw = Stopwatch.StartNew();

        await connection.OpenAsync();

        Console.WriteLine(
            $"RUC OpenConnectionAsync: {sw.ElapsedMilliseconds} ms"
        );

        sw.Restart();

        var rezultat =
            await connection.QuerySingleAsync<RucChangeDTO>(
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

        Console.WriteLine(
            $"RUC QuerySingleAsync: {sw.ElapsedMilliseconds} ms"
        );

        Console.WriteLine(
            $"RUC TOTAL: {total.ElapsedMilliseconds} ms"
        );

        return rezultat;
    }
}