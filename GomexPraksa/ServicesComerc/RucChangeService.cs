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
        DashboardFilterDTO filter,
        DateOnly prethodniDatumOd,
        DateOnly prethodniDatumDo)
    {
        if (!filter.DatumOd.HasValue)
        {
            throw new ArgumentException(
                "DatumOd je obavezan.");
        }

        if (!filter.DatumDo.HasValue)
        {
            throw new ArgumentException(
                "DatumDo je obavezan.");
        }

        var datumOd =
            filter.DatumOd.Value;

        var datumDo =
            filter.DatumDo.Value;

        var danas =
            DateOnly.FromDateTime(
                DateTime.Now);

        if (datumOd > datumDo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(filter.DatumOd),
                "DatumOd ne može biti posle DatumDo.");
        }

        if (datumOd > danas)
        {
            throw new ArgumentOutOfRangeException(
                nameof(filter.DatumOd),
                "DatumOd ne može biti u budućnosti.");
        }

        if (datumDo > danas)
        {
            throw new ArgumentOutOfRangeException(
                nameof(filter.DatumDo),
                "DatumDo ne može biti u budućnosti.");
        }

        if (prethodniDatumOd >
            prethodniDatumDo)
        {
            throw new ArgumentException(
                "Početak prethodnog perioda ne može biti posle njegovog kraja.");
        }

        if (filter.OdeljenjeId.HasValue &&
            filter.OdeljenjeId.Value <= 0)
        {
            throw new ArgumentException(
                "OdeljenjeId nije validan.");
        }

        if (filter.KategorijaId.HasValue &&
            filter.KategorijaId.Value <= 0)
        {
            throw new ArgumentException(
                "KategorijaId nije validan.");
        }

        if (filter.DobavljacId.HasValue &&
            filter.DobavljacId.Value <= 0)
        {
            throw new ArgumentException(
                "DobavljacId nije validan.");
        }

        if (filter.TipProdajeId.HasValue &&
            filter.TipProdajeId.Value <= 0)
        {
            throw new ArgumentException(
                "TipProdajeId nije validan.");
        }

        var access =
            await _userAccess
                .GetCurrentUserAccessAsync();

        if (!access.CanViewAllCategories &&
            access.KategorijaIds.Count == 0)
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija.");
        }

        return await _repo
            .CheckInfoForChangesAsync(
                filter,
                prethodniDatumOd,
                prethodniDatumDo,
                access.CanViewAllCategories,
                access.KategorijaIds
            );
    }
}