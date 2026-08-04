using Models.Dtos;

namespace GomexPraksa.Services;

public interface IAkcijaService
{
    Task<IEnumerable<AkcijaDTO>> GetAllAsync();

    Task<AkcijaDTO> GetByIdAsync(int id);

    Task<IEnumerable<AkcijaDTO>> GetBuduceAsync();

    Task<IEnumerable<AkcijaDTO>> GetTrenutneAsync();

    Task<IEnumerable<AkcijaDTO>> GetByArtikalIdAsync(int artikalId);

    Task<AkcijaDTO?> GetPoslednjuZaArtikalAsync(int artikalId);
}