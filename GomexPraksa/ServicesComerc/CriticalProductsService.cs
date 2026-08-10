using GomexPraksa.RepositoryComerc;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc
{
    public class CriticalProductsService : ICriticalProductsService
    {
        private readonly ICriticalProducts _repo;
        public CriticalProductsService(ICriticalProducts repo)
        {
            _repo = repo;
        }
        public Task<CriticalProductsDTO> CriticalProductsPage()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop(DateOnly datumOd, DateOnly datumDo)
        {
            var Danas = DateOnly.FromDateTime(DateTime.Now);
            if (datumOd > datumDo)
            {
                throw new Exception("Greska u biranju datuma");
            }
            return await _repo.CriticalProductsTop5(datumOd, datumDo);
        }
    }
}
