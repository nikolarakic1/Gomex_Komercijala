using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.ModelsDash;

namespace GomexPraksa.Repository
{
    public class DobavljacRepo : IDobavljacRepo
    {
        private readonly IConnFactory _connFactory;

        public DobavljacRepo(IConnFactory connFactory)
        {
            _connFactory = connFactory;
        }

        public async Task<IEnumerable<Dobavljac>> GetAllDobavljace(
            bool canViewAllCategories,
            List<int> kategorijaIds)
        {
            const string sql = """
                SELECT
                    d.DobavljacId,
                    d.Naziv,
                    d.Aktivan
                FROM dbo.Dobavljac d
                WHERE
                    @CanViewAllCategories = 1
                    OR EXISTS
                    (
                        SELECT 1
                        FROM dbo.Artikal a
                        INNER JOIN dbo.RobnaGrupa rg
                            ON rg.RobnaGrupaId = a.RobnaGrupaId
                        WHERE a.DobavljacId = d.DobavljacId
                          AND rg.KategorijaId IN @KategorijaIds
                    )
                ORDER BY d.Naziv;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<Dobavljac>(
                sql,
                new
                {
                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds
                }
            );
        }

        public async Task<Dobavljac?> GetByIdAsync(
            int id,
            bool canViewAllCategories,
            List<int> kategorijaIds)
        {
            const string sql = """
                SELECT
                    d.DobavljacId,
                    d.Naziv,
                    d.Aktivan
                FROM dbo.Dobavljac d
                WHERE
                    d.DobavljacId = @Id
                    AND
                    (
                        @CanViewAllCategories = 1
                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.Artikal a
                            INNER JOIN dbo.RobnaGrupa rg
                                ON rg.RobnaGrupaId = a.RobnaGrupaId
                            WHERE a.DobavljacId = d.DobavljacId
                              AND rg.KategorijaId IN @KategorijaIds
                        )
                    );
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection
                .QueryFirstOrDefaultAsync<Dobavljac>(
                    sql,
                    new
                    {
                        Id = id,
                        CanViewAllCategories = canViewAllCategories,
                        KategorijaIds = kategorijaIds
                    }
                );
        }

        public async Task<IEnumerable<Dobavljac>> SearchAsync(
            string? naziv,
            bool? aktivan,
            bool canViewAllCategories,
            List<int> kategorijaIds)
        {
            const string sql = """
                SELECT TOP (5)
                    d.DobavljacId,
                    d.Naziv,
                    d.Aktivan
                FROM dbo.Dobavljac d
                WHERE
                    (@Naziv IS NULL
                        OR d.Naziv LIKE '%' + @Naziv + '%')

                    AND
                    (@Aktivan IS NULL
                        OR d.Aktivan = @Aktivan)

                    AND
                    (
                        @CanViewAllCategories = 1
                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.Artikal a
                            INNER JOIN dbo.RobnaGrupa rg
                                ON rg.RobnaGrupaId = a.RobnaGrupaId
                            WHERE a.DobavljacId = d.DobavljacId
                              AND rg.KategorijaId IN @KategorijaIds
                        )
                    )

                ORDER BY d.Naziv;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<Dobavljac>(
                sql,
                new
                {
                    Naziv = string.IsNullOrWhiteSpace(naziv)
                        ? null
                        : naziv.Trim(),

                    Aktivan = aktivan,

                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds
                }
            );
        }
        public async Task<IEnumerable<Dobavljac>> CriticalDobavljaciAsync(bool canViewAllCategories, List<int> KategorijaIds)
        {
            const string sql = """
                
                """;
            throw new ArgumentException();
        }
    }
}