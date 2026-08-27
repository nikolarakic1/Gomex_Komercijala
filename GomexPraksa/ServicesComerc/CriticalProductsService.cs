using GomexPraksa.JWTInfo;
using GomexPraksa.RepositoryComerc;
using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc
{
    public class CriticalProductsService : ICriticalProductsService
    {
        private readonly ICriticalProducts _repo;
        private readonly IUserAccess _userAccess;

        public CriticalProductsService(
            ICriticalProducts repo,
            IUserAccess userAccess)
        {
            _repo = repo;
            _userAccess = userAccess;
        }

        public async Task<IEnumerable<CriticalProductsPageDTO>>
            CriticalProductsPage(FilterSharedPages filter)
        {
            if (filter.DatumOd == default ||
                filter.DatumDo == default)
            {
                throw new ArgumentException(
                    "Period je obavezan.");
            }

            if (filter.DatumDo < filter.DatumOd)
            {
                throw new ArgumentException(
                    "Datum završetka ne može biti pre datuma početka.");
            }

            if (filter.DatumDo >
                DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException(
                    "Datum završetka ne može biti u budućnosti.");
            }

            var access =
                await _userAccess.GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena nijedna kategorija.");
            }

            return await _repo.ShowCriticalProductsAsync(
                filter,
                access.CanViewAllCategories,
                access.KategorijaIds
            );
        }

        public async Task<IEnumerable<CriticalProductsDTO>>
            CriticalProductsTop(
                DateOnly datumOd,
                DateOnly datumDo)
        {
            if (datumOd > datumDo)
            {
                throw new ArgumentException(
                    "Datum početka ne može biti posle datuma završetka.");
            }

            if (datumDo >
                DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException(
                    "Datum završetka ne može biti u budućnosti.");
            }

            var access =
                await _userAccess.GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena nijedna kategorija.");
            }

            return await _repo.CriticalProductsTop5(
                datumOd,
                datumDo,
                access.CanViewAllCategories,
                access.KategorijaIds
            );
        }
    }
}