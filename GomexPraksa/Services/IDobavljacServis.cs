using GomexPraksa.AddedFunctions;
using Models.Dtos;

namespace GomexPraksa.Services;

public interface IDobavljacServis
{
    Task<PaginationGeneric<DobavljacDTO>>
        GetAllDobavljaceAsync(
            PaginationParams pagination);

    Task<DobavljacDTO?>
        GetByIdAsync(
            int id);

    Task<PaginationGeneric<DobavljacDTO>>
        SearchAsync(
            string? naziv,
            bool? aktivan,
            PaginationParams pagination);
}