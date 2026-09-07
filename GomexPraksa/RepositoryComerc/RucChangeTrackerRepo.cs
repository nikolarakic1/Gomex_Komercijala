using Dapper;
using GomexPraksa.ConnectionFactory;
using Microsoft.Data.SqlClient;
using Models.DtosComerc;
using System.Diagnostics;
using System.Text;

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

        var datumOd = filter.DatumOd.Value;
        var datumDo = filter.DatumDo.Value;

        if (datumOd > datumDo)
        {
            throw new ArgumentException(
                "Datum početka ne može biti posle datuma završetka."
            );
        }

        if (datumDo >
            DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException(
                "Datum završetka ne može biti u budućnosti."
            );
        }

        if (!canViewAllCategories &&
            (kategorijaIds == null ||
             kategorijaIds.Count == 0))
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija."
            );
        }

        var where = new StringBuilder();

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

        if (filter.OdeljenjeId.HasValue)
        {
            where.AppendLine(
                "AND k.OdeljenjeId = @OdeljenjeId"
            );

            parametri.Add(
                "OdeljenjeId",
                filter.OdeljenjeId.Value
            );
        }

        if (filter.KategorijaId.HasValue)
        {
            where.AppendLine(
                "AND k.KategorijaId = @KategorijaId"
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
                "AND k.KategorijaId IN @KategorijaIds"
            );

            parametri.Add(
                "KategorijaIds",
                kategorijaIds
            );
        }

        string sql =
            $"""
            WITH Podaci AS
            (
                SELECT

                    COALESCE(
                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN kr.MPBezPDV
                                ELSE 0
                            END
                        ),
                        0
                    ) AS ActualPromet,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN kr.MPBezPDV
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PlanPromet,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 6
                                    THEN kr.RUC12
                                ELSE 0
                            END
                        ),
                        0
                    ) AS ActualRuc,

                    COALESCE(
                        SUM(
                            CASE
                                WHEN kr.TipProdajeId = 7
                                    THEN kr.RUC12
                                ELSE 0
                            END
                        ),
                        0
                    ) AS PlanRuc

                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal a
                    ON a.ArtikalId = kr.ArtikalId

                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId

                INNER JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId

                {where}
            ),

            Procenti AS
            (
                SELECT

                    CAST(
                        ActualPromet
                        AS DECIMAL(38, 10)
                    ) AS ActualPromet,

                    CAST(
                        PlanPromet
                        AS DECIMAL(38, 10)
                    ) AS PlanPromet,

                    CAST(
                        ActualRuc
                        AS DECIMAL(38, 10)
                    ) AS ActualRuc,

                    CAST(
                        PlanRuc
                        AS DECIMAL(38, 10)
                    ) AS PlanRuc,

                    CASE
                        WHEN ActualPromet = 0
                            THEN CAST(0 AS DECIMAL(38, 10))

                        ELSE
                            CAST(
                                ActualRuc
                                AS DECIMAL(38, 10)
                            )
                            /
                            NULLIF(
                                CAST(
                                    ActualPromet
                                    AS DECIMAL(38, 10)
                                ),
                                0
                            )
                    END
                    AS ActualRucProcenat,

                    CASE
                        WHEN PlanPromet = 0
                            THEN CAST(0 AS DECIMAL(38, 10))

                        ELSE
                            CAST(
                                PlanRuc
                                AS DECIMAL(38, 10)
                            )
                            /
                            NULLIF(
                                CAST(
                                    PlanPromet
                                    AS DECIMAL(38, 10)
                                ),
                                0
                            )
                    END
                    AS PlanRucProcenat

                FROM Podaci
            ),

            Efekti AS
            (
                SELECT

                    ActualPromet,
                    PlanPromet,
                    ActualRuc,
                    PlanRuc,

                    (
                        ActualRucProcenat
                        -
                        PlanRucProcenat
                    )
                    *
                    PlanPromet
                    AS MarginEffect,

                    (
                        ActualPromet
                        -
                        PlanPromet
                    )
                    *
                    PlanRucProcenat
                    AS VolumeEffect

                FROM Procenti
            ),

            FinalniPodaci AS
            (
                SELECT

                    ActualPromet,
                    PlanPromet,
                    ActualRuc,
                    PlanRuc,
                    MarginEffect,
                    VolumeEffect,

                    ActualRuc
                    -
                    PlanRuc
                    -
                    MarginEffect
                    -
                    VolumeEffect
                    AS MixEffect

                FROM Efekti
            )

            SELECT

                CAST(
                    PlanRuc
                    AS DECIMAL(28, 6)
                ) AS PocetniRuc,

                CAST(
                    MarginEffect
                    AS DECIMAL(28, 6)
                ) AS MarginEffect,

                CAST(
                    VolumeEffect
                    AS DECIMAL(28, 6)
                ) AS VolumeEffect,

                CAST(
                    MixEffect
                    AS DECIMAL(28, 6)
                ) AS MixEffect,

                CAST(
                    ActualRuc - PlanRuc
                    AS DECIMAL(28, 6)
                ) AS UkupnaPromena,

                CASE
                    WHEN PlanRuc = 0
                        THEN CAST(0 AS DECIMAL(28, 10))

                    ELSE
                        CAST(
                            ActualRuc - PlanRuc
                            AS DECIMAL(28, 10)
                        )
                        /
                        NULLIF(
                            CAST(
                                PlanRuc
                                AS DECIMAL(28, 10)
                            ),
                            0
                        )
                END
                AS UkupnaPromenaProcenat,

                CAST(
                    ActualRuc
                    AS DECIMAL(28, 6)
                ) AS KonacniRuc,

                CAST(
                    ActualRuc
                    -
                    (
                        PlanRuc
                        +
                        MarginEffect
                        +
                        VolumeEffect
                        +
                        MixEffect
                    )
                    AS DECIMAL(28, 6)
                ) AS KontrolnaRazlika

            FROM FinalniPodaci

            OPTION (RECOMPILE);
            """;

        using var connection =
            (SqlConnection)
            _connFactory.CreateConnection();

        var total =
            Stopwatch.StartNew();

        await connection.OpenAsync();

        var queryTimer =
            Stopwatch.StartNew();

        var rezultat =
            await connection
                .QuerySingleAsync<RucChangeDTO>(
                    sql,
                    parametri
                );

        queryTimer.Stop();
        total.Stop();

        Console.WriteLine(
            $"RUC Query: {queryTimer.ElapsedMilliseconds} ms"
        );

        Console.WriteLine(
            $"RUC TOTAL: {total.ElapsedMilliseconds} ms"
        );

        Console.WriteLine(
            $"RUC WATERFALL | " +
            $"PLAN: {rezultat.PocetniRuc:N2} | " +
            $"MARGIN: {rezultat.MarginEffect:N2} | " +
            $"VOLUME: {rezultat.VolumeEffect:N2} | " +
            $"MIX: {rezultat.MixEffect:N2} | " +
            $"ACTUAL: {rezultat.KonacniRuc:N2} | " 
           
        );

        return rezultat;
    }
}