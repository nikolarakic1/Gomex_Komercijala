using Dapper;
using GomexPraksa.ConnectionFactory;
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
        SELECT TOP 5
            a.ArtikalId,
            a.Naziv AS NazivArtikla,
            k.Naziv AS Kategorija,

            CASE
                WHEN ABS(SUM(kr.RUC12)) >= 5000
                     OR SUM(kr.NedostatakMargine) >= 5000
                    THEN 'Visok'

                WHEN ABS(SUM(kr.RUC12)) >= 2000
                     OR SUM(kr.NedostatakMargine) >= 2000
                    THEN 'Srednji'

                ELSE 'Nizak'
            END AS Severnost,

            (
                CASE
                    WHEN SUM(kr.RUC12) < 0
                        THEN SUM(kr.RUC12)
                    ELSE 0
                END
                -
                SUM(kr.NedostatakMargine)
            ) AS ProcenjeniUticaj

        FROM dbo.KomercijalniRezultat kr

        INNER JOIN dbo.Artikal a
            ON a.ArtikalId = kr.ArtikalId

        LEFT JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId

        LEFT JOIN dbo.Kategorija k
            ON k.KategorijaId = rg.KategorijaId

        WHERE kr.DatumRezultata >= @DatumOd
          AND kr.DatumRezultata < DATEADD(DAY, 1, @DatumDo)

        GROUP BY
            a.ArtikalId,
            a.Naziv,
            k.Naziv

        HAVING
            SUM(kr.RUC12) <= 0
            OR SUM(kr.NedostatakMargine) > 0

        ORDER BY
            ProcenjeniUticaj ASC;
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

        public Task<IEnumerable<CriticalProductsDTO>> ShowCriticalProductsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
