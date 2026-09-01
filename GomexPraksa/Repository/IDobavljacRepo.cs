using GomexPraksa.AddedFunctions;
using Models.ModelsDash;

namespace GomexPraksa.Repository
{
    public interface IDobavljacRepo
    {
        Task<PaginationGeneric<Dobavljac>> GetAllDobavljace(
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams pagination);

        Task<Dobavljac?> GetByIdAsync(
            int id,
            bool canViewAllCategories,
            List<int> kategorijaIds);

        Task<PaginationGeneric<Dobavljac>> SearchAsync(
            string? naziv,
            bool? aktivan,
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams pagination);

        Task<PaginationGeneric<Dobavljac>> CriticalDobavljaciAsync(
            bool canViewAllCategories,
            List<int> kategorijaIds,
            PaginationParams pagination);
    }
}