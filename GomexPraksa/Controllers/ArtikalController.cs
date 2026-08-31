using GomexPraksa.AddedFunctions;
using GomexPraksa.Services;
using GomexPraksa.ServicesComerc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Models.DtosComerc;
using System.Security.Claims;

namespace GomexPraksa.Controllers
{
    [Authorize(Roles = "Menadzer,SefMenadzera")]
    [ApiController]
    [Route("api/artikli")]
    public class ArtikliController : ControllerBase
    {
        private readonly IArtikalService _artikalService;
        private readonly ICriticalProductsService _productsService;

        public ArtikliController(IArtikalService artikalService, ICriticalProductsService productsService)
        {
            _artikalService = artikalService;
            _productsService = productsService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationGeneric<ArtikalDto>>> GetAll(
    [FromQuery] PaginationParams pagination)
        {
            var artikli =
                await _artikalService.GetAllAsync(
                    pagination
                );

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
        public async Task<ActionResult<PaginationGeneric<ArtikalDto>>> Search(
    [FromQuery] ArtikalFilterDto filter,
    [FromQuery] PaginationParams pagination)
        {
            var artikli =
                await _artikalService.SearchAsync(
                    filter,
                    pagination
                );

            return Ok(artikli);
        }
        [HttpGet("criticalProductsTop")]
        public async Task<ActionResult<IEnumerable<CriticalProductsDTO>>> Top5CriticalProducts(
    [FromQuery] DashboardFilterDTO filter)
        {
            var proizvodi =
                await _productsService.CriticalProductsTop(filter);

            return Ok(proizvodi);
        }
        [HttpGet("CriticalPage")]
        public async Task<ActionResult<IEnumerable<CriticalProductsPageDTO>>> CriticalProductsPage(
        [FromQuery] FilterSharedPages filter)
        {
            var proizvodi = await _productsService.CriticalProductsPage(filter);

            return Ok(proizvodi);
        }
    }


}
