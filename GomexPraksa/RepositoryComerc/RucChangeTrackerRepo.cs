using Dapper;
using GomexPraksa.ConnectionFactory;
using Microsoft.Data.SqlClient;
using Models.DtosComerc;
using System.Diagnostics;

namespace GomexPraksa.RepositoryComerc;

public class RucChangeTrackerRepo : IRucChangeTracker
{
    private readonly IConnFactory _connFactory;

    public RucChangeTrackerRepo(
        IConnFactory connFactory)
    {
        _connFactory = connFactory;
    }

    public async Task<RucChangeDTO> CheckInfoForChangesAsync(
        DashboardFilterDTO filter,
        DateOnly prethodniDatumOd,
        DateOnly prethodniDatumDo,
        bool canViewAllCategories,
        List<int> kategorijaIds)
    {
        if (!filter.DatumOd.HasValue)
        {
            throw new ArgumentException(
                "Datum od je obavezan."
            );
        }

        if (!filter.DatumDo.HasValue)
        {
            throw new ArgumentException(
                "Datum do je obavezan."
            );
        }

        var datumOd =
            filter.DatumOd.Value;

        var datumDo =
            filter.DatumDo.Value;

        const string sql = """
        WITH Podaci AS
        (
            SELECT
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
                            THEN kr.RUC12
                        ELSE 0
                    END
                ) AS ActualRuc,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 7
                            THEN kr.Kolicina
                        ELSE 0
                    END
                ) AS PlanKolicina,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 6
                            THEN kr.Kolicina
                        ELSE 0
                    END
                ) AS ActualKolicina

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

            WHERE
                kr.DatumRezultata >=
                    @DatumOd

                AND kr.DatumRezultata <
                    DATEADD(
                        DAY,
                        1,
                        @DatumDo
                    )

                AND kr.TipProdajeId
                    IN (6, 7)

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
                    @CanViewAllCategories = 1
                    OR k.KategorijaId
                       IN @KategorijaIds
                )
        ),

        Racun AS
        (
            SELECT
                COALESCE(
                    PlanRuc,
                    0
                ) AS PlanRuc,

                COALESCE(
                    ActualRuc,
                    0
                ) AS ActualRuc,

                COALESCE(
                    PlanKolicina,
                    0
                ) AS PlanKolicina,

                COALESCE(
                    ActualKolicina,
                    0
                ) AS ActualKolicina,

                CASE
                    WHEN COALESCE(
                        PlanKolicina,
                        0
                    ) = 0
                        THEN 0

                    ELSE
                        COALESCE(
                            PlanRuc,
                            0
                        )
                        /
                        NULLIF(
                            PlanKolicina,
                            0
                        )
                END
                    AS PlanRucPoJedinici

            FROM Podaci
        ),

        Efekti AS
        (
            SELECT
                PlanRuc,
                ActualRuc,

                (
                    ActualKolicina
                    -
                    PlanKolicina
                )
                *
                PlanRucPoJedinici
                    AS VolumeEffect,

                CAST(
                    0
                    AS DECIMAL(18, 6)
                )
                    AS MixEffect

            FROM Racun
        )

        SELECT
            PlanRuc
                AS PocetniRuc,

            (
                ActualRuc
                -
                PlanRuc
                -
                VolumeEffect
            )
                AS MarginEffect,

            VolumeEffect,

            MixEffect,

            ActualRuc
            -
            PlanRuc
                AS UkupnaPromena,

            CASE
                WHEN PlanRuc = 0
                    THEN 0

                ELSE
                    (
                        ActualRuc
                        -
                        PlanRuc
                    )
                    /
                    NULLIF(
                        PlanRuc,
                        0
                    )
            END
                AS UkupnaPromenaProcenat,

            ActualRuc
                AS KonacniRuc,

            CAST(
                0
                AS DECIMAL(18, 6)
            )
                AS KontrolnaRazlika

        FROM Efekti;
        """;

        using var connection =
            (SqlConnection)
            _connFactory.CreateConnection();

        var total =
            Stopwatch.StartNew();

        await connection.OpenAsync();

        var sw =
            Stopwatch.StartNew();

        var rezultat =
            await connection
                .QuerySingleAsync<RucChangeDTO>(
                    sql,
                    new
                    {
                        DatumOd =
                            datumOd.ToDateTime(
                                TimeOnly.MinValue
                            ),

                        DatumDo =
                            datumDo.ToDateTime(
                                TimeOnly.MinValue
                            ),

                        filter.OdeljenjeId,

                        filter.KategorijaId,

                        filter.DobavljacId,

                        CanViewAllCategories =
                            canViewAllCategories,

                        KategorijaIds =
                            kategorijaIds
                    }
                );

        Console.WriteLine(
            $"RUC Query: " +
            $"{sw.ElapsedMilliseconds} ms"
        );

        Console.WriteLine(
            $"RUC TOTAL: " +
            $"{total.ElapsedMilliseconds} ms"
        );

        return rezultat;
    }
}