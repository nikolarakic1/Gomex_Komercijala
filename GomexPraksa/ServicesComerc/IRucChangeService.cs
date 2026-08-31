using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc
{
    public interface IRucChangeService
    {
        Task<RucChangeDTO> CheckInfoForChangesAsync(
            DashboardFilterDTO filter,
            DateOnly prethodniDatumOd,
            DateOnly prethodniDatumDo
        );
    }
}