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

        public async Task<IEnumerable<Artikal>> GetAllAsync(
    bool canViewAllCategories,
    List<int> kategorijaIds)
        {
            const string sql = """
        SELECT
            a.ArtikalId,
            a.Sifra,
            a.Naziv,
            a.DobavljacId,
            a.RobnaGrupaId,
            a.Aktivan
        FROM dbo.Artikal a

        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId

        WHERE
            @CanViewAllCategories = 1
            OR rg.KategorijaId IN @KategorijaIds

        ORDER BY a.Naziv;
        """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<Artikal>(
                sql,
                new
                {
                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds
                }
            );
        }

        public async Task<Artikal?> GetByIdAsync(
     int id,
     bool canViewAllCategories,
     List<int> kategorijaIds)
        {
            const string sql = """
        SELECT
            a.ArtikalId,
            a.Sifra,
            a.Naziv,
            a.DobavljacId,
            a.RobnaGrupaId,
            a.Aktivan
        FROM dbo.Artikal a

        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId

        WHERE
            a.ArtikalId = @Id
            AND
            (
                @CanViewAllCategories = 1
                OR rg.KategorijaId IN @KategorijaIds
            );
        """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Artikal>(
                sql,
                new
                {
                    Id = id,
                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds
                }
            );
        }

        public async Task<Artikal?> GetBySifraAsync(
     string sifra,
     bool canViewAllCategories,
     List<int> kategorijaIds)
        {
            const string sql = """
        SELECT
            a.ArtikalId,
            a.Sifra,
            a.Naziv,
            a.DobavljacId,
            a.RobnaGrupaId,
            a.Aktivan
        FROM dbo.Artikal a

        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId

        WHERE
            a.Sifra = @Sifra
            AND
            (
                @CanViewAllCategories = 1
                OR rg.KategorijaId IN @KategorijaIds
            );
        """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Artikal>(
                sql,
                new
                {
                    Sifra = sifra,
                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds
                }
            );
        }

        public async Task<IEnumerable<Artikal>> SearchAsync(
    string? naziv,
    int? dobavljacId,
    int? robnaGrupaId,
    bool? aktivan,
    bool canViewAllCategories,
    List<int> kategorijaIds)
        {
            const string sql = """
        SELECT
            a.ArtikalId,
            a.Sifra,
            a.Naziv,
            a.DobavljacId,
            a.RobnaGrupaId,
            a.Aktivan
        FROM dbo.Artikal a

        INNER JOIN dbo.RobnaGrupa rg
            ON rg.RobnaGrupaId = a.RobnaGrupaId

        WHERE
            (@Naziv IS NULL OR a.Naziv LIKE '%' + @Naziv + '%')
            AND (@DobavljacId IS NULL OR a.DobavljacId = @DobavljacId)
            AND (@RobnaGrupaId IS NULL OR a.RobnaGrupaId = @RobnaGrupaId)
            AND (@Aktivan IS NULL OR a.Aktivan = @Aktivan)

            AND
            (
                @CanViewAllCategories = 1
                OR rg.KategorijaId IN @KategorijaIds
            )

        ORDER BY a.Naziv;
        """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<Artikal>(
                sql,
                new
                {
                    Naziv = string.IsNullOrWhiteSpace(naziv)
                        ? null
                        : naziv,

                    DobavljacId = dobavljacId,
                    RobnaGrupaId = robnaGrupaId,
                    Aktivan = aktivan,

                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds
                }
            );
        }
    }
}