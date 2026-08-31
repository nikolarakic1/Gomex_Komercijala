using GomexPraksa.AddedFunctions;
using GomexPraksa.JWTInfo;
using GomexPraksa.Repository;
using Models.Dtos;
using Models.ModelsDash;

namespace GomexPraksa.Services
{
    public class ArtikalService : IArtikalService
    {
        private readonly IArtikalRepo _artikalRepo;
        private readonly IUserAccess _userAccess;

        public ArtikalService(IArtikalRepo artikalRepo, IUserAccess userAccess)
        {
            _artikalRepo = artikalRepo;
            _userAccess = userAccess;
        }

        public async Task<PaginationGeneric<ArtikalDto>> GetAllAsync(
    PaginationParams pagination)
        {
            var access =
                await _userAccess.GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena nijedna kategorija."
                );
            }

            var artikli =
                await _artikalRepo.GetAllAsync(
                    access.CanViewAllCategories,
                    access.KategorijaIds,
                    pagination
                );

            return new PaginationGeneric<ArtikalDto>
            {
                Items = artikli.Items.Select(MapToDto),
                Page = artikli.Page,
                PageSize = artikli.PageSize,
                TotalCount = artikli.TotalCount
            };
        }

        public async Task<ArtikalDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "ArtikalId mora biti veći od nule.");
            }

            var access =
                await _userAccess.GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena nijedna kategorija."
                );
            }

            var artikal = await _artikalRepo.GetByIdAsync(
                id,
                access.CanViewAllCategories,
                access.KategorijaIds
            );

            if (artikal is null)
            {
                throw new KeyNotFoundException(
                    $"Artikal sa ID-em {id} nije pronađen.");
            }

            return MapToDto(artikal);
        }
        public async Task<ArtikalDto> GetBySifraAsync(string sifra)
        {
            if (string.IsNullOrWhiteSpace(sifra))
            {
                throw new ArgumentException(
                    "Šifra artikla je obavezna.");
            }

            var access =
                await _userAccess.GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena nijedna kategorija."
                );
            }

            var artikal = await _artikalRepo.GetBySifraAsync(
                sifra.Trim(),
                access.CanViewAllCategories,
                access.KategorijaIds
            );

            if (artikal is null)
            {
                throw new KeyNotFoundException(
                    $"Artikal sa šifrom '{sifra}' nije pronađen.");
            }

            return MapToDto(artikal);
        }

        public async Task<PaginationGeneric<ArtikalDto>> SearchAsync(
    ArtikalFilterDto filter,
    PaginationParams pagination)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var access =
                await _userAccess.GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena nijedna kategorija."
                );
            }

            var artikli =
                await _artikalRepo.SearchAsync(
                    filter.Naziv,
                    filter.DobavljacId,
                    filter.RobnaGrupaId,
                    filter.Aktivan,
                    access.CanViewAllCategories,
                    access.KategorijaIds,
                    pagination
                );

            return new PaginationGeneric<ArtikalDto>
            {
                Items =
                    artikli.Items
                        .Select(MapToDto)
                        .ToList(),

                Page =
                    artikli.Page,

                PageSize =
                    artikli.PageSize,

                TotalCount =
                    artikli.TotalCount
            };
        }

        private static ArtikalDto MapToDto(Artikal artikal)
        {
            return new ArtikalDto
            {
                ArtikalId = artikal.ArtikalId,
                Sifra = artikal.Sifra,
                Naziv = artikal.Naziv,
                DobavljacId = artikal.DobavljacId,
                RobnaGrupaId = artikal.RobnaGrupaId,
                Aktivan = artikal.Aktivan,
                RedovnaCena = artikal.RedovnaCena
            };
        }
    }
}
