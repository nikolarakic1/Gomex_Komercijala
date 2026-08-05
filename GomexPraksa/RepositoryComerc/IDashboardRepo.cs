using Models.DtosComerc;

namespace GomexPraksa.RepositoryComerc
{
    public interface IDashboardRepo
    {
        public Task<DashboardSummaryDTO> FillCardsAsync(DashboardFilterDTO filterDTO);
    }
}
