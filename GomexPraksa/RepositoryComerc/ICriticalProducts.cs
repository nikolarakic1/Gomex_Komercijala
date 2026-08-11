using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public interface ICriticalProducts
    {
        Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop5(
            DateOnly datumOd,
            DateOnly datumDo);

        Task<IEnumerable<CriticalProductsPageDTO>> ShowCriticalProductsAsync(
            FilterSharedPages filter);
    }
}