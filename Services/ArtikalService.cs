using GomexPraksa.Repository;
using Models.Dtos;
using Models.ModelsDash;

namespace GomexPraksa.Services
{
    public class ArtikalService : IArtikalService
    {
        private readonly IArtikalRepo _artikalRepo;

        public ArtikalService(IArtikalRepo artikalRepo)
        {
            _artikalRepo = artikalRepo;
        }

        public async Task<IEnumerable<ArtikalDto>> GetAllAsync()
        {
            var artikli = await _artikalRepo.GetAllAsync();

            return artikli.Select(MapToDto);
        }

        public async Task<ArtikalDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "ArtikalId mora biti veći od nule.");
            }

            var artikal = await _artikalRepo.GetByIdAsync(id);

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

            var artikal = await _artikalRepo.GetBySifraAsync(
                sifra.Trim());

            if (artikal is null)
            {
                throw new KeyNotFoundException(
                    $"Artikal sa šifrom '{sifra}' nije pronađen.");
            }

            return MapToDto(artikal);
        }

        public async Task<IEnumerable<ArtikalDto>> SearchAsync(
            ArtikalFilterDto filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var artikli = await _artikalRepo.SearchAsync(
                filter.Naziv,
                filter.DobavljacId,
                filter.RobnaGrupaId,
                filter.Aktivan);

            return artikli.Select(MapToDto);
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
                Aktivan = artikal.Aktivan
            };
        }
    }
}
