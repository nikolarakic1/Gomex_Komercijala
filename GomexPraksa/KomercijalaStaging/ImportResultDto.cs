namespace GomexPraksa.KomercijalaStaging
{
    public class ImportResultDto
    {
        public int ImportBatchId { get; set; }

        public int InsertedRows { get; set; }

        public string Status { get; set; }
            = string.Empty;
    }
}