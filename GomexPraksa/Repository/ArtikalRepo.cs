using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.ModelsDash;

namespace GomexPraksa.Repository
{
    public class ArtikalRepo : IArtikalRepo
    {
        private readonly IConnFactory _connFactory;

        public ArtikalRepo(IConnFactory connFactory)
        {
            _connFactory = connFactory;
        }

        public async Task<IEnumerable<Artikal>> GetAllAsync()
        {
            const string sql = """
                SELECT
                    ArtikalId,
                    Sifra,
                    Naziv,
                    DobavljacId,
                    RobnaGrupaId,
                    Aktivan
                FROM dbo.Artikal
                ORDER BY Naziv;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<Artikal>(sql);
        }

        public async Task<Artikal?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT
                    ArtikalId,
                    Sifra,
                    Naziv,
                    DobavljacId,
                    RobnaGrupaId,
                    Aktivan
                FROM dbo.Artikal
                WHERE ArtikalId = @Id;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Artikal>(
                sql,
                new { Id = id }
            );
        }

        public async Task<Artikal?> GetBySifraAsync(string sifra)
        {
            const string sql = """
                SELECT
                    ArtikalId,
                    Sifra,
                    Naziv,
                    DobavljacId,
                    RobnaGrupaId,
                    Aktivan
                FROM dbo.Artikal
                WHERE Sifra = @Sifra;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Artikal>(
                sql,
                new { Sifra = sifra }
            );
        }

        public async Task<IEnumerable<Artikal>> SearchAsync(
            string? naziv,
            int? dobavljacId,
            int? robnaGrupaId,
            bool? aktivan)
        {
            const string sql = """
                SELECT
                    ArtikalId,
                    Sifra,
                    Naziv,
                    DobavljacId,
                    RobnaGrupaId,
                    Aktivan
                FROM dbo.Artikal
                WHERE
                    (@Naziv IS NULL OR Naziv LIKE '%' + @Naziv + '%')
                    AND (@DobavljacId IS NULL OR DobavljacId = @DobavljacId)
                    AND (@RobnaGrupaId IS NULL OR RobnaGrupaId = @RobnaGrupaId)
                    AND (@Aktivan IS NULL OR Aktivan = @Aktivan)
                ORDER BY Naziv;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<Artikal>(
                sql,
                new
                {
                    Naziv = string.IsNullOrWhiteSpace(naziv) ? null : naziv,
                    DobavljacId = dobavljacId,
                    RobnaGrupaId = robnaGrupaId,
                    Aktivan = aktivan
                }
            );
        }
    }
}