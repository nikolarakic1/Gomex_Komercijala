using Microsoft.Data.SqlClient;
using System.Data;

namespace GomexPraksa.ConnectionFactory
{
    public class ConnFactory : IConnFactory
    {
        private readonly string _connString;
        public ConnFactory(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Problem sa konekcijom");
        }
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connString);
        }
    }
}
