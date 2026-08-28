using GomexPraksa.AddedFunctions;
using Models.Dtos;

namespace GomexPraksa.Services
{
    public interface IArtikalService
    {
        public Task<PaginationGeneric<ArtikalDto>> GetAllAsync(
    PaginationParams pagination);

        Task<ArtikalDto> GetByIdAsync(int id);

        Task<ArtikalDto> GetBySifraAsync(string sifra);

        Task<IEnumerable<ArtikalDto>> SearchAsync(
            ArtikalFilterDto filter);
    }
}
