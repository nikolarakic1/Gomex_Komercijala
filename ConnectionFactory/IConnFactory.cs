using System.Data;

namespace GomexPraksa.ConnectionFactory
{
    public interface IConnFactory
    {
        IDbConnection CreateConnection();
    }
}
