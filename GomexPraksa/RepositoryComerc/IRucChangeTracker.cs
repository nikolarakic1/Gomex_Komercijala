using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc;

public interface IRucChangeTracker
{
    Task<RucChangeDTO> CheckInfoForChangesAsync(
        DashboardFilterDTO filter,
        bool canViewAllCategories,
        List<int> kategorijaIds
    );
}