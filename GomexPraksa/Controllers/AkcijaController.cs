using GomexPraksa.Services;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace GomexPraksa.Controllers;

[ApiController]
[Route("api/akcije")]
public class AkcijeController : ControllerBase
{
    private readonly IAkcijaService _akcijaService;

    public AkcijeController(IAkcijaService akcijaService)
    {
        _akcijaService = akcijaService;
    }

    // GET: api/akcije
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AkcijaDTO>>> GetAllAsync()
    {
        var akcije = await _akcijaService.GetAllAsync();

        return Ok(akcije);
    }

    // GET: api/akcije/buduce
    [HttpGet("buduce")]
    public async Task<ActionResult<IEnumerable<AkcijaDTO>>> GetBuduceAsync()
    {
        var akcije = await _akcijaService.GetBuduceAsync();

        return Ok(akcije);
    }

    // GET: api/akcije/trenutne
    [HttpGet("trenutne")]
    public async Task<ActionResult<IEnumerable<AkcijaDTO>>> GetTrenutneAsync()
    {
        var akcije = await _akcijaService.GetTrenutneAsync();

        return Ok(akcije);
    }

    // GET: api/akcije/artikal/15
    [HttpGet("artikal/{artikalId:int}")]
    public async Task<ActionResult<IEnumerable<AkcijaDTO>>> GetByArtikalIdAsync(
        int artikalId)
    {
        if (artikalId <= 0)
        {
            return BadRequest(
                "ID artikla mora biti veći od nule.");
        }

        var akcije =
            await _akcijaService.GetByArtikalIdAsync(artikalId);

        return Ok(akcije);
    }

    // GET: api/akcije/artikal/15/poslednja
    [HttpGet("artikal/{artikalId:int}/poslednja")]
    public async Task<ActionResult<AkcijaDTO>> GetPoslednjuZaArtikalAsync(
        int artikalId)
    {
        if (artikalId <= 0)
        {
            return BadRequest(
                "ID artikla mora biti veći od nule.");
        }

        var poslednjaAkcija =
            await _akcijaService.GetPoslednjuZaArtikalAsync(artikalId);

        if (poslednjaAkcija is null)
        {
            return NotFound(
                $"Nije pronađena akcija za artikal sa ID-em {artikalId}.");
        }

        return Ok(poslednjaAkcija);
    }
}