using Models.Dtos;

namespace GomexPraksa.Services
{
    public interface IArtikalService
    {
        Task<IEnumerable<ArtikalDto>> GetAllAsync();

        Task<ArtikalDto> GetByIdAsync(int id);

        Task<ArtikalDto> GetBySifraAsync(string sifra);

        Task<IEnumerable<ArtikalDto>> SearchAsync(
            ArtikalFilterDto filter);
    }
}
