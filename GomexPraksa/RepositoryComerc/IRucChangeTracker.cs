using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc;

public interface IRucChangeTracker
{
    Task<RucChangeDTO> CheckInfoForChangesAsync(
        DashboardFilterDTO filter,
        DateOnly prethodniDatumOd,
        DateOnly prethodniDatumDo,
        bool canViewAllCategories,
        List<int> kategorijaIds
    );
}