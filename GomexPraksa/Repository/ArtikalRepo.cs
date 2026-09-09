using Dapper;
using GomexPraksa.AddedFunctions;
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

        public async Task<PaginationGeneric<Artikal>> GetAllAsync(
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams paginationArtikli)
        {
            const string sql = """
                SELECT
                    a.ArtikalId,
                    a.Sifra,
                    a.Naziv,
                    a.DobavljacId,
                    a.RobnaGrupaId,
                    a.Aktivan,
                    a.RedovnaCena,
                    b.Naziv AS NazivDobavljaca
                FROM dbo.Artikal a
                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId
                INNER JOIN dbo.Dobavljac b
                    ON b.DobavljacId = a.DobavljacId
                WHERE
                    @CanViewAllCategories = 1
                    OR rg.KategorijaId IN @KategorijaIds
                ORDER BY a.Naziv
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM dbo.Artikal a
                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId
                WHERE
                    @CanViewAllCategories = 1
                    OR rg.KategorijaId IN @KategorijaIds;
                """;

            using var connection = _connFactory.CreateConnection();

            var offset =
                (paginationArtikli.Page - 1)
                * paginationArtikli.PageSize;

            List<Artikal> artikli;
            int totalCount;

            using (var result = await connection.QueryMultipleAsync(
                sql,
                new
                {
                    CanViewAllCategories = canViewAllCategories,
                    KategorijaIds = kategorijaIds,
                    Offset = offset,
                    PageSize = paginationArtikli.PageSize
                }))
            {
                artikli =
                    (await result.ReadAsync<Artikal>())
                    .ToList();

                totalCount =
                    await result.ReadSingleAsync<int>();
            }

            await PopuniStatistikuAsync(
                connection,
                artikli
            );

            return new PaginationGeneric<Artikal>
            {
                Items = artikli,
                Page = paginationArtikli.Page,
                PageSize = paginationArtikli.PageSize,
                TotalCount = totalCount
            };
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
                    a.Aktivan,
                    a.RedovnaCena,
                    b.Naziv AS NazivDobavljaca
                FROM dbo.Artikal a
                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId
                INNER JOIN dbo.Dobavljac b
                    ON b.DobavljacId = a.DobavljacId
                WHERE
                    a.ArtikalId = @Id
                    AND
                    (
                        @CanViewAllCategories = 1
                        OR rg.KategorijaId IN @KategorijaIds
                    );
                """;

            using var connection =
                _connFactory.CreateConnection();

            var artikal =
                await connection.QuerySingleOrDefaultAsync<Artikal>(
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

            if (artikal == null)
            {
                return null;
            }

            await PopuniStatistikuAsync(
                connection,
                new List<Artikal>
                {
                    artikal
                }
            );

            return artikal;
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
                    a.Aktivan,
                    a.RedovnaCena,
                    b.Naziv AS NazivDobavljaca
                FROM dbo.Artikal a
                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId
                INNER JOIN dbo.Dobavljac b
                    ON b.DobavljacId = a.DobavljacId
                WHERE
                    a.Sifra = @Sifra
                    AND
                    (
                        @CanViewAllCategories = 1
                        OR rg.KategorijaId IN @KategorijaIds
                    );
                """;

            using var connection =
                _connFactory.CreateConnection();

            var artikal =
                await connection.QuerySingleOrDefaultAsync<Artikal>(
                    sql,
                    new
                    {
                        Sifra = sifra,
                        CanViewAllCategories =
                            canViewAllCategories,
                        KategorijaIds =
                            kategorijaIds
                    }
                );

            if (artikal == null)
            {
                return null;
            }

            await PopuniStatistikuAsync(
                connection,
                new List<Artikal>
                {
                    artikal
                }
            );

            return artikal;
        }

        public async Task<PaginationGeneric<Artikal>> SearchAsync(
            string? naziv,
            int? dobavljacId,
            int? robnaGrupaId,
            int? odeljenjeId,
            string? sifra,
            bool? aktivan,
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams paginationArtikli)
        {
            const string sql = """
                SELECT
                    a.ArtikalId,
                    a.Sifra,
                    a.Naziv,
                    a.DobavljacId,
                    a.RobnaGrupaId,
                    a.Aktivan,
                    a.RedovnaCena,
                    b.Naziv AS NazivDobavljaca
                FROM dbo.Artikal a
                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId
                INNER JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId
                INNER JOIN dbo.Dobavljac b
                    ON b.DobavljacId = a.DobavljacId
                WHERE
                    (
                        @Naziv IS NULL
                        OR a.Naziv LIKE '%' + @Naziv + '%'
                    )
                    AND
                    (
                        @DobavljacId IS NULL
                        OR a.DobavljacId = @DobavljacId
                    )
                    AND
                    (
                        @RobnaGrupaId IS NULL
                        OR a.RobnaGrupaId = @RobnaGrupaId
                    )
                    AND
                    (
                        @OdeljenjeId IS NULL
                        OR k.OdeljenjeId = @OdeljenjeId
                    )
                    AND
                    (
                        @Sifra IS NULL
                        OR a.Sifra LIKE '%' + @Sifra + '%'
                    )
                    AND
                    (
                        @Aktivan IS NULL
                        OR a.Aktivan = @Aktivan
                    )
                    AND
                    (
                        @CanViewAllCategories = 1
                        OR rg.KategorijaId IN @KategorijaIds
                    )
                ORDER BY a.Naziv
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM dbo.Artikal a
                INNER JOIN dbo.RobnaGrupa rg
                    ON rg.RobnaGrupaId = a.RobnaGrupaId
                INNER JOIN dbo.Kategorija k
                    ON k.KategorijaId = rg.KategorijaId
                WHERE
                    (
                        @Naziv IS NULL
                        OR a.Naziv LIKE '%' + @Naziv + '%'
                    )
                    AND
                    (
                        @DobavljacId IS NULL
                        OR a.DobavljacId = @DobavljacId
                    )
                    AND
                    (
                        @RobnaGrupaId IS NULL
                        OR a.RobnaGrupaId = @RobnaGrupaId
                    )
                    AND
                    (
                        @OdeljenjeId IS NULL
                        OR k.OdeljenjeId = @OdeljenjeId
                    )
                    AND
                    (
                        @Sifra IS NULL
                        OR a.Sifra LIKE '%' + @Sifra + '%'
                    )
                    AND
                    (
                        @Aktivan IS NULL
                        OR a.Aktivan = @Aktivan
                    )
                    AND
                    (
                        @CanViewAllCategories = 1
                        OR rg.KategorijaId IN @KategorijaIds
                    );
                """;

            using var connection =
                _connFactory.CreateConnection();

            var offset =
                (paginationArtikli.Page - 1)
                * paginationArtikli.PageSize;

            var parametri = new
            {
                Naziv =
                    string.IsNullOrWhiteSpace(naziv)
                        ? null
                        : naziv.Trim(),

                DobavljacId =
                    dobavljacId,

                RobnaGrupaId =
                    robnaGrupaId,

                OdeljenjeId =
                    odeljenjeId,

                Sifra =
                    string.IsNullOrWhiteSpace(sifra)
                        ? null
                        : sifra.Trim(),

                Aktivan =
                    aktivan,

                CanViewAllCategories =
                    canViewAllCategories,

                KategorijaIds =
                    kategorijaIds,

                Offset =
                    offset,

                PageSize =
                    paginationArtikli.PageSize
            };

            List<Artikal> artikli;
            int totalCount;

            using (var result =
                await connection.QueryMultipleAsync(
                    sql,
                    parametri
                ))
            {
                artikli =
                    (await result.ReadAsync<Artikal>())
                    .ToList();

                totalCount =
                    await result.ReadSingleAsync<int>();
            }

            await PopuniStatistikuAsync(
                connection,
                artikli
            );

            return new PaginationGeneric<Artikal>
            {
                Items = artikli,
                Page = paginationArtikli.Page,
                PageSize = paginationArtikli.PageSize,
                TotalCount = totalCount
            };
        }

        private async Task PopuniStatistikuAsync(
    System.Data.IDbConnection connection,
    List<Artikal> artikli)
        {
            if (artikli.Count == 0)
            {
                return;
            }

            var ids = artikli
                .Select(a => a.ArtikalId)
                .Distinct()
                .ToList();

            const string sql = """
        WITH Agregat AS
        (
            SELECT
                kr.ArtikalId,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 6
                        THEN COALESCE(kr.MPBezPDV, 0)
                        ELSE 0
                    END
                ) AS ActualPromet,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 6
                        THEN COALESCE(kr.RUC12, 0)
                        ELSE 0
                    END
                ) AS ActualRuc,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 7
                        THEN COALESCE(kr.MPBezPDV, 0)
                        ELSE 0
                    END
                ) AS PlanPromet,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 7
                        THEN COALESCE(kr.RUC12, 0)
                        ELSE 0
                    END
                ) AS PlanRuc,

                SUM(
                    CASE
                        WHEN kr.TipProdajeId = 6
                        THEN COALESCE(kr.NedostatakMargine, 0)
                        ELSE 0
                    END
                ) AS NedostatakMargine,

                MAX(
                    kr.DatumRezultata
                ) AS PoslednjiDatumPodataka

            FROM dbo.KomercijalniRezultat kr

            WHERE
                kr.ArtikalId IN @ArtikalIds

            GROUP BY
                kr.ArtikalId
        ),
        Izracunato AS
        (
            SELECT
                ArtikalId,

                ActualPromet,

                ActualRuc,

                PlanPromet,

                PlanRuc,

                CASE
                    WHEN ActualPromet = 0
                    THEN 0
                    ELSE ActualRuc / NULLIF(ActualPromet, 0)
                END AS ActualRucPct,

                CASE
                    WHEN PlanPromet = 0
                    THEN 0
                    ELSE PlanRuc / NULLIF(PlanPromet, 0)
                END AS PlanRucPct,

                NedostatakMargine,

                PoslednjiDatumPodataka

            FROM Agregat
        )
        SELECT
            ArtikalId,

            ActualPromet AS Promet,

            ActualRuc AS RUC12,

            PlanPromet,

            PlanRuc AS PlanRUC12,

            ActualRucPct AS RUC12Procenat,

            PlanRucPct AS PlanRUC12Procenat,

            ActualRucPct
                - PlanRucPct
                AS OdstupanjeRUC12ProcentniPoeni,

            (
                ActualRuc
                - PlanRuc
                -
                (
                    (ActualPromet - PlanPromet)
                    * PlanRucPct
                )
            ) AS Margina,

            NedostatakMargine,

            PoslednjiDatumPodataka

        FROM Izracunato;
        """;

            var statistike =
                (
                    await connection.QueryAsync<ArtikalStatistikaRow>(
                        sql,
                        new
                        {
                            ArtikalIds = ids
                        }
                    )
                )
                .ToDictionary(x => x.ArtikalId);

            foreach (var artikal in artikli)
            {
                if (!statistike.TryGetValue(
                    artikal.ArtikalId,
                    out var stat))
                {
                    continue;
                }

                artikal.Promet =
                    stat.Promet;

                artikal.RUC12 =
                    stat.RUC12;

                artikal.RUC12Procenat =
                    stat.RUC12Procenat;

                artikal.PlanPromet =
                    stat.PlanPromet;

                artikal.PlanRUC12 =
                    stat.PlanRUC12;

                artikal.PlanRUC12Procenat =
                    stat.PlanRUC12Procenat;

                artikal.OdstupanjeRUC12ProcentniPoeni =
                    stat.OdstupanjeRUC12ProcentniPoeni;

                artikal.Margina =
                    stat.Margina;

                artikal.NedostatakMargine =
                    stat.NedostatakMargine;

                artikal.PoslednjiDatumPodataka =
                    stat.PoslednjiDatumPodataka;
            }
        }

        private class ArtikalStatistikaRow
        {
            public int ArtikalId { get; set; }

            public decimal Promet { get; set; }

            public decimal RUC12 { get; set; }

            public decimal RUC12Procenat { get; set; }

            public decimal PlanPromet { get; set; }

            public decimal PlanRUC12 { get; set; }

            public decimal PlanRUC12Procenat { get; set; }

            public decimal OdstupanjeRUC12ProcentniPoeni { get; set; }

            public decimal Margina { get; set; }

            public decimal NedostatakMargine { get; set; }

            public DateTime? PoslednjiDatumPodataka { get; set; }
        }
    }
}