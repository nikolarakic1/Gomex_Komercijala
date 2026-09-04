using GomexPraksa.AddedFunctions;
using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

public interface ICriticalProductsService
{
    Task<IEnumerable<CriticalProductsDTO>>
        CriticalProductsTop(
            DashboardFilterDTO filter);

    Task<PaginationGeneric<CriticalProductsPageDTO>>
        CriticalProductsPage(
            FilterSharedPages filter,
            PaginationParams pagination);
}