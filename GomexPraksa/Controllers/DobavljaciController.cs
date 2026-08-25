using GomexPraksa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace GomexPraksa.Controllers;

[Authorize(Roles = "Menadzer,SefMenadzera")]
[ApiController]
[Route("api/dobavljaci")]
public class DobavljaciController : ControllerBase
{
    private readonly IDobavljacServis _dobavljacServis;

    public DobavljaciController(IDobavljacServis dobavljacServis)
    {
        _dobavljacServis = dobavljacServis;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DobavljacDTO>>> GetAll()
    {
        var dobavljaci =
            await _dobavljacServis.GetAllDobavljaceAsync();

        return Ok(dobavljaci);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DobavljacDTO>> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("ID dobavljača mora biti veći od nule.");
        }

        var dobavljac =
            await _dobavljacServis.GetByIdAsync(id);

        if (dobavljac is null)
        {
            return NotFound(
                $"Dobavljač sa ID-em {id} nije pronađen.");
        }

        return Ok(dobavljac);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<DobavljacDTO>>> Search(
        [FromQuery] string? naziv,
        [FromQuery] bool? aktivan)
    {
        var dobavljaci =
            await _dobavljacServis.SearchAsync(naziv, aktivan);

        return Ok(dobavljaci);
    }
}