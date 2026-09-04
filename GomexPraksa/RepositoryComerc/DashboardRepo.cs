using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.DtosComerc;
using System.Diagnostics;
using System.Text;

namespace GomexPraksa.RepositoryComerc
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly IConnFactory _connFactory;

        public DashboardRepo(
            IConnFactory connFactory)
        {
            _connFactory = connFactory;
        }

        public async Task<DashboardSummaryDTO> FillCardsAsync(
            DashboardFilterDTO filterDTO,
            bool canViewAllCategories,
            List<int> kategorijaIds)
        {
            ArgumentNullException.ThrowIfNull(filterDTO);

            // =============================================
            // DATUMI
            // =============================================

            bool imaDatumOd =
                filterDTO.DatumOd.HasValue;

            bool imaDatumDo =
                filterDTO.DatumDo.HasValue;

            if (imaDatumOd != imaDatumDo)
            {
                throw new ArgumentException(
                    "Moraju biti uneti i DatumOd i DatumDo, ili nijedan."
                );
            }

            DateOnly datumOd;
            DateOnly datumDo;

            if (!imaDatumOd &&
                !imaDatumDo)
            {
                datumDo =
                    DateOnly.FromDateTime(
                        DateTime.Today
                    );

                datumOd =
                    datumDo.AddDays(-29);
            }
            else
            {
                datumOd =
                    filterDTO.DatumOd!.Value;

                datumDo =
                    filterDTO.DatumDo!.Value;
            }

            if (datumOd > datumDo)
            {
                throw new ArgumentException(
                    "DatumOd ne može biti posle DatumDo."
                );
            }

            // =============================================
            // PRETHODNI PERIOD
            //
            // Ako je trenutni period 30 dana,
            // prethodni period je prethodnih 30 dana.
            // =============================================

            int brojDana =
                datumDo.DayNumber
                -
                datumOd.DayNumber
                +
                1;

            DateOnly prethodniDatumDo =
                datumOd.AddDays(-1);

            DateOnly prethodniDatumOd =
                prethodniDatumDo.AddDays(
                    -(brojDana - 1)
                );

            // =============================================
            // DINAMICKI WHERE
            //
            // Dodajemo samo filtere koji postoje.
            // Nema:
            //
            // @X IS NULL OR Kolona = @X
            //
            // SQL Server dobija mnogo cistiji query.
            // =============================================

            var where =
                new StringBuilder();

            where.AppendLine(
                """
                WHERE
                    kr.DatumRezultata >= @PrethodniDatumOd

                    AND kr.DatumRezultata <
                        DATEADD(
                            DAY,
                            1,
                            @DatumDo
                        )

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

            parametri.Add(
                "PrethodniDatumOd",
                prethodniDatumOd.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            parametri.Add(
                "PrethodniDatumDo",
                prethodniDatumDo.ToDateTime(
                    TimeOnly.MinValue
                )
            );

            // =============================================
            // ODELJENJE
            // =============================================

            if (filterDTO.OdeljenjeId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.OdeljenjeId =
                        @OdeljenjeId
                    """
                );

                parametri.Add(
                    "OdeljenjeId",
                    filterDTO.OdeljenjeId.Value
                );
            }

            // =============================================
            // KATEGORIJA
            // =============================================

            if (filterDTO.KategorijaId.HasValue)
            {
                where.AppendLine(
                    """
                    AND k.KategorijaId =
                        @KategorijaId
                    """
                );

                parametri.Add(
                    "KategorijaId",
                    filterDTO.KategorijaId.Value
                );
            }

            // =============================================
            // DOBAVLJAC
            // =============================================

            if (filterDTO.DobavljacId.HasValue)
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
                    filterDTO.DobavljacId.Value
                );
            }

            // =============================================
            // TIP PRODAJE
            // =============================================

            if (filterDTO.TipProdajeId.HasValue)
            {
                where.AppendLine(
                    """
                    AND kr.TipProdajeId =
                        @TipProdajeId
                    """
                );

                parametri.Add(
                    "TipProdajeId",
                    filterDTO.TipProdajeId.Value
                );
            }

            // =============================================
            // MENADZEROVE KATEGORIJE
            //
            // Sef nema IN listu u SQL-u uopste.
            // =============================================

            if (!canViewAllCategories)
            {
                if (kategorijaIds is null ||
                    kategorijaIds.Count == 0)
                {
                    throw new UnauthorizedAccessException(
                        "Korisniku nije dodeljena nijedna kategorija."
                    );
                }

                where.AppendLine(
                    """
                    AND k.KategorijaId
                        IN @KategorijaIds
                    """
                );

                parametri.Add(
                    "KategorijaIds",
                    kategorijaIds
                );
            }

            // =============================================
            // SQL
            // =============================================

            string sql =
                $"""
                WITH PoArtiklu AS
                (
                    SELECT
                        kr.ArtikalId,

                        -- =================================
                        -- TRENUTNI PERIOD
                        -- =================================

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )

                                THEN kr.MPBezPDV
                                ELSE 0
                            END
                        )
                            AS TrenutniPromet,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )

                                THEN kr.RUC12
                                ELSE 0
                            END
                        )
                            AS TrenutniRuc,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )

                                THEN
                                    COALESCE(
                                        kr.MarginEffect,
                                        0
                                    )
                                    +
                                    COALESCE(
                                        kr.MixEffect,
                                        0
                                    )

                                ELSE 0
                            END
                        )
                            AS TrenutniNedostatak,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )

                                THEN 1
                                ELSE 0
                            END
                        )
                            AS TrenutniBrojRedova,

                        -- =================================
                        -- PRETHODNI PERIOD
                        -- =================================

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )

                                THEN kr.MPBezPDV
                                ELSE 0
                            END
                        )
                            AS PrethodniPromet,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )

                                THEN kr.RUC12
                                ELSE 0
                            END
                        )
                            AS PrethodniRuc,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )

                                THEN
                                    COALESCE(
                                        kr.MarginEffect,
                                        0
                                    )
                                    +
                                    COALESCE(
                                        kr.MixEffect,
                                        0
                                    )

                                ELSE 0
                            END
                        )
                            AS PrethodniNedostatak,

                        SUM(
                            CASE
                                WHEN
                                    kr.DatumRezultata >=
                                        @PrethodniDatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @PrethodniDatumDo
                                        )

                                THEN 1
                                ELSE 0
                            END
                        )
                            AS PrethodniBrojRedova,

                        -- =================================
                        -- POSLEDNJI IMPORT
                        -- samo trenutni period
                        -- =================================

                        MAX(
                            CASE
                                WHEN
                                    kr.DatumRezultata >= @DatumOd

                                    AND kr.DatumRezultata <
                                        DATEADD(
                                            DAY,
                                            1,
                                            @DatumDo
                                        )

                                THEN kr.DatumUnosa
                                ELSE NULL
                            END
                        )
                            AS PodaciOsvezeni

                    FROM
                        dbo.KomercijalniRezultat kr

                    INNER JOIN
                        dbo.Artikal a
                            ON a.ArtikalId =
                               kr.ArtikalId

                    LEFT JOIN
                        dbo.RobnaGrupa rg
                            ON rg.RobnaGrupaId =
                               a.RobnaGrupaId

                    LEFT JOIN
                        dbo.Kategorija k
                            ON k.KategorijaId =
                               rg.KategorijaId

                    {where}

                    GROUP BY
                        kr.ArtikalId
                ),

                Ukupno AS
                (
                    SELECT

                        COALESCE(
                            SUM(
                                TrenutniPromet
                            ),
                            0
                        )
                            AS TrenutniPromet,

                        COALESCE(
                            SUM(
                                TrenutniRuc
                            ),
                            0
                        )
                            AS TrenutniRuc,

                        COALESCE(
                            SUM(
                                TrenutniNedostatak
                            ),
                            0
                        )
                            AS TrenutniNedostatak,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        TrenutniBrojRedova > 0

                                        AND
                                        (
                                            TrenutniRuc <= 0

                                            OR

                                            TrenutniNedostatak < 0
                                        )

                                    THEN 1
                                    ELSE 0
                                END
                            ),
                            0
                        )
                            AS TrenutniKriticni,

                        COALESCE(
                            SUM(
                                PrethodniPromet
                            ),
                            0
                        )
                            AS PrethodniPromet,

                        COALESCE(
                            SUM(
                                PrethodniRuc
                            ),
                            0
                        )
                            AS PrethodniRuc,

                        COALESCE(
                            SUM(
                                PrethodniNedostatak
                            ),
                            0
                        )
                            AS PrethodniNedostatak,

                        COALESCE(
                            SUM(
                                CASE
                                    WHEN
                                        PrethodniBrojRedova > 0

                                        AND
                                        (
                                            PrethodniRuc <= 0

                                            OR

                                            PrethodniNedostatak < 0
                                        )

                                    THEN 1
                                    ELSE 0
                                END
                            ),
                            0
                        )
                            AS PrethodniKriticni,

                        MAX(
                            PodaciOsvezeni
                        )
                            AS PodaciOsvezeni

                    FROM
                        PoArtiklu
                )

                SELECT

                    -- =====================================
                    -- PROMET
                    -- =====================================

                    TrenutniPromet
                        AS PrometBezPdv,

                    CAST(
                        CASE
                            WHEN PrethodniPromet = 0
                                THEN 0

                            ELSE
                                CAST(
                                    TrenutniPromet
                                    -
                                    PrethodniPromet
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    PrethodniPromet,
                                    0
                                )
                        END

                        AS DECIMAL(18, 4)
                    )
                        AS PrometPromenaProcenat,

                    -- =====================================
                    -- RUC
                    -- =====================================

                    TrenutniRuc
                        AS Ruc12,

                    CAST(
                        CASE
                            WHEN PrethodniRuc = 0
                                THEN 0

                            ELSE
                                CAST(
                                    TrenutniRuc
                                    -
                                    PrethodniRuc
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    PrethodniRuc,
                                    0
                                )
                        END

                        AS DECIMAL(18, 4)
                    )
                        AS Ruc12PromenaProcenat,

                    -- =====================================
                    -- RUC %
                    -- =====================================

                    CAST(
                        CASE
                            WHEN TrenutniPromet = 0
                                THEN 0

                            ELSE
                                CAST(
                                    TrenutniRuc
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    TrenutniPromet,
                                    0
                                )
                        END

                        AS DECIMAL(18, 4)
                    )
                        AS Ruc12Procenat,

                    -- =====================================
                    -- PROMENA RUC % U PROCENTNIM POENIMA
                    -- =====================================

                    CAST(
                        (
                            CASE
                                WHEN TrenutniPromet = 0
                                    THEN 0

                                ELSE
                                    CAST(
                                        TrenutniRuc
                                        AS DECIMAL(28, 6)
                                    )
                                    /
                                    NULLIF(
                                        TrenutniPromet,
                                        0
                                    )
                            END
                        )
                        -
                        (
                            CASE
                                WHEN PrethodniPromet = 0
                                    THEN 0

                                ELSE
                                    CAST(
                                        PrethodniRuc
                                        AS DECIMAL(28, 6)
                                    )
                                    /
                                    NULLIF(
                                        PrethodniPromet,
                                        0
                                    )
                            END
                        )

                        AS DECIMAL(18, 4)
                    )
                        AS Ruc12PromenaProcentniPoeni,

                    -- =====================================
                    -- KRITICNI ARTIKLI
                    -- =====================================

                    TrenutniKriticni
                        AS KriticniArtikli,

                    TrenutniKriticni
                    -
                    PrethodniKriticni
                        AS KriticniArtikliPromena,

                    -- =====================================
                    -- NEDOSTATAK MARZE
                    -- =====================================

                    TrenutniNedostatak
                        AS NedostatakMarze,

                    CAST(
                        CASE
                            WHEN PrethodniNedostatak = 0
                                THEN 0

                            ELSE
                                CAST(
                                    TrenutniNedostatak
                                    -
                                    PrethodniNedostatak
                                    AS DECIMAL(28, 6)
                                )
                                /
                                NULLIF(
                                    ABS(
                                        PrethodniNedostatak
                                    ),
                                    0
                                )
                        END

                        AS DECIMAL(18, 4)
                    )
                        AS NedostatakMarzePromenaProcenat,

                    -- =====================================
                    -- POSLEDNJE OSVEZAVANJE
                    -- =====================================

                    PodaciOsvezeni

                FROM
                    Ukupno

                OPTION (RECOMPILE);
                """;

            // =============================================
            // IZVRSAVANJE
            // =============================================

            using var connection =
                _connFactory.CreateConnection();

            var sw =
                Stopwatch.StartNew();

            var rezultat =
                await connection
                    .QuerySingleAsync<DashboardSummaryDTO>(
                        sql,
                        parametri
                    );

            sw.Stop();

            Console.WriteLine(
                $"DASHBOARD SQL: " +
                $"{sw.ElapsedMilliseconds} ms"
            );

            return rezultat;
        }
    }
}