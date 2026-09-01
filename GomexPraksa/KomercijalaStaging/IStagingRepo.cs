using System.Data;

namespace GomexPraksa.KomercijalaStaging
{
    public interface IStagingRepo
    {
        Task BulkInsertStagingAsync( DataTable table);
    }
}
