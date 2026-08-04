using GomexPraksa.Repository;
using Models.Dtos;
using Models.ReadDetails;

namespace GomexPraksa.Services
{
    public class AkcijaService : IAkcijaService
    {
        private readonly IAkcijaRepo _akcijaRepo;

        public AkcijaService(IAkcijaRepo akcijaRepo)
        {
            _akcijaRepo = akcijaRepo;
        }

        public async Task<IEnumerable<AkcijaDTO>> GetAllAsync()
        {
            var akcije = await _akcijaRepo.GetAllAsync();

            return akcije.Select(MapToDto);
        }

        public async Task<IEnumerable<AkcijaDTO>> GetBuduceAsync()
        {
            var akcije = await _akcijaRepo.GetBuduceAsync();

            return akcije.Select(MapToDto);
        }

        public async Task<IEnumerable<AkcijaDTO>> GetTrenutneAsync()
        {
            var akcije = await _akcijaRepo.GetTrenutneAsync();

            return akcije.Select(MapToDto);
        }

        public async Task<IEnumerable<AkcijaDTO>> GetByArtikalIdAsync(
            int artikalId)
        {
            if (artikalId <= 0)
            {
                throw new ArgumentException(
                    "ArtikalId mora biti veći od nule.");
            }

            var akcije =
                await _akcijaRepo.GetByArtikalIdAsync(artikalId);

            return akcije.Select(MapToDto);
        }

        public async Task<AkcijaDTO> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "AkcijaId mora biti veći od nule.");
            }

            var akcija = await _akcijaRepo.GetByIdAsync(id);

            if (akcija is null)
            {
                throw new KeyNotFoundException(
                    $"Akcija sa ID-em {id} nije pronađena.");
            }

            return MapToDto(akcija);
        }

        public async Task<AkcijaDTO?> GetPoslednjuZaArtikalAsync(
            int artikalId)
        {
            if (artikalId <= 0)
            {
                throw new ArgumentException(
                    "ArtikalId mora biti veći od nule.");
            }

            var akcija =
                await _akcijaRepo.GetPoslednjuZaArtikalAsync(artikalId);

            return akcija is null
                ? null
                : MapToDto(akcija);
        }

        private static AkcijaDTO MapToDto(AkcijaDetalji akcija)
        {
            return new AkcijaDTO
            {
                AkcijaId = akcija.AkcijaId,
                ArtikalId = akcija.ArtikalId,
                DatumOd = akcija.DatumOd,
                DatumDo = akcija.DatumDo,
                AkcijskaCena = akcija.AkcijskaCena,
                TipAkcije = akcija.TipAkcije
            };
        }
    }
}