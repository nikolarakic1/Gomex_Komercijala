using System.Data;

namespace GomexPraksa.KomercijalaStaging
{
    public interface IStagingRepo
    {
        Task<int> CreateImportBatchAsync(
            string fileName);

        Task BulkInsertStagingAsync(
            int importBatchId,
            DataTable table);

        Task MarkImportFailedAsync(
            int importBatchId,
            string errorMessage);
    }
}