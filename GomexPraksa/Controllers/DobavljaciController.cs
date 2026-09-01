using GomexPraksa.AddedFunctions;
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

    public DobavljaciController(
        IDobavljacServis dobavljacServis)
    {
        _dobavljacServis = dobavljacServis;
    }

    // =====================================================
    // GET ALL
    // =====================================================

    [HttpGet]
    public async Task<ActionResult<PaginationGeneric<DobavljacDTO>>> GetAll(
        [FromQuery] PaginationParams pagination)
    {
        var dobavljaci =
            await _dobavljacServis
                .GetAllDobavljaceAsync(
                    pagination
                );

        return Ok(dobavljaci);
    }

    // =====================================================
    // GET BY ID
    // =====================================================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DobavljacDTO>> GetById(
        int id)
    {
        if (id <= 0)
        {
            return BadRequest(
                "ID dobavljača mora biti veći od nule."
            );
        }

        var dobavljac =
            await _dobavljacServis
                .GetByIdAsync(id);

        if (dobavljac is null)
        {
            return NotFound(
                $"Dobavljač sa ID-em {id} nije pronađen."
            );
        }

        return Ok(dobavljac);
    }

    // =====================================================
    // SEARCH
    // =====================================================

    [HttpGet("search")]
    public async Task<ActionResult<PaginationGeneric<DobavljacDTO>>> Search(
        [FromQuery] string? naziv,
        [FromQuery] bool? aktivan,
        [FromQuery] PaginationParams pagination)
    {
        var dobavljaci =
            await _dobavljacServis
                .SearchAsync(
                    naziv,
                    aktivan,
                    pagination
                );

        return Ok(dobavljaci);
    }
}