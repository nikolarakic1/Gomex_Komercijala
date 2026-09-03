using Microsoft.Data.SqlClient;
using System.Data;

namespace GomexPraksa.KomercijalaStaging
{
    public class StagingRepo : IStagingRepo
    {
        private readonly string _connectionString;

        public StagingRepo(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string nije pronađen.");
        }

        public async Task<int> CreateImportBatchAsync(
            string fileName)
        {
            const string sql = """
                INSERT INTO dbo.KomercijalaImportBatch
                (
                    FileName,
                    Status
                )
                OUTPUT INSERTED.ImportBatchId
                VALUES
                (
                    @FileName,
                    'Processing'
                );
                """;

            await using var connection =
                new SqlConnection(
                    _connectionString);

            await connection.OpenAsync();

            await using var command =
                new SqlCommand(
                    sql,
                    connection);

            command.Parameters.Add(
                "@FileName",
                SqlDbType.NVarChar,
                255
            ).Value = fileName;

            var result =
                await command.ExecuteScalarAsync();

            if (result == null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Nije moguće kreirati import batch.");
            }

            return Convert.ToInt32(result);
        }

        public async Task BulkInsertStagingAsync(
            int importBatchId,
            DataTable table)
        {
            ArgumentNullException.ThrowIfNull(table);

            if (table.Rows.Count == 0)
            {
                throw new ArgumentException(
                    "Tabela za import nema podataka.");
            }

            await using var connection =
                new SqlConnection(
                    _connectionString);

            await connection.OpenAsync();

            await using var transaction =
                (SqlTransaction)
                await connection
                    .BeginTransactionAsync();

            try
            {
                using var bulkCopy =
                    new SqlBulkCopy(
                        connection,
                        SqlBulkCopyOptions.CheckConstraints |
                        SqlBulkCopyOptions.TableLock,
                        transaction);

                bulkCopy.DestinationTableName =
                    "dbo.KomercijalaImportStaging";

                bulkCopy.BatchSize = 10000;
                bulkCopy.BulkCopyTimeout = 0;

                bulkCopy.ColumnMappings.Add(
                    "ImportBatchId",
                    "ImportBatchId");

                bulkCopy.ColumnMappings.Add(
                    "Tip",
                    "Tip");

                bulkCopy.ColumnMappings.Add(
                    "Godina",
                    "Godina");

                bulkCopy.ColumnMappings.Add(
                    "Mesec",
                    "Mesec");

                bulkCopy.ColumnMappings.Add(
                    "Nedelja",
                    "Nedelja");

                bulkCopy.ColumnMappings.Add(
                    "IdKampanja1",
                    "IdKampanja1");

                bulkCopy.ColumnMappings.Add(
                    "IdKampanja2",
                    "IdKampanja2");

                bulkCopy.ColumnMappings.Add(
                    "CmUtice",
                    "CmUtice");

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
                    "PDV",
                    "PDV");

                bulkCopy.ColumnMappings.Add(
                    "Dobavljac",
                    "Dobavljac");

                bulkCopy.ColumnMappings.Add(
                    "PliUzvoz",
                    "PliUzvoz");

                bulkCopy.ColumnMappings.Add(
                    "Kolicina",
                    "Kolicina");

                bulkCopy.ColumnMappings.Add(
                    "MPVrednost",
                    "MPVrednost");

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

                await bulkCopy
                    .WriteToServerAsync(table);

                const string updateSql = """
                    UPDATE dbo.KomercijalaImportBatch
                    SET
                        Status = 'Completed',
                        ImportedRows = @ImportedRows,
                        FinishedAt = SYSDATETIME(),
                        ErrorMessage = NULL
                    WHERE ImportBatchId =
                        @ImportBatchId;
                    """;

                await using var command =
                    new SqlCommand(
                        updateSql,
                        connection,
                        transaction);

                command.Parameters.Add(
                    "@ImportBatchId",
                    SqlDbType.Int
                ).Value = importBatchId;

                command.Parameters.Add(
                    "@ImportedRows",
                    SqlDbType.Int
                ).Value = table.Rows.Count;

                var affectedRows =
                    await command
                        .ExecuteNonQueryAsync();

                if (affectedRows != 1)
                {
                    throw new InvalidOperationException(
                        $"Import batch {importBatchId} " +
                        "nije pronađen.");
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task MarkImportFailedAsync(
            int importBatchId,
            string errorMessage)
        {
            const string sql = """
                UPDATE dbo.KomercijalaImportBatch
                SET
                    Status = 'Failed',
                    FinishedAt = SYSDATETIME(),
                    ErrorMessage = @ErrorMessage
                WHERE ImportBatchId =
                    @ImportBatchId;
                """;

            if (errorMessage.Length > 2000)
            {
                errorMessage =
                    errorMessage[..2000];
            }

            await using var connection =
                new SqlConnection(
                    _connectionString);

            await connection.OpenAsync();

            await using var command =
                new SqlCommand(
                    sql,
                    connection);

            command.Parameters.Add(
                "@ImportBatchId",
                SqlDbType.Int
            ).Value = importBatchId;

            command.Parameters.Add(
                "@ErrorMessage",
                SqlDbType.NVarChar,
                2000
            ).Value = errorMessage;

            await command.ExecuteNonQueryAsync();
        }
    }
}