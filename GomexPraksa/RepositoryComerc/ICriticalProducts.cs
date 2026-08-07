using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public interface ICriticalProducts
    {
        Task<IEnumerable<CriticalProductsDTO>> ShowCriticalProductsAsync();
        Task<IEnumerable<CriticalProductsDTO>> CriticalProductsPage();
    }
}
