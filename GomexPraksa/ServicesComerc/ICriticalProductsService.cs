using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

    public interface ICriticalProductsService
    {
        public Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop(DateOnly datumOd,DateOnly datumDo);
    public Task<CriticalProductsDTO> CriticalProductsPage();
    }

