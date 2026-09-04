using GomexPraksa.AddedFunctions;
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

        // =============================================
        // CRITICAL PRODUCTS PAGE
        // =============================================

        public async Task<
            PaginationGeneric<CriticalProductsPageDTO>>
            CriticalProductsPage(
                FilterSharedPages filter,
                PaginationParams pagination)
        {
            // =============================================
            // VALIDACIJA PERIODA
            // =============================================

            if (filter.DatumOd == default ||
                filter.DatumDo == default)
            {
                throw new ArgumentException(
                    "Period je obavezan."
                );
            }

            if (filter.DatumDo < filter.DatumOd)
            {
                throw new ArgumentException(
                    "Datum završetka ne može biti " +
                    "pre datuma početka."
                );
            }

            if (filter.DatumDo >
                DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException(
                    "Datum završetka ne može biti " +
                    "u budućnosti."
                );
            }

            // =============================================
            // VALIDACIJA FILTERA
            // =============================================

            if (filter.OdeljenjeId.HasValue &&
                filter.OdeljenjeId.Value <= 0)
            {
                throw new ArgumentException(
                    "OdeljenjeId nije validan."
                );
            }

            if (filter.KategorijaId.HasValue &&
                filter.KategorijaId.Value <= 0)
            {
                throw new ArgumentException(
                    "KategorijaId nije validan."
                );
            }

            if (filter.DobavljacId.HasValue &&
                filter.DobavljacId.Value <= 0)
            {
                throw new ArgumentException(
                    "DobavljacId nije validan."
                );
            }

            if (filter.TipProdajeId.HasValue &&
                filter.TipProdajeId.Value <= 0)
            {
                throw new ArgumentException(
                    "TipProdajeId nije validan."
                );
            }

            // =============================================
            // USER ACCESS
            // =============================================

            var access =
                await _userAccess
                    .GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena " +
                    "nijedna kategorija."
                );
            }

            // =============================================
            // REPOSITORY
            // =============================================

            return await _repo
                .ShowCriticalProductsAsync(
                    filter,
                    pagination,
                    access.CanViewAllCategories,
                    access.KategorijaIds
                );
        }

        // =============================================
        // TOP 5 CRITICAL PRODUCTS
        // =============================================

        public async Task<IEnumerable<CriticalProductsDTO>>
            CriticalProductsTop(
                DashboardFilterDTO filter)
        {
            if (!filter.DatumOd.HasValue ||
                !filter.DatumDo.HasValue)
            {
                throw new ArgumentException(
                    "Period je obavezan."
                );
            }

            var datumOd =
                filter.DatumOd.Value;

            var datumDo =
                filter.DatumDo.Value;

            if (datumOd > datumDo)
            {
                throw new ArgumentException(
                    "Datum početka ne može biti " +
                    "posle datuma završetka."
                );
            }

            if (datumDo >
                DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException(
                    "Datum završetka ne može biti " +
                    "u budućnosti."
                );
            }

            // =============================================
            // VALIDACIJA FILTERA
            // =============================================

            if (filter.OdeljenjeId.HasValue &&
                filter.OdeljenjeId.Value <= 0)
            {
                throw new ArgumentException(
                    "OdeljenjeId nije validan."
                );
            }

            if (filter.KategorijaId.HasValue &&
                filter.KategorijaId.Value <= 0)
            {
                throw new ArgumentException(
                    "KategorijaId nije validan."
                );
            }

            if (filter.DobavljacId.HasValue &&
                filter.DobavljacId.Value <= 0)
            {
                throw new ArgumentException(
                    "DobavljacId nije validan."
                );
            }

            if (filter.TipProdajeId.HasValue &&
                filter.TipProdajeId.Value <= 0)
            {
                throw new ArgumentException(
                    "TipProdajeId nije validan."
                );
            }

            // =============================================
            // USER ACCESS
            // =============================================

            var access =
                await _userAccess
                    .GetCurrentUserAccessAsync();

            if (!access.CanViewAllCategories &&
                access.KategorijaIds.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Korisniku nije dodeljena " +
                    "nijedna kategorija."
                );
            }

            // =============================================
            // REPOSITORY
            // =============================================

            return await _repo
                .CriticalProductsTop5(
                    filter,
                    datumOd,
                    datumDo,
                    access.CanViewAllCategories,
                    access.KategorijaIds
                );
        }
    }
}