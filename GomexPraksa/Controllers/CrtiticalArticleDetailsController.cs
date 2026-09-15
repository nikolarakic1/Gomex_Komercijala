using GomexPraksa.AddedFunctions;
using Microsoft.AspNetCore.Mvc;

namespace GomexPraksa.Controllers
{
    [ApiController]
    [Route("api/artikli")]
    public class CriticalArticleDetailsController
        : ControllerBase
    {
        private readonly ICriticalArticleDetailsService _service;

        public CriticalArticleDetailsController(
            ICriticalArticleDetailsService service)
        {
            _service = service;
        }

        [HttpGet("{sifra}/critical-details")]
        public async Task<IActionResult> GetCriticalDetails(
            string sifra,
            [FromQuery] int godinaOd = 2025,
            [FromQuery] int godinaDo = 2026)
        {
            try
            {
                var rezultat =
                    await _service.GetDetailsAsync(
                        sifra,
                        godinaOd,
                        godinaDo
                    );

                if (rezultat == null)
                {
                    return NotFound(
                        new
                        {
                            message =
                                $"Artikal sa šifrom {sifra} nije pronađen."
                        }
                    );
                }

                return Ok(rezultat);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new
                    {
                        message = ex.Message
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Greška pri učitavanju detalja kritičnog artikla.",
                        innerMessage =
                            ex.Message
                    }
                );
            }
        }
    }
}