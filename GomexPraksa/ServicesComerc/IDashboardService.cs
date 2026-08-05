using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc
{
    public interface IDashboardService
    {
        public Task<DashboardSummaryDTO> FillCardsAsync(DashboardFilterDTO filterDTO);
    }
}
