using Dapper;
using GomexPraksa.AddedFunctions;
using GomexPraksa.ConnectionFactory;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public class CriticalArticleDetailsRepo
        : ICriticalArticleDetailsRepo
    {
        private readonly IConnFactory _connection;

        public CriticalArticleDetailsRepo(
            IConnFactory connection)
        {
            _connection = connection;
        }

        public async Task<CriticalArticleDetailsDTO?>
            GetDetailsAsync(
                string sifra,
                int godinaOd,
                int godinaDo)
        {
            if (string.IsNullOrWhiteSpace(sifra))
            {
                throw new ArgumentException(
                    "Šifra artikla je obavezna."
                );
            }

            if (godinaOd > godinaDo)
            {
                throw new ArgumentException(
                    "Početna godina ne može biti veća od završne."
                );
            }

            using var connection =
                _connection.CreateConnection();

            connection.Open();

            const string infoSql = """
                SELECT TOP (1)
                    a.ArtikalId,
                    a.Sifra,
                    a.Naziv,
                    d.Naziv AS Dobavljac,
                    a.RedovnaCena
                FROM dbo.Artikal a
                LEFT JOIN dbo.Dobavljac d
                    ON d.DobavljacId = a.DobavljacId
                WHERE a.Sifra = @Sifra;
                """;

            var info =
                await connection
                    .QuerySingleOrDefaultAsync<
                        CriticalArticleInfoRow
                    >(
                        infoSql,
                        new
                        {
                            Sifra = sifra.Trim()
                        }
                    );

            if (info == null)
            {
                return null;
            }

            const string performanceSql = """
                DECLARE @ImportBatchId INT =
                (
                    SELECT MAX(s.ImportBatchId)
                    FROM dbo.KomercijalaImportStaging s
                    WHERE
                        s.Sifra = @Sifra
                        AND s.Godina BETWEEN
                            @GodinaOd AND @GodinaDo
                );

                WITH Dedup AS
                (
                    SELECT
                        s.Id,
                        s.ImportBatchId,
                        s.Tip,
                        s.Godina,
                        s.Mesec,
                        s.Nedelja,
                        s.IdKampanja1,
                        s.IdKampanja2,
                        s.CmUtice,
                        s.Sifra,
                        s.Kolicina,
                        s.MPBezPDV,
                        s.RUC12,

                        ROW_NUMBER() OVER
                        (
                            PARTITION BY
                                s.ImportBatchId,
                                s.Tip,
                                s.Godina,
                                s.Mesec,
                                s.Nedelja,
                                s.IdKampanja1,
                                s.IdKampanja2,
                                s.CmUtice,
                                s.Sifra,
                                s.Kolicina,
                                s.MPBezPDV,
                                s.RUC12
                            ORDER BY
                                s.Id
                        ) AS rn

                    FROM dbo.KomercijalaImportStaging s

                    WHERE
                        s.ImportBatchId = @ImportBatchId
                        AND s.Sifra = @Sifra
                        AND s.Godina BETWEEN
                            @GodinaOd AND @GodinaDo
                ),

                Cisti AS
                (
                    SELECT
                        Tip,
                        Godina,
                        Mesec,
                        Nedelja,
                        IdKampanja1,
                        IdKampanja2,
                        CmUtice,
                        Kolicina,
                        MPBezPDV,
                        RUC12
                    FROM Dedup
                    WHERE rn = 1
                )

                SELECT
                    Godina,

                    Tip,

                    COALESCE(
                        NULLIF(
                            LTRIM(
                                RTRIM(IdKampanja1)
                            ),
                            ''
                        ),
                        'NEPOZNATO'
                    ) AS VrstaProdaje,

                    COALESCE(
                        NULLIF(
                            LTRIM(
                                RTRIM(IdKampanja2)
                            ),
                            ''
                        ),
                        'NEPOZNATO'
                    ) AS TipAkcije,

                    SUM(
                        COALESCE(Kolicina, 0)
                    ) AS Kolicina,

                    SUM(
                        COALESCE(MPBezPDV, 0)
                    ) AS Promet,

                    SUM(
                        COALESCE(RUC12, 0)
                    ) AS Ruc12,

                    CAST(
                        CASE
                            WHEN
                                SUM(
                                    COALESCE(
                                        MPBezPDV,
                                        0
                                    )
                                ) = 0
                                THEN 0

                            ELSE
                                SUM(
                                    COALESCE(
                                        RUC12,
                                        0
                                    )
                                )
                                /
                                NULLIF(
                                    SUM(
                                        COALESCE(
                                            MPBezPDV,
                                            0
                                        )
                                    ),
                                    0
                                )
                        END
                        AS DECIMAL(18, 6)
                    ) AS Ruc12Procenat,

                    CAST(
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN
                                        UPPER(
                                            LTRIM(
                                                RTRIM(
                                                    COALESCE(
                                                        CmUtice,
                                                        ''
                                                    )
                                                )
                                            )
                                        ) = 'DA'
                                        THEN 1
                                    ELSE 0
                                END
                            ) = 1
                            THEN 1
                            ELSE 0
                        END
                        AS BIT
                    ) AS CmUtice

                FROM Cisti

                GROUP BY
                    Godina,
                    Tip,
                    IdKampanja1,
                    IdKampanja2

                ORDER BY
                    Godina,
                    Tip,
                    CASE
                        WHEN IdKampanja1 = 'AKCIJSKA'
                            THEN 0
                        WHEN IdKampanja1 = 'REDOVNA'
                            THEN 1
                        ELSE 2
                    END,
                    IdKampanja2;
                """;

            var performanse =
                (
                    await connection
                        .QueryAsync<
                            CriticalArticlePerformanceDTO
                        >(
                            performanceSql,
                            new
                            {
                                Sifra = sifra.Trim(),
                                GodinaOd = godinaOd,
                                GodinaDo = godinaDo
                            }
                        )
                )
                .ToList();

            const string akcijaSql = """
                SELECT TOP (1)
                    ak.AkcijskaCena,
                    ak.DatumOd,
                    ak.DatumDo,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija ak
                LEFT JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId =
                       ak.TipAkcijeId
                WHERE
                    ak.ArtikalId = @ArtikalId
                    AND CAST(GETDATE() AS DATE)
                        BETWEEN
                            ak.DatumOd
                            AND ak.DatumDo
                ORDER BY
                    ak.DatumDo DESC;
                """;

            var aktivnaAkcija =
                await connection
                    .QuerySingleOrDefaultAsync<
                        ActiveActionRow
                    >(
                        akcijaSql,
                        new
                        {
                            info.ArtikalId
                        }
                    );

            var model =
                new CriticalArticleDetailsDTO
                {
                    ArtikalId =
                        info.ArtikalId,

                    Sifra =
                        info.Sifra,

                    Naziv =
                        info.Naziv,

                    Dobavljac =
                        info.Dobavljac,

                    RedovnaCena =
                        info.RedovnaCena,

                    Performanse =
                        performanse
                };

            if (aktivnaAkcija != null)
            {
                model.TrenutniStatus =
                    "AKCIJSKA";

                model.TrenutniTipAkcije =
                    aktivnaAkcija.TipAkcije;

                model.AkcijskaCena =
                    aktivnaAkcija.AkcijskaCena;
            }
            else
            {
                model.TrenutniStatus =
                    "REDOVNA";

                model.TrenutniTipAkcije =
                    null;

                model.AkcijskaCena =
                    null;
            }

            var godinaZaEfekte =
                godinaDo;

            var actual =
                performanse
                    .Where(
                        x =>
                            x.Godina ==
                                godinaZaEfekte
                            &&
                            x.Tip.Equals(
                                "ACTUAL",
                                StringComparison
                                    .OrdinalIgnoreCase
                            )
                    )
                    .ToList();

            var plan =
                performanse
                    .Where(
                        x =>
                            x.Godina ==
                                godinaZaEfekte
                            &&
                            x.Tip.Equals(
                                "PLAN",
                                StringComparison
                                    .OrdinalIgnoreCase
                            )
                    )
                    .ToList();

            var actualPromet =
                actual.Sum(x => x.Promet);

            var actualRuc =
                actual.Sum(x => x.Ruc12);

            var planPromet =
                plan.Sum(x => x.Promet);

            var planRuc =
                plan.Sum(x => x.Ruc12);

            var actualRucPct =
                actualPromet == 0
                    ? 0
                    : actualRuc /
                      actualPromet;

            var planRucPct =
                planPromet == 0
                    ? 0
                    : planRuc /
                      planPromet;

            var marginEffect =
                (
                    actualRucPct
                    -
                    planRucPct
                )
                *
                planPromet;

            var volumeEffect =
                (
                    actualPromet
                    -
                    planPromet
                )
                *
                planRucPct;

            var totalDiff =
                actualRuc
                -
                planRuc;

            var mixEffect =
                totalDiff
                -
                marginEffect
                -
                volumeEffect;

            model.MarginEffect =
                marginEffect;

            model.VolumeEffect =
                volumeEffect;

            model.MixEffect =
                mixEffect;

            model.TotalDiff =
                totalDiff;

            return model;
        }

        private class CriticalArticleInfoRow
        {
            public int ArtikalId { get; set; }

            public string Sifra { get; set; } =
                string.Empty;

            public string Naziv { get; set; } =
                string.Empty;

            public string? Dobavljac { get; set; }

            public decimal? RedovnaCena { get; set; }
        }

        private class ActiveActionRow
        {
            public decimal? AkcijskaCena { get; set; }

            public DateOnly DatumOd { get; set; }

            public DateOnly DatumDo { get; set; }

            public string? TipAkcije { get; set; }
        }
    }
}