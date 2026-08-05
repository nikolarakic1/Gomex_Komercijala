using GomexPraksa.RepositoryComerc;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepo _repo;
        public DashboardService(IDashboardRepo repo)
        {
            _repo = repo;
        }
        public async Task<DashboardSummaryDTO> FillCardsAsync(DashboardFilterDTO filterDTO)
        {
            {
                if (filterDTO.NedeljaOd > filterDTO.NedeljaDo)
                {
                    throw new ArgumentException(
                        "Početna nedelja ne može biti veća od završne.");
                }

                int brojNedelja =
                    filterDTO.NedeljaDo - filterDTO.NedeljaOd + 1;

                int prethodnaNedeljaDo =
                    filterDTO.NedeljaOd - 1;

                int prethodnaNedeljaOd =
                    prethodnaNedeljaDo - brojNedelja + 1;

                return await _repo.FillCardsAsync(
                    filterDTO);
            }

        }
    }
}
