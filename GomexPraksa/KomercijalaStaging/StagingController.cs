using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GomexPraksa.KomercijalaStaging
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
                return BadRequest(new
                {
                    Message = "Excel fajl nije prosleđen."
                });
            }

            if (file.Length == 0)
            {
                return BadRequest(new
                {
                    Message = "Excel fajl je prazan."
                });
            }

            try
            {
                var result =
                    await _stagingService
                        .ImportExcelAsync(file);

                return Ok(new
                {
                    Message = "Import uspešno završen.",
                    result.ImportBatchId,
                    result.InsertedRows,
                    result.Status
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    InnerMessage =
                        ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = ex.Message,
                        InnerMessage =
                            ex.InnerException?.Message,
                        ExceptionType =
                            ex.GetType().Name
                    });
            }
        }
    }
}