using Dapper;
using GomexPraksa.Auth;
using GomexPraksa.ConnectionFactory;
using Microsoft.EntityFrameworkCore;
using Models.ModelsDash;
using Models.ReadDetails;

namespace GomexPraksa.Repository
{
    public class AkcijaRepo : IAkcijaRepo
    {
        private readonly IConnFactory _connFactory;
        private readonly AuthDbContext _context;

        public AkcijaRepo(IConnFactory connFactory , AuthDbContext context)
        {
            _connFactory = connFactory;
            _context = context;
        }

        public async Task<IEnumerable<AkcijaDetalji>> GetAllAsync()
        {
            const string sql = """
                SELECT
                    a.AkcijaId,
                    a.ArtikalId,
                    a.DatumOd,
                    a.DatumDo,
                    a.AkcijskaCena,
                    a.TipAkcijeId,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija a
                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId
                ORDER BY a.DatumOd DESC;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(sql);
        }

        public async Task<IEnumerable<AkcijaDetalji>> GetBuduceAsync()
        {
            const string sql = """
                SELECT
                    a.AkcijaId,
                    a.ArtikalId,
                    a.DatumOd,
                    a.DatumDo,
                    a.AkcijskaCena,
                    a.TipAkcijeId,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija a
                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId
                WHERE a.DatumOd > GETDATE()
                ORDER BY a.DatumOd;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(sql);
        }

        public async Task<IEnumerable<AkcijaDetalji>> GetByArtikalIdAsync(
            int artikalId)
        {
            const string sql = """
                SELECT
                    a.AkcijaId,
                    a.ArtikalId,
                    a.DatumOd,
                    a.DatumDo,
                    a.AkcijskaCena,
                    a.TipAkcijeId,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija a
                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId
                WHERE a.ArtikalId = @ArtikalId
                ORDER BY a.DatumOd DESC;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(
                sql,
                new { ArtikalId = artikalId }
            );
        }

        public async Task<AkcijaDetalji?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT
                    a.AkcijaId,
                    a.ArtikalId,
                    a.DatumOd,
                    a.DatumDo,
                    a.AkcijskaCena,
                    a.TipAkcijeId,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija a
                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId
                WHERE a.AkcijaId = @AkcijaId;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<AkcijaDetalji>(
                sql,
                new { AkcijaId = id }
            );
        }

        public async Task<AkcijaDetalji?> GetPoslednjuZaArtikalAsync(
            int artikalId)
        {
            const string sql = """
                SELECT TOP 1
                    a.AkcijaId,
                    a.ArtikalId,
                    a.DatumOd,
                    a.DatumDo,
                    a.AkcijskaCena,
                    a.TipAkcijeId,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija a
                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId
                WHERE a.ArtikalId = @ArtikalId
                  AND a.DatumDo < GETDATE()
                ORDER BY a.DatumDo DESC;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<AkcijaDetalji>(
                sql,
                new { ArtikalId = artikalId }
            );
        }

        public async Task<IEnumerable<AkcijaDetalji>> GetTrenutneAsync()
        {
            const string sql = """
                SELECT
                    a.AkcijaId,
                    a.ArtikalId,
                    a.DatumOd,
                    a.DatumDo,
                    a.AkcijskaCena,
                    a.TipAkcijeId,
                    ta.Naziv AS TipAkcije
                FROM dbo.Akcija a
                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId
                WHERE a.DatumOd <= GETDATE()
                  AND a.DatumDo >= GETDATE()
                ORDER BY a.DatumDo ASC;
                """;

            using var connection = _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(sql);
        }
        public async Task<Akcija?> DodajAkciju(Akcija akcija)
        {
            if(akcija is null)
    {
                return null;
            }

            await _context.Akcija.AddAsync(akcija);

            var rezultat =
                await _context.SaveChangesAsync();

            if (rezultat == 0)
            {
                return null;
            }

            return akcija;


        }
    }
}