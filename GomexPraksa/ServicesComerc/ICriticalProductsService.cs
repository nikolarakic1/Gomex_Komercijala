using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

public interface ICriticalProductsService
{
    Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop(
        DashboardFilterDTO filter);

    Task<IEnumerable<CriticalProductsPageDTO>> CriticalProductsPage(
        FilterSharedPages filter);
}