using Dapper;
using GomexPraksa.ConnectionFactory;
using Microsoft.AspNetCore.Mvc;
using Models.ModelsDash;

namespace GomexPraksa.Controllers
{
    [ApiController]
    [Route("api/kategorije")]
    public class KategorijeController : ControllerBase
    {
        private readonly IConnFactory _connFactory;

        public KategorijeController(IConnFactory connFactory)
        {
            _connFactory = connFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kategorija>>> GetAll()
        {
            try
            {
                using var connection = _connFactory.CreateConnection();

                const string sql = @"SELECT KategorijaId, OdeljenjeId, Naziv FROM dbo.Kategorija ORDER BY Naziv";

                var list = await connection.QueryAsync<Kategorija>(sql);

                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500, "Greška prilikom učitavanja kategorija.");
            }
        }
    }
}
