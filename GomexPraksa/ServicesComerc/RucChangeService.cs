using GomexPraksa.RepositoryComerc;
using Models.DtosComerc;
using System.Runtime.CompilerServices;

namespace GomexPraksa.ServicesComerc;

public class RucChangeService : IRucChangeService
{
    private readonly IRucChangeTracker _repo;
    public RucChangeService(IRucChangeTracker repo)
    {
        _repo = repo;
    }
    public async Task<RucChangeDTO> CheckInfoForChangesAsync(DateOnly datumOd, DateOnly? datumDo, DateOnly? prethodniDatumOd, DateOnly? prethodniDatumDo)
    {
        DateOnly danas = DateOnly.FromDateTime(DateTime.Now);
        if (datumOd > datumDo)
        {
            throw new ArgumentOutOfRangeException("ne moze datum danas biti veci od datuma do");
        }
        if(datumDo < datumOd)
        {
            throw new ArgumentOutOfRangeException("ne moze datum do biti manji od datuma od");
        }
        if(datumOd > danas)
        {
            throw new ArgumentOutOfRangeException("ne moze datumod biti veci od danas");
        }
        
        var checkInfoForChanges = await _repo.CheckInfoForChangesAsync(datumOd,datumDo,prethodniDatumOd,prethodniDatumDo);
        return checkInfoForChanges;
    }
}

