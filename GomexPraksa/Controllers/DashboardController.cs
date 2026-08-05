using GomexPraksa.ServicesComerc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DtosComerc;

namespace GomexPraksa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDTO>> GetSummary(
            [FromQuery] DashboardFilterDTO filterDTO)
        {
            var rezultat = await _dashboardService.FillCardsAsync(filterDTO);

            return Ok(rezultat);
        }
    }
}
