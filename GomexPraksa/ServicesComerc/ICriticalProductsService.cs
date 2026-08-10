using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

    public interface ICriticalProductsService
    {
    Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop(
    DateOnly datumOd,
    DateOnly datumDo);

    Task<IEnumerable<CriticalProductsPageDTO>> CriticalProductsPage(
        FilterSharedPages filter);
}

