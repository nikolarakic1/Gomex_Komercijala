using GomexPraksa.ServicesComerc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DtosComerc;

namespace GomexPraksa.Controllers
{
    [Authorize(Roles = "Menadzer,SefMenadzera")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDTO>> GetSummary(
            [FromQuery] DashboardFilterDTO filterDTO)
        {
            try
            {
                var rezultat =
                    await _dashboardService.FillCardsAsync(
                        filterDTO
                    );

                return Ok(rezultat);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    "Greška prilikom obrade zahteva."
                );
            }
        }
    }
}