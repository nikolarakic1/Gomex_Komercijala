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
        DateOnly datumOd,
        DateOnly? datumDo,
        DateOnly? prethodniDatumOd,
        DateOnly? prethodniDatumDo)
    {
        DateOnly danas =
            DateOnly.FromDateTime(DateTime.Now);

        if (!datumDo.HasValue)
        {
            throw new ArgumentException(
                "DatumDo je obavezan.");
        }

        if (datumOd > datumDo.Value)
        {
            throw new ArgumentOutOfRangeException(
                nameof(datumOd),
                "DatumOd ne može biti posle DatumDo.");
        }

        if (datumOd > danas)
        {
            throw new ArgumentOutOfRangeException(
                nameof(datumOd),
                "DatumOd ne može biti u budućnosti.");
        }

        if (datumDo.Value > danas)
        {
            throw new ArgumentOutOfRangeException(
                nameof(datumDo),
                "DatumDo ne može biti u budućnosti.");
        }

        if (prethodniDatumOd.HasValue !=
            prethodniDatumDo.HasValue)
        {
            throw new ArgumentException(
                "Moraju biti uneta oba datuma prethodnog perioda.");
        }

        if (prethodniDatumOd.HasValue &&
            prethodniDatumOd.Value >
            prethodniDatumDo!.Value)
        {
            throw new ArgumentException(
                "Početak prethodnog perioda ne može biti posle njegovog kraja.");
        }

        var access =
            await _userAccess.GetCurrentUserAccessAsync();

        if (!access.CanViewAllCategories &&
            access.KategorijaIds.Count == 0)
        {
            throw new UnauthorizedAccessException(
                "Korisniku nije dodeljena nijedna kategorija.");
        }

        var rezultat =
            await _repo.CheckInfoForChangesAsync(
                datumOd,
                datumDo,
                prethodniDatumOd,
                prethodniDatumDo,
                access.CanViewAllCategories,
                access.KategorijaIds
            );

        return rezultat;
    }
}