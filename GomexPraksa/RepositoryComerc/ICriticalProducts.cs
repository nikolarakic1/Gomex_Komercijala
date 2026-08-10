using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public interface ICriticalProducts
    {
        Task<IEnumerable<CriticalProductsDTO>> ShowCriticalProductsAsync();
        public Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop5(
     DateOnly datumOd,
     DateOnly datumDo);
    }
}
