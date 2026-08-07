using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

    public interface RucChangeService
    {
    public Task<RucChangeDTO> CheckInfoForChangesAsync(
       DateOnly datumOd,
       DateOnly datumDo,
       DateOnly prethodniDatumOd,
       DateOnly prethodniDatumDo);
}

