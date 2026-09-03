using Microsoft.AspNetCore.Http;

namespace GomexPraksa.KomercijalaStaging
{
    public interface IStagingService
    {
        Task<ImportResultDto> ImportExcelAsync(
            IFormFile file);
    }
}