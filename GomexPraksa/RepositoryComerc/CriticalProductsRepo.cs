using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public class CriticalProductsRepo : ICriticalProducts
    {
        public Task<IEnumerable<CriticalProductsDTO>> CriticalProductsPage()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CriticalProductsDTO>> ShowCriticalProductsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
