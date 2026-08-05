using Models.Dtos;

namespace GomexPraksa.Services;

public interface IDobavljacServis
{
    Task<IEnumerable<DobavljacDTO>> GetAllDobavljaceAsync();

    Task<DobavljacDTO?> GetByIdAsync(int id);

    Task<IEnumerable<DobavljacDTO>> SearchAsync(
        string? naziv,
        bool? aktivan);
}