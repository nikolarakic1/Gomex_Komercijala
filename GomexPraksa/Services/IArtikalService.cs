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

        public Task<PaginationGeneric<ArtikalDto>> SearchAsync(
    ArtikalFilterDto filter,
    PaginationParams pagination);
    }
}
