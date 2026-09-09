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

        public AkcijaService(
            IAkcijaRepo akcijaRepo,
            IArtikalRepo artikalRepo)
        {
            _akcijaRepo = akcijaRepo;
            _artikalRepo = artikalRepo;
        }

        public async Task<IEnumerable<AkcijaDTO>> GetAllAsync()
        {
            var akcije =
                await _akcijaRepo.GetAllAsync();

            return akcije
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<AkcijaDTO>> GetBuduceAsync()
        {
            var akcije =
                await _akcijaRepo.GetBuduceAsync();

            return akcije
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<AkcijaDTO>> GetTrenutneAsync()
        {
            var akcije =
                await _akcijaRepo.GetTrenutneAsync();

            return akcije
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<AkcijaDTO>> GetByArtikalIdAsync(
            int artikalId)
        {
            if (artikalId <= 0)
            {
                throw new ArgumentException(
                    "ArtikalId mora biti veći od nule."
                );
            }

            var akcije =
                await _akcijaRepo.GetByArtikalIdAsync(
                    artikalId
                );

            return akcije
                .Select(MapToDto)
                .ToList();
        }

        public async Task<AkcijaDTO> GetByIdAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "AkcijaId mora biti veći od nule."
                );
            }

            var akcija =
                await _akcijaRepo.GetByIdAsync(
                    id
                );

            if (akcija is null)
            {
                throw new KeyNotFoundException(
                    $"Akcija sa ID-em {id} nije pronađena."
                );
            }

            return MapToDto(
                akcija
            );
        }

        public async Task<AkcijaDTO?> GetPoslednjuZaArtikalAsync(
            int artikalId)
        {
            if (artikalId <= 0)
            {
                throw new ArgumentException(
                    "ArtikalId mora biti veći od nule."
                );
            }

            var akcija =
                await _akcijaRepo.GetPoslednjuZaArtikalAsync(
                    artikalId
                );

            if (akcija is null)
            {
                return null;
            }

            return MapToDto(
                akcija
            );
        }

        public async Task<AkcijaDTO?> DodajAkciju(
            DodajAkcijuDTO dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(
                    nameof(dto)
                );
            }

            if (string.IsNullOrWhiteSpace(
                dto.SifraArtikla))
            {
                throw new ArgumentException(
                    "Šifra artikla je obavezna."
                );
            }

            if (dto.DatumDo < dto.DatumOd)
            {
                throw new ArgumentException(
                    "Datum završetka akcije ne može biti pre datuma početka."
                );
            }

            if (dto.AkcijskaCena <= 0)
            {
                throw new ArgumentException(
                    "Akcijska cena mora biti veća od nule."
                );
            }

            if (dto.TipAkcijeId <= 0)
            {
                throw new ArgumentException(
                    "Tip akcije je obavezan."
                );
            }

            var artikal =
                await _artikalRepo.GetBySifraAsync(
                    dto.SifraArtikla.Trim(),
                    true,
                    new List<int>()
                );

            if (artikal is null)
            {
                return null;
            }

            var novaAkcija =
                new Akcija
                {
                    ArtikalId =
                        artikal.ArtikalId,

                    DatumOd =
                        dto.DatumOd.ToDateTime(
                            TimeOnly.MinValue
                        ),

                    DatumDo =
                        dto.DatumDo.ToDateTime(
                            TimeOnly.MinValue
                        ),

                    AkcijskaCena =
                        dto.AkcijskaCena,

                    TipAkcijeId =
                        dto.TipAkcijeId
                };

            var rezultat =
                await _akcijaRepo.DodajAkciju(
                    novaAkcija
                );

            if (rezultat is null)
            {
                return null;
            }

            var detalji =
                await _akcijaRepo.GetByIdAsync(
                    rezultat.AkcijaId
                );

            if (detalji is null)
            {
                return new AkcijaDTO
                {
                    AkcijaId =
                        rezultat.AkcijaId,

                    ArtikalId =
                        rezultat.ArtikalId,

                    DatumOd =
                        rezultat.DatumOd,

                    DatumDo =
                        rezultat.DatumDo,

                    AkcijskaCena =
                        rezultat.AkcijskaCena,

                    TipAkcijeId =
                        rezultat.TipAkcijeId,

                    ActualPromet =
                        0,

                    ActualRUC12 =
                        0,

                    ActualRUC12Procenat =
                        0
                };
            }

            return MapToDto(
                detalji
            );
        }

        public async Task<IEnumerable<TipAkcije>>
            GetAktivniTipoviAkcije()
        {
            return await _akcijaRepo
                .GetAktivniTipoviAkcije();
        }

        private static AkcijaDTO MapToDto(
            AkcijaDetalji akcija)
        {
            return new AkcijaDTO
            {
                AkcijaId =
                    akcija.AkcijaId,

                ArtikalId =
                    akcija.ArtikalId,

                DatumOd =
                    akcija.DatumOd,

                DatumDo =
                    akcija.DatumDo,

                AkcijskaCena =
                    akcija.AkcijskaCena,

                TipAkcije =
                    akcija.TipAkcije,

                TipAkcijeId =
                    akcija.TipAkcijeId,

                ActualPromet =
                    akcija.ActualPromet,

                ActualRUC12 =
                    akcija.ActualRUC12,

                ActualRUC12Procenat =
                    akcija.ActualRUC12Procenat
            };
        }
    }
}