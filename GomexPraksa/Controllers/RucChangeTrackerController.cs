using GomexPraksa.ServicesComerc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DtosComerc;

namespace GomexPraksa.Controllers
{
    [Authorize(Roles = "Menadzer,SefMenadzera")]
    [ApiController]
    [Route("api/RucChangeTracker")]
    public class RucChangeTrackerController : ControllerBase
    {
        private readonly IRucChangeService _service;

        public RucChangeTrackerController(
            IRucChangeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<RucChangeDTO>> CheckInfoForChangesAsync(
            [FromQuery] DashboardFilterDTO filter,
            [FromQuery] DateOnly prethodniDatumOd,
            [FromQuery] DateOnly prethodniDatumDo)
        {
            try
            {
                var rezultat =
                    await _service.CheckInfoForChangesAsync(
                        filter,
                        prethodniDatumOd,
                        prethodniDatumDo
                    );

                return Ok(rezultat);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
           
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    "Greška prilikom obrade RUC podataka."
                );
            }
        }
    }
}