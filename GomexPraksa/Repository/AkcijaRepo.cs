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

        public AkcijaRepo(
            IConnFactory connFactory,
            AuthDbContext context)
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
                    ta.Naziv AS TipAkcije,

                    COALESCE(actual.ActualPromet, 0) AS ActualPromet,
                    COALESCE(actual.ActualRUC12, 0) AS ActualRUC12,

                    CASE
                        WHEN COALESCE(actual.ActualPromet, 0) = 0
                        THEN 0
                        ELSE
                            COALESCE(actual.ActualRUC12, 0)
                            / NULLIF(actual.ActualPromet, 0)
                    END AS ActualRUC12Procenat

                FROM dbo.Akcija a

                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId

                OUTER APPLY
                (
                    SELECT
                        SUM(COALESCE(kr.MPBezPDV, 0)) AS ActualPromet,
                        SUM(COALESCE(kr.RUC12, 0)) AS ActualRUC12

                    FROM dbo.KomercijalniRezultat kr

                    WHERE
                        kr.ArtikalId = a.ArtikalId
                        AND kr.TipProdajeId = 6
                        AND kr.DatumRezultata >= a.DatumOd
                        AND kr.DatumRezultata < DATEADD(DAY, 1, a.DatumDo)
                ) actual

                ORDER BY a.DatumOd DESC;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(
                sql
            );
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
                    ta.Naziv AS TipAkcije,

                    COALESCE(actual.ActualPromet, 0) AS ActualPromet,
                    COALESCE(actual.ActualRUC12, 0) AS ActualRUC12,

                    CASE
                        WHEN COALESCE(actual.ActualPromet, 0) = 0
                        THEN 0
                        ELSE
                            COALESCE(actual.ActualRUC12, 0)
                            / NULLIF(actual.ActualPromet, 0)
                    END AS ActualRUC12Procenat

                FROM dbo.Akcija a

                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId

                OUTER APPLY
                (
                    SELECT
                        SUM(COALESCE(kr.MPBezPDV, 0)) AS ActualPromet,
                        SUM(COALESCE(kr.RUC12, 0)) AS ActualRUC12

                    FROM dbo.KomercijalniRezultat kr

                    WHERE
                        kr.ArtikalId = a.ArtikalId
                        AND kr.TipProdajeId = 6
                        AND kr.DatumRezultata >= a.DatumOd
                        AND kr.DatumRezultata < DATEADD(DAY, 1, a.DatumDo)
                ) actual

                WHERE
                    a.DatumOd > GETDATE()

                ORDER BY a.DatumOd;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(
                sql
            );
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
                    ta.Naziv AS TipAkcije,

                    COALESCE(actual.ActualPromet, 0) AS ActualPromet,
                    COALESCE(actual.ActualRUC12, 0) AS ActualRUC12,

                    CASE
                        WHEN COALESCE(actual.ActualPromet, 0) = 0
                        THEN 0
                        ELSE
                            COALESCE(actual.ActualRUC12, 0)
                            / NULLIF(actual.ActualPromet, 0)
                    END AS ActualRUC12Procenat

                FROM dbo.Akcija a

                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId

                OUTER APPLY
                (
                    SELECT
                        SUM(COALESCE(kr.MPBezPDV, 0)) AS ActualPromet,
                        SUM(COALESCE(kr.RUC12, 0)) AS ActualRUC12

                    FROM dbo.KomercijalniRezultat kr

                    WHERE
                        kr.ArtikalId = a.ArtikalId
                        AND kr.TipProdajeId = 6
                        AND kr.DatumRezultata >= a.DatumOd
                        AND kr.DatumRezultata < DATEADD(DAY, 1, a.DatumDo)
                ) actual

                WHERE
                    a.ArtikalId = @ArtikalId

                ORDER BY a.DatumOd DESC;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(
                sql,
                new
                {
                    ArtikalId = artikalId
                }
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
                    ta.Naziv AS TipAkcije,

                    COALESCE(actual.ActualPromet, 0) AS ActualPromet,
                    COALESCE(actual.ActualRUC12, 0) AS ActualRUC12,

                    CASE
                        WHEN COALESCE(actual.ActualPromet, 0) = 0
                        THEN 0
                        ELSE
                            COALESCE(actual.ActualRUC12, 0)
                            / NULLIF(actual.ActualPromet, 0)
                    END AS ActualRUC12Procenat

                FROM dbo.Akcija a

                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId

                OUTER APPLY
                (
                    SELECT
                        SUM(COALESCE(kr.MPBezPDV, 0)) AS ActualPromet,
                        SUM(COALESCE(kr.RUC12, 0)) AS ActualRUC12

                    FROM dbo.KomercijalniRezultat kr

                    WHERE
                        kr.ArtikalId = a.ArtikalId
                        AND kr.TipProdajeId = 6
                        AND kr.DatumRezultata >= a.DatumOd
                        AND kr.DatumRezultata < DATEADD(DAY, 1, a.DatumDo)
                ) actual

                WHERE
                    a.AkcijaId = @AkcijaId;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection
                .QuerySingleOrDefaultAsync<AkcijaDetalji>(
                    sql,
                    new
                    {
                        AkcijaId = id
                    }
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
                    ta.Naziv AS TipAkcije,

                    COALESCE(actual.ActualPromet, 0) AS ActualPromet,
                    COALESCE(actual.ActualRUC12, 0) AS ActualRUC12,

                    CASE
                        WHEN COALESCE(actual.ActualPromet, 0) = 0
                        THEN 0
                        ELSE
                            COALESCE(actual.ActualRUC12, 0)
                            / NULLIF(actual.ActualPromet, 0)
                    END AS ActualRUC12Procenat

                FROM dbo.Akcija a

                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId

                OUTER APPLY
                (
                    SELECT
                        SUM(COALESCE(kr.MPBezPDV, 0)) AS ActualPromet,
                        SUM(COALESCE(kr.RUC12, 0)) AS ActualRUC12

                    FROM dbo.KomercijalniRezultat kr

                    WHERE
                        kr.ArtikalId = a.ArtikalId
                        AND kr.TipProdajeId = 6
                        AND kr.DatumRezultata >= a.DatumOd
                        AND kr.DatumRezultata < DATEADD(DAY, 1, a.DatumDo)
                ) actual

                WHERE
                    a.ArtikalId = @ArtikalId
                    AND a.DatumDo < GETDATE()

                ORDER BY a.DatumDo DESC;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection
                .QueryFirstOrDefaultAsync<AkcijaDetalji>(
                    sql,
                    new
                    {
                        ArtikalId = artikalId
                    }
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
                    ta.Naziv AS TipAkcije,

                    COALESCE(actual.ActualPromet, 0) AS ActualPromet,
                    COALESCE(actual.ActualRUC12, 0) AS ActualRUC12,

                    CASE
                        WHEN COALESCE(actual.ActualPromet, 0) = 0
                        THEN 0
                        ELSE
                            COALESCE(actual.ActualRUC12, 0)
                            / NULLIF(actual.ActualPromet, 0)
                    END AS ActualRUC12Procenat

                FROM dbo.Akcija a

                INNER JOIN dbo.TipAkcije ta
                    ON ta.TipAkcijeId = a.TipAkcijeId

                OUTER APPLY
                (
                    SELECT
                        SUM(COALESCE(kr.MPBezPDV, 0)) AS ActualPromet,
                        SUM(COALESCE(kr.RUC12, 0)) AS ActualRUC12

                    FROM dbo.KomercijalniRezultat kr

                    WHERE
                        kr.ArtikalId = a.ArtikalId
                        AND kr.TipProdajeId = 6
                        AND kr.DatumRezultata >= a.DatumOd
                        AND kr.DatumRezultata < DATEADD(DAY, 1, a.DatumDo)
                ) actual

                WHERE
                    a.DatumOd <= GETDATE()
                    AND a.DatumDo >= GETDATE()

                ORDER BY a.DatumDo ASC;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<AkcijaDetalji>(
                sql
            );
        }

        public async Task<Akcija?> DodajAkciju(Akcija akcija)
        {
            if (akcija is null)
            {
                return null;
            }

            await _context.Akcija.AddAsync(
                akcija
            );

            var rezultat =
                await _context.SaveChangesAsync();

            if (rezultat == 0)
            {
                return null;
            }

            return akcija;
        }

        public async Task<IEnumerable<TipAkcije>> GetAktivniTipoviAkcije()
        {
            const string sql = """
                SELECT
                    TipAkcijeId,
                    Naziv,
                    Aktivan
                FROM dbo.TipAkcije
                WHERE Aktivan = 1
                ORDER BY Naziv;
                """;

            using var connection =
                _connFactory.CreateConnection();

            return await connection.QueryAsync<TipAkcije>(
                sql
            );
        }
    }
}