using Dapper;
using GomexPraksa.ConnectionFactory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.ModelsDash;

namespace GomexPraksa.Controllers
{
    [Authorize(Roles = "Menadzer,SefMenadzera")]
    [ApiController]
    [Route("api/odeljenja")]
    public class OdeljenjaController : ControllerBase
    {
        private readonly IConnFactory _connFactory;

        public OdeljenjaController(IConnFactory connFactory)
        {
            _connFactory = connFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Odeljenje>>> GetAll()
        {
            try
            {
                using var connection = _connFactory.CreateConnection();

                const string sql = @"SELECT OdeljenjeId, Naziv FROM dbo.Odeljenje ORDER BY Naziv";

                var list = await connection.QueryAsync<Odeljenje>(sql);

                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500, "Greška prilikom učitavanja odeljenja.");
            }
        }
    }
}
