using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public class CriticalProductsRepo : ICriticalProducts
    {
        private readonly IConnFactory _connection;
        public CriticalProductsRepo(IConnFactory connection)
        {
            _connection = connection;
        }
        public async Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop5(
    DateOnly datumOd,
    DateOnly datumDo)
        {
            const string sql = """
        WITH Kriticni AS
        (
            SELECT
                kr.ArtikalId,
                SUM(kr.RUC12) AS Ruc12,
                SUM(kr.NedostatakMargine) AS NedostatakMargine
            FROM dbo.KomercijalniRezultat kr
            WHERE kr.DatumRezultata >= @DatumOd
              AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)
            GROUP BY kr.ArtikalId
            HAVING
                SUM(kr.RUC12) <= 0
                OR SUM(kr.NedostatakMargine) > 0
        )

        SELECT TOP 5
            a.ArtikalId,
            a.Naziv AS NazivArtikla,
            k.Naziv AS Kategorija,

            CASE
                WHEN ABS(kr.Ruc12) >= 5000
                     OR kr.NedostatakMargine >= 5000
                    THEN 'Visok'

                WHEN ABS(kr.Ruc12) >= 2000
                     OR kr.NedostatakMargine >= 2000
                    THEN 'Srednji'

                ELSE 'Nizak'
            END AS Severnost,

            (
                CASE
                    WHEN kr.Ruc12 < 0
                        THEN kr.Ruc12
                    ELSE 0
                END
                - kr.NedostatakMargine
            ) AS ProcenjeniUticaj

        FROM Kriticni kr

        INNER JOIN dbo.Artikal a
            ON a.ArtikalId = kr.ArtikalId

        LEFT JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId

        LEFT JOIN dbo.Kategorija k
            ON k.KategorijaId = rg.KategorijaId

        ORDER BY ProcenjeniUticaj ASC;
        """;

            using var connection = _connection.CreateConnection();

            return await connection.QueryAsync<CriticalProductsDTO>(
                sql,
                new
                {
                    DatumOd = datumOd.ToDateTime(TimeOnly.MinValue),
                    DatumDo = datumDo.ToDateTime(TimeOnly.MinValue)
                }
            );
        }

        public async Task<IEnumerable<CriticalProductsPageDTO>> ShowCriticalProductsAsync(FilterSharedPages filter)
        {
            const string sql = """
                                SELECT
                    ar.ArtikalId,
                    ar.Sifra,
                    ar.Naziv,
                    d.Naziv AS Dobavljac,

                    SUM(kr.MPBezPDV) AS Promet,
                    SUM(kr.RUC12) AS RUC12,

                    CASE
                        WHEN SUM(kr.MPBezPDV) = 0 THEN 0
                        ELSE SUM(kr.RUC12) / SUM(kr.MPBezPDV)
                    END AS RUC12Procenat,

                    SUM(kr.NedostatakMargine) AS NedostatakMargine

                FROM dbo.KomercijalniRezultat kr

                INNER JOIN dbo.Artikal ar
                    ON kr.ArtikalId = ar.ArtikalId

                LEFT JOIN dbo.Dobavljac d
                    ON ar.DobavljacId = d.DobavljacId

                LEFT JOIN dbo.RobnaGrupa rg
                    ON ar.RobnaGrupaId = rg.RobnaGrupaId

                LEFT JOIN dbo.Kategorija k
                    ON rg.KategorijaId = k.KategorijaId

                WHERE kr.DatumRezultata >= @DatumOd
                  AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)

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
                      OR ar.DobavljacId = @DobavljacId
                  )

                  AND (
                      @TipProdajeId IS NULL
                      OR kr.TipProdajeId = @TipProdajeId
                  )

                GROUP BY
                    ar.ArtikalId,
                    ar.Sifra,
                    ar.Naziv,
                    d.Naziv

                ORDER BY
                    SUM(kr.NedostatakMargine) DESC;
                """;
            using var connection = _connection.CreateConnection();
            return await connection.QueryAsync<CriticalProductsPageDTO>(
       sql,
       new
       {
           DatumOd = filter.DatumOd.ToDateTime(TimeOnly.MinValue),
           DatumDo = filter.DatumDo.ToDateTime(TimeOnly.MinValue),

           filter.OdeljenjeId,
           filter.KategorijaId,
           filter.DobavljacId,
           filter.TipProdajeId
       });

        }
        
    }
}
