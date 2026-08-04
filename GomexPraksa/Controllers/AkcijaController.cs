using GomexPraksa.Services;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace GomexPraksa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AkcijaController : Controller
    {
        private readonly IAkcijaService _akcijaService;
        public AkcijaController(IAkcijaService akcijaService)
        {
            _akcijaService = akcijaService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AkcijaDTO>>> GetAllAsync()
        {
            var getAll = await _akcijaService.GetAllAsync();
            if(getAll is null)
            {
                return BadRequest();
            }
            return Ok(getAll);
            
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AkcijaDTO>>> GetBuduceAsync()
        {
            var getBuduce = await _akcijaService.GetBuduceAsync();
            if(getBuduce is null)
            {
                return BadRequest();
            }
            return Ok(getBuduce);
        }
        [HttpGet]
        public async Task<ActionResult<AkcijaDTO>> GetTrenutneAsync()
        {
            var getTrenutne = await _akcijaService.GetTrenutneAsync();
            if(getTrenutne is null)
            {
                return BadRequest();
            }
            return Ok(getTrenutne);
        }
        [HttpGet]
        public async Task<IActionResult> GetByArtikalIdAsync(int id)
        {
            var getArtikalPoIdu = await _akcijaService.GetByArtikalIdAsync(id);
            if(getArtikalPoIdu is null)
            {
                return BadRequest();
            }
            return Ok(getArtikalPoIdu);
        }
        



    }
}
