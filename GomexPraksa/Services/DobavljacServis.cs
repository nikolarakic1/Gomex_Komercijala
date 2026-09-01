using GomexPraksa.AddedFunctions;
using GomexPraksa.JWTInfo;
using GomexPraksa.Repository;
using Models.Dtos;
using Models.ModelsDash;

namespace GomexPraksa.Services;

public class DobavljacServis : IDobavljacServis
{
    private readonly IDobavljacRepo _repo;
    private readonly IUserAccess _userAccess;

    public DobavljacServis(
        IDobavljacRepo repo,
        IUserAccess userAccess)
    {
        _repo = repo;
        _userAccess = userAccess;
    }

    public async Task<PaginationGeneric<DobavljacDTO>>
        GetAllDobavljaceAsync(
            PaginationParams pagination)
    {
        var access =
            await _userAccess.GetCurrentUserAccessAsync();

        if (!access.CanViewAllCategories &&
            access.KategorijaIds.Count == 0)
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija."
            );
        }

        var dobavljaci =
            await _repo.GetAllDobavljace(
                access.CanViewAllCategories,
                access.KategorijaIds,
                pagination
            );

        return new PaginationGeneric<DobavljacDTO>
        {
            Items =
                dobavljaci.Items
                    .Select(MapToDto)
                    .ToList(),

            Page =
                dobavljaci.Page,

            PageSize =
                dobavljaci.PageSize,

            TotalCount =
                dobavljaci.TotalCount
        };
    }

    public async Task<DobavljacDTO?> GetByIdAsync(
        int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "DobavljacId mora biti veći od nule."
            );
        }

        var access =
            await _userAccess.GetCurrentUserAccessAsync();

        if (!access.CanViewAllCategories &&
            access.KategorijaIds.Count == 0)
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija."
            );
        }

        var dobavljac =
            await _repo.GetByIdAsync(
                id,
                access.CanViewAllCategories,
                access.KategorijaIds
            );

        if (dobavljac is null)
        {
            return null;
        }

        return MapToDto(dobavljac);
    }

    public async Task<PaginationGeneric<DobavljacDTO>>
        SearchAsync(
            string? naziv,
            bool? aktivan,
            PaginationParams pagination)
    {
        var access =
            await _userAccess.GetCurrentUserAccessAsync();

        if (!access.CanViewAllCategories &&
            access.KategorijaIds.Count == 0)
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija."
            );
        }

        var dobavljaci =
            await _repo.SearchAsync(
                naziv,
                aktivan,
                access.CanViewAllCategories,
                access.KategorijaIds,
                pagination
            );

        return new PaginationGeneric<DobavljacDTO>
        {
            Items =
                dobavljaci.Items
                    .Select(MapToDto)
                    .ToList(),

            Page =
                dobavljaci.Page,

            PageSize =
                dobavljaci.PageSize,

            TotalCount =
                dobavljaci.TotalCount
        };
    }

    private static DobavljacDTO MapToDto(
        Dobavljac dobavljac)
    {
        return new DobavljacDTO
        {
            DobavljacId =
                dobavljac.DobavljacId,

            Naziv =
                dobavljac.Naziv,

            Aktivan =
                dobavljac.Aktivan
        };
    }
}