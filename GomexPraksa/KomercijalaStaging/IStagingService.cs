namespace GomexPraksa.KomercijalaStaging
{
    public interface IStagingService
    {
        Task<int> ImportExcelAsync(
            IFormFile file);
    }
}