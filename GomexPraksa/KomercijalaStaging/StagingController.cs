using GomexPraksa.KomercijalaStaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GomexPraksa.Controllers
{
    [Authorize(Roles = "Menadzer,SefMenadzera")]
    [ApiController]
    [Route("api/staging")]
    public class StagingController : ControllerBase
    {
        private readonly IStagingService _stagingService;

        public StagingController(
            IStagingService stagingService)
        {
            _stagingService = stagingService;
        }

        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcel(
            IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(
                    "Excel fajl nije prosleđen.");
            }

            if (file.Length == 0)
            {
                return BadRequest(
                    "Excel fajl je prazan.");
            }

            try
            {
                var insertedRows =
                    await _stagingService
                        .ImportExcelAsync(file);

                return Ok(new
                {
                    Message = "Import uspešno završen.",
                    InsertedRows = insertedRows
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        Message =
                            "Došlo je do greške prilikom importa Excel fajla."
                    }
                );
            }
        }
    }
}