using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace GomexPraksa.KomercijalaStaging
{
    public class StagingRepo : IStagingRepo
    {
        private readonly string _connectionString;
        public StagingRepo(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string nije pronađen.");
        }
        public async Task BulkInsertStagingAsync(DataTable table)
        {
            await using var connections = new SqlConnection(_connectionString);
            await connections.OpenAsync();
            using var bulkCopy =new SqlBulkCopy(connections);
            bulkCopy.DestinationTableName =
            "dbo.KomercijalaImportStaging";
            bulkCopy.BatchSize = 10000;
            bulkCopy.BulkCopyTimeout = 120;
            bulkCopy.ColumnMappings.Add("Tip", "Tip");
            bulkCopy.ColumnMappings.Add("Godina", "Godina");
            bulkCopy.ColumnMappings.Add("Mesec", "Mesec");
            bulkCopy.ColumnMappings.Add("Nedelja", "Nedelja");

            bulkCopy.ColumnMappings.Add(
                "Odeljenje",
                "Odeljenje");

            bulkCopy.ColumnMappings.Add(
                "Kategorija",
                "Kategorija");

            bulkCopy.ColumnMappings.Add(
                "RobnaGrupa",
                "RobnaGrupa");

            bulkCopy.ColumnMappings.Add(
                "Sifra",
                "Sifra");

            bulkCopy.ColumnMappings.Add(
                "Artikal",
                "Artikal");

            bulkCopy.ColumnMappings.Add(
                "Dobavljac",
                "Dobavljac");

            bulkCopy.ColumnMappings.Add(
                "Kolicina",
                "Kolicina");

            bulkCopy.ColumnMappings.Add(
                "MPBezPDV",
                "MPBezPDV");

            bulkCopy.ColumnMappings.Add(
                "RUC1",
                "RUC1");

            bulkCopy.ColumnMappings.Add(
                "RUC2",
                "RUC2");

            bulkCopy.ColumnMappings.Add(
                "RUC12",
                "RUC12");

            bulkCopy.ColumnMappings.Add(
                "NabavnaVrednost",
                "NabavnaVrednost");
            await bulkCopy.WriteToServerAsync(table);
        }
    }
}
