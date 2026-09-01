using Dapper;
using GomexPraksa.AddedFunctions;
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

        public async Task<PaginationGeneric<Dobavljac>> GetAllDobavljace(
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams pagination)
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

                        WHERE
                            a.DobavljacId = d.DobavljacId
                            AND rg.KategorijaId IN @KategorijaIds
                    )

                ORDER BY d.Naziv
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;


                SELECT COUNT(*)
                FROM dbo.Dobavljac d
                WHERE
                    @CanViewAllCategories = 1
                    OR EXISTS
                    (
                        SELECT 1
                        FROM dbo.Artikal a

                        INNER JOIN dbo.RobnaGrupa rg
                            ON rg.RobnaGrupaId = a.RobnaGrupaId

                        WHERE
                            a.DobavljacId = d.DobavljacId
                            AND rg.KategorijaId IN @KategorijaIds
                    );
                """;

            using var connection =
                _connFactory.CreateConnection();

            var offset =
                (pagination.Page - 1)
                * pagination.PageSize;

            using var result =
                await connection.QueryMultipleAsync(
                    sql,
                    new
                    {
                        CanViewAllCategories =
                            canViewAllCategories,

                        KategorijaIds =
                            kategorijaIds,

                        Offset =
                            offset,

                        PageSize =
                            pagination.PageSize
                    }
                );

            var dobavljaci =
                (await result.ReadAsync<Dobavljac>())
                .ToList();

            var totalCount =
                await result.ReadSingleAsync<int>();

            return new PaginationGeneric<Dobavljac>
            {
                Items = dobavljaci,
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
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

                            WHERE
                                a.DobavljacId = d.DobavljacId
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

                        CanViewAllCategories =
                            canViewAllCategories,

                        KategorijaIds =
                            kategorijaIds
                    }
                );
        }

        public async Task<PaginationGeneric<Dobavljac>> SearchAsync(
            string? naziv,
            bool? aktivan,
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams pagination)
        {
            const string sql = """
                SELECT
                    d.DobavljacId,
                    d.Naziv,
                    d.Aktivan
                FROM dbo.Dobavljac d
                WHERE
                    (
                        @Naziv IS NULL
                        OR d.Naziv LIKE '%' + @Naziv + '%'
                    )

                    AND
                    (
                        @Aktivan IS NULL
                        OR d.Aktivan = @Aktivan
                    )

                    AND
                    (
                        @CanViewAllCategories = 1

                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.Artikal a

                            INNER JOIN dbo.RobnaGrupa rg
                                ON rg.RobnaGrupaId = a.RobnaGrupaId

                            WHERE
                                a.DobavljacId = d.DobavljacId
                                AND rg.KategorijaId IN @KategorijaIds
                        )
                    )

                ORDER BY d.Naziv
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;


                SELECT COUNT(*)
                FROM dbo.Dobavljac d
                WHERE
                    (
                        @Naziv IS NULL
                        OR d.Naziv LIKE '%' + @Naziv + '%'
                    )

                    AND
                    (
                        @Aktivan IS NULL
                        OR d.Aktivan = @Aktivan
                    )

                    AND
                    (
                        @CanViewAllCategories = 1

                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.Artikal a

                            INNER JOIN dbo.RobnaGrupa rg
                                ON rg.RobnaGrupaId = a.RobnaGrupaId

                            WHERE
                                a.DobavljacId = d.DobavljacId
                                AND rg.KategorijaId IN @KategorijaIds
                        )
                    );
                """;

            using var connection =
                _connFactory.CreateConnection();

            var offset =
                (pagination.Page - 1)
                * pagination.PageSize;

            var parametri = new
            {
                Naziv =
                    string.IsNullOrWhiteSpace(naziv)
                        ? null
                        : naziv.Trim(),

                Aktivan =
                    aktivan,

                CanViewAllCategories =
                    canViewAllCategories,

                KategorijaIds =
                    kategorijaIds,

                Offset =
                    offset,

                PageSize =
                    pagination.PageSize
            };

            using var result =
                await connection.QueryMultipleAsync(
                    sql,
                    parametri
                );

            var dobavljaci =
                (await result.ReadAsync<Dobavljac>())
                .ToList();

            var totalCount =
                await result.ReadSingleAsync<int>();

            return new PaginationGeneric<Dobavljac>
            {
                Items = dobavljaci,
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginationGeneric<Dobavljac>>
            CriticalDobavljaciAsync(
                bool canViewAllCategories,
                List<int> kategorijaIds,
                PaginationParams pagination)
        {
            throw new NotImplementedException();
        }
    }
}