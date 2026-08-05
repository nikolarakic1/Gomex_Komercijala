using GomexPraksa.Services;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace GomexPraksa.Controllers
{
    [ApiController]
    [Route("api/artikli")]
    public class ArtikliController : ControllerBase
    {
        private readonly IArtikalService _artikalService;

        public ArtikliController(IArtikalService artikalService)
        {
            _artikalService = artikalService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtikalDto>>> GetAll()
        {
            var artikli = await _artikalService.GetAllAsync();

            return Ok(artikli);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ArtikalDto>> GetById(int id)
        {
            var artikal = await _artikalService.GetByIdAsync(id);

            return Ok(artikal);
        }

        [HttpGet("sifra/{sifra}")]
        public async Task<ActionResult<ArtikalDto>> GetBySifra(
            string sifra)
        {
            var artikal =
                await _artikalService.GetBySifraAsync(sifra);

            return Ok(artikal);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ArtikalDto>>> Search(
            [FromQuery] ArtikalFilterDto filter)
        {
            var artikli =
                await _artikalService.SearchAsync(filter);

            return Ok(artikli);
        }
    }
}
