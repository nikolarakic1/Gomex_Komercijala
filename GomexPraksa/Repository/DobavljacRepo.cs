using Dapper;
using GomexPraksa.ConnectionFactory;
using Models.ModelsDash;

namespace GomexPraksa.Repository;

    public class DobavljacRepo :  IDobavljacRepo
    {
    private readonly IConnFactory _connFactory;
    public DobavljacRepo(IConnFactory connFactory)
    {
        _connFactory = connFactory;
    }

    public async Task<IEnumerable<Dobavljac>> GetAllDobavljace()
    {
        var sql = """
            SELECT
            DobavljacId,
            Naziv,
            Aktivan FROM dbo_Dobavljac;
            
            """;
        using var connection = _connFactory.CreateConnection();
        return await connection.QueryAsync<Dobavljac>(sql);
    }

    public async Task<Dobavljac?> GetByIdAsync(int id)
    {
        const string sql = """
        SELECT
            DobavljacId,
            Naziv,
            Aktivan
        FROM dbo.Dobavljac
        WHERE DobavljacId = @Id;
        """;

        using var connection = _connFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Dobavljac>(
            sql,
            new { Id = id }
        );
    }

    public async Task<IEnumerable<Dobavljac>> SearchAsync(
     string? naziv,
     bool? aktivan)
    {
        const string sql = """
        SELECT TOP (5)
            DobavljacId,
            Naziv,
            Aktivan
        FROM dbo.Dobavljac
        WHERE
            (@Naziv IS NULL OR Naziv LIKE '%' + @Naziv + '%')
            AND
            (@Aktivan IS NULL OR Aktivan = @Aktivan)
        ORDER BY Naziv;
        """;

        using var connection = _connFactory.CreateConnection();

        return await connection.QueryAsync<Dobavljac>(
            sql,
            new
            {
                Naziv = string.IsNullOrWhiteSpace(naziv)
                    ? null
                    : naziv.Trim(),

                Aktivan = aktivan
            });
    }
}

