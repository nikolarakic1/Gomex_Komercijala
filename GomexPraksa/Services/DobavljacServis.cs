using GomexPraksa.Repository;
using Models.Dtos;
using Models.ModelsDash;

namespace GomexPraksa.Services;

public class DobavljacServis : IDobavljacServis
{
    private readonly IDobavljacRepo _repo;

    public DobavljacServis(IDobavljacRepo repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<DobavljacDTO>> GetAllDobavljaceAsync()
    {
        var dobavljaci = await _repo.GetAllDobavljace();

        return dobavljaci.Select(MapToDto);
    }

    public async Task<DobavljacDTO?> GetByIdAsync(int id)
    {
        var dobavljac = await _repo.GetByIdAsync(id);

        if (dobavljac is null)
            return null;

        return MapToDto(dobavljac);
    }

    public async Task<IEnumerable<DobavljacDTO>> SearchAsync(
        string? naziv,
        bool? aktivan)
    {
        var dobavljaci = await _repo.SearchAsync(naziv, aktivan);

        return dobavljaci.Select(MapToDto);
    }

    private static DobavljacDTO MapToDto(Dobavljac dobavljac) =>
        new()
        {
            DobavljacId = dobavljac.DobavljacId,
            Naziv = dobavljac.Naziv,
            Aktivan = dobavljac.Aktivan
        };
}