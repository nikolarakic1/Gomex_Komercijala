using Models.DtosComerc;
using Models.ModelsDash;

namespace GomexPraksa.RepositoryComerc
{
    public interface IRucChangeTracker
    {
        public async Task<RucChangeDTO> CheckInfoForChangesAsync(
       DateOnly datumOd,
       DateOnly datumDo,
       DateOnly prethodniDatumOd,
       DateOnly prethodniDatumDo)
    }
}
