using GomexPraksa.JWTInfo;
using GomexPraksa.Repository;
using Models.Dtos;
using Models.DtosComerc;
using Models.ModelsDash;
using Models.ReadDetails;

namespace GomexPraksa.Services
{
    public class AkcijaService : IAkcijaService
    {
        private readonly IAkcijaRepo _akcijaRepo;
        private readonly IArtikalRepo _artikalRepo;
    

        public AkcijaService(IAkcijaRepo akcijaRepo,IArtikalRepo artikalRepo)
        {
            _akcijaRepo = akcijaRepo;
            _artikalRepo = artikalRepo;
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
        public async Task<AkcijaDTO?> DodajAkciju(
    DodajAkcijuDTO dto)
        {
            var artikal =
                await _artikalRepo.GetBySifraAsync(
                    dto.SifraArtikla,
                    true,
                    new List<int>()
                );

            if (artikal is null)
            {
                return null;
            }

            var novaAkcija = new Akcija
            {
                ArtikalId = artikal.ArtikalId,

                DatumOd = dto.DatumOd.ToDateTime(
                    TimeOnly.MinValue
                ),

                DatumDo = dto.DatumDo.ToDateTime(
                    TimeOnly.MinValue
                ),

                AkcijskaCena = dto.AkcijskaCena,

                TipAkcijeId = dto.TipAkcijeId
            };

            var rezultat =
                await _akcijaRepo.DodajAkciju(
                    novaAkcija
                );

            if (rezultat is null)
            {
                return null;
            }

            return MapToDtoNovaAkcija(rezultat);
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
                TipAkcije = akcija.TipAkcije,
                TipAkcijeId = akcija.TipAkcijeId
            };
        }
        private static AkcijaDTO MapToDtoNovaAkcija(Akcija akcija)
        {
            return new AkcijaDTO
            {
                AkcijaId = akcija.AkcijaId,
                ArtikalId = akcija.ArtikalId,
                DatumOd = akcija.DatumOd,
                DatumDo = akcija.DatumDo,
                AkcijskaCena = akcija.AkcijskaCena,
                TipAkcijeId = akcija.TipAkcijeId
            };
        }
    }
}