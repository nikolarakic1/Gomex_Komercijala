using GomexPraksa.JWTInfo;
using GomexPraksa.RepositoryComerc;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

public class RucChangeService : IRucChangeService
{
    private readonly IRucChangeTracker _repo;
    private readonly IUserAccess _userAccess;

    public RucChangeService(
        IRucChangeTracker repo,
        IUserAccess userAccess)
    {
        _repo = repo;
        _userAccess = userAccess;
    }

    public async Task<RucChangeDTO> CheckInfoForChangesAsync(
        DashboardFilterDTO filter)
    {
        // =============================================
        // DATUMI
        // =============================================

        if (!filter.DatumOd.HasValue)
        {
            throw new ArgumentException(
                "DatumOd je obavezan."
            );
        }

        if (!filter.DatumDo.HasValue)
        {
            throw new ArgumentException(
                "DatumDo je obavezan."
            );
        }

        var datumOd =
            filter.DatumOd.Value;

        var datumDo =
            filter.DatumDo.Value;

        var danas =
            DateOnly.FromDateTime(
                DateTime.Today
            );

        if (datumOd > datumDo)
        {
            throw new ArgumentException(
                "DatumOd ne može biti posle DatumDo."
            );
        }

        if (datumOd > danas)
        {
            throw new ArgumentException(
                "DatumOd ne može biti u budućnosti."
            );
        }

        if (datumDo > danas)
        {
            throw new ArgumentException(
                "DatumDo ne može biti u budućnosti."
            );
        }

        // =============================================
        // FILTER VALIDACIJA
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

        // =============================================
        // NAPOMENA:
        //
        // TipProdajeId se ovde NAMERNO ne validira.
        //
        // RUC waterfall interno koristi:
        // 6 = ACTUAL
        // 7 = PLAN
        //
        // Zato eksterni TipProdaje filter nema smisla
        // za ovaj endpoint.
        // =============================================

        // =============================================
        // USER ACCESS
        // =============================================

        var access =
            await _userAccess
                .GetCurrentUserAccessAsync();

        if (!access.CanViewAllCategories &&
            (
                access.KategorijaIds == null ||
                access.KategorijaIds.Count == 0
            ))
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija."
            );
        }

        // =============================================
        // REPOSITORY
        // =============================================

        return await _repo
            .CheckInfoForChangesAsync(
                filter,
                access.CanViewAllCategories,
                access.KategorijaIds
            );
    }
}