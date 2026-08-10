using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public interface ICriticalProducts
    {
        Task<IEnumerable<CriticalProductsDTO>> ShowCriticalProductsAsync(FilterSharedPages filter);
        public Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop5(
     DateOnly datumOd,
     DateOnly datumDo);
    }
}
