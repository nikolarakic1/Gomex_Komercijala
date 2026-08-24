using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDTO> FillCardsAsync(
            DashboardFilterDTO filterDTO,
            string userId,
            bool isSef
        );
    }
}