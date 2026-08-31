using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public interface ICriticalProducts
    {
        Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop5(
            DashboardFilterDTO filter,
            DateOnly datumOd,
            DateOnly datumDo,
            bool CanAllViewCategories,
            List<int> KategorijaIds);

        Task<IEnumerable<CriticalProductsPageDTO>> ShowCriticalProductsAsync(
            FilterSharedPages filter , bool CanViewAllCategories,List<int> KategorijaIds);
        
    }
}