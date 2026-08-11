using GomexPraksa.ServicesComerc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Models.Dtos;
using Models.DtosComerc;

namespace GomexPraksa.Controllers
{
    [ApiController]
    [Route("api/RucChangeTracker")]
    public class RucChangeTrackerController : Controller
    {
        private readonly IRucChangeService _serivce;
        public RucChangeTrackerController(IRucChangeService service)
        {
            _serivce = service;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RucChangeDTO>>> CheckInfoForChangesAsync(DateOnly datumOd,
        DateOnly? datumDo,
        DateOnly? prethodniDatumOd,
        DateOnly? prethodniDatumDo)
        {
            var checkinfo = await _serivce.CheckInfoForChangesAsync(datumOd, datumDo, prethodniDatumOd, prethodniDatumDo);
            if(checkinfo is null)
            {
                return BadRequest();
            }
            return Ok(checkinfo);
        }
    }
}
