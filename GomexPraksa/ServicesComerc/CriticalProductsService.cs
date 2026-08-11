using GomexPraksa.RepositoryComerc;
using Models.Dtos;
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

        public async Task<IEnumerable<CriticalProductsPageDTO>> CriticalProductsPage(
            FilterSharedPages filter)
        {
            if (filter.DatumOd == default || filter.DatumDo == default)
                throw new ArgumentException("Period je obavezan.");

            if (filter.DatumDo < filter.DatumOd)
                throw new ArgumentException(
                    "Datum završetka ne može biti pre datuma početka.");

            if (filter.DatumDo > DateTime.Today)
                throw new ArgumentException(
                    "Datum završetka ne može biti u budućnosti.");

            return await _repo.ShowCriticalProductsAsync(filter);
        }

        public async Task<IEnumerable<CriticalProductsDTO>> CriticalProductsTop(
            DateOnly datumOd,
            DateOnly datumDo)
        {
            if (datumOd > datumDo)
            {
                throw new ArgumentException(
                    "Datum početka ne može biti posle datuma završetka.");
            }

            return await _repo.CriticalProductsTop5(datumOd, datumDo);
        }
    }
}
