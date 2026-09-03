using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;

namespace GomexPraksa.KomercijalaStaging
{
    public class StagingService : IStagingService
    {
        private readonly IStagingRepo _stagingRepo;
        private readonly ILogger<StagingService> _logger;

        private const int MaxRowsToImport = 5000;

        private static readonly string[] RequiredExcelColumns =
        {
            "TIP",
            "GODINA",
            "MESEC",
            "NEDELJA",
            "ID_KAMPANJE",
            "ID_KAMPANJE2",
            "CM_UTICE",
            "ODELJENJE",
            "KATEGORIJA",
            "ROBNA_GRUPA",
            "SIFRA",
            "ARTIKAL",
            "PDV",
            "DOBAVLJAC",
            "PLIUZVOZ",
            "KOLICINA",
            "MP_VREDNOST",
            "MP_BEZ_PDV",
            "RUC1",
            "RUC2",
            "RUC12",
            "NABAVNA_VRED"
        };

        public StagingService(
            IStagingRepo stagingRepo,
            ILogger<StagingService> logger)
        {
            _stagingRepo = stagingRepo;
            _logger = logger;
        }

        public async Task<ImportResultDto> ImportExcelAsync(
            IFormFile file)
        {
            ValidateFile(file);

            await using var stream =
                file.OpenReadStream();

            using var workbook =
                new XLWorkbook(stream);

            var worksheet =
                workbook.Worksheets.FirstOrDefault()
                ?? throw new ArgumentException(
                    "Excel fajl nema worksheet.");

            var firstRow =
                worksheet.FirstRowUsed();

            var lastRow =
                worksheet.LastRowUsed();

            if (firstRow == null ||
                lastRow == null)
            {
                throw new ArgumentException(
                    "Excel fajl nema podataka.");
            }

            var columnMap =
                CreateColumnMap(firstRow);

            ValidateRequiredColumns(
                columnMap);

            int? importBatchId = null;

            try
            {
                importBatchId =
                    await _stagingRepo
                        .CreateImportBatchAsync(
                            file.FileName);

                var table =
                    CreateDataTable();

                var firstDataRow =
                    firstRow.RowNumber() + 1;

                var lastDataRow =
                    lastRow.RowNumber();

                for (var rowNumber = firstDataRow;
                     rowNumber <= lastDataRow;
                     rowNumber++)
                {
                    var row =
                        worksheet.Row(rowNumber);

                    if (IsEmptyRow(
                            row,
                            columnMap))
                    {
                        continue;
                    }

                    try
                    {
                        var dataRow =
                            table.NewRow();

                        dataRow["ImportBatchId"] =
                            importBatchId.Value;

                        dataRow["Tip"] =
                            GetString(
                                row,
                                columnMap,
                                "TIP");

                        dataRow["Godina"] =
                            GetInt(
                                row,
                                columnMap,
                                "GODINA",
                                rowNumber);

                        dataRow["Mesec"] =
                            GetInt(
                                row,
                                columnMap,
                                "MESEC",
                                rowNumber);

                        dataRow["Nedelja"] =
                            GetInt(
                                row,
                                columnMap,
                                "NEDELJA",
                                rowNumber);

                        dataRow["IdKampanja1"] =
                        GetNullableString(
                          row,
                          columnMap,
                         "ID_KAMPANJE");

                        dataRow["IdKampanja2"] =
                            GetNullableString(
                                row,
                                columnMap,
                                "ID_KAMPANJE2");

                        dataRow["CmUtice"] =
                        GetNullableString(
                             row,
                             columnMap,
                                "CM_UTICE");

                        dataRow["Odeljenje"] =
                            GetString(
                                row,
                                columnMap,
                                "ODELJENJE");

                        dataRow["Kategorija"] =
                            GetString(
                                row,
                                columnMap,
                                "KATEGORIJA");

                        dataRow["RobnaGrupa"] =
                            GetString(
                                row,
                                columnMap,
                                "ROBNA_GRUPA");

                        dataRow["Sifra"] =
                            GetString(
                                row,
                                columnMap,
                                "SIFRA");

                        dataRow["Artikal"] =
                            GetString(
                                row,
                                columnMap,
                                "ARTIKAL");

                        dataRow["PDV"] =
                            GetNullableDecimal(
                                row,
                                columnMap,
                                "PDV",
                                rowNumber);

                        dataRow["Dobavljac"] =
                            GetString(
                                row,
                                columnMap,
                                "DOBAVLJAC");

                        dataRow["PliUzvoz"] =
                            GetNullableString(
                                row,
                                columnMap,
                                "PLIUZVOZ");

                        dataRow["Kolicina"] =
                            GetRequiredDecimal(
                                row,
                                columnMap,
                                "KOLICINA",
                                rowNumber);

                        dataRow["MPVrednost"] =
                            GetNullableDecimal(
                                row,
                                columnMap,
                                "MP_VREDNOST",
                                rowNumber);

                        dataRow["MPBezPDV"] =
                            GetRequiredDecimal(
                                row,
                                columnMap,
                                "MP_BEZ_PDV",
                                rowNumber);

                        dataRow["RUC1"] =
                            GetNullableDecimal(
                                row,
                                columnMap,
                                "RUC1",
                                rowNumber);

                        dataRow["RUC2"] =
                            GetNullableDecimal(
                                row,
                                columnMap,
                                "RUC2",
                                rowNumber);

                        dataRow["RUC12"] =
                            GetNullableDecimal(
                                row,
                                columnMap,
                                "RUC12",
                                rowNumber);

                        dataRow["NabavnaVrednost"] =
                            GetRequiredDecimal(
                                row,
                                columnMap,
                                "NABAVNA_VRED",
                                rowNumber);

                        table.Rows.Add(dataRow);

                        if (table.Rows.Count >=
                            MaxRowsToImport)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(
                            $"Greška u Excel redu " +
                            $"{rowNumber}: {ex.Message}",
                            ex);
                    }
                }

                if (table.Rows.Count == 0)
                {
                    throw new ArgumentException(
                        "Excel nema nijedan red za import.");
                }

                await _stagingRepo
                    .BulkInsertStagingAsync(
                        importBatchId.Value,
                        table);

                return new ImportResultDto
                {
                    ImportBatchId =
                        importBatchId.Value,

                    InsertedRows =
                        table.Rows.Count,

                    Status =
                        "Completed"
                };
            }
            catch (Exception ex)
            {
                if (importBatchId.HasValue)
                {
                    try
                    {
                        await _stagingRepo
                            .MarkImportFailedAsync(
                                importBatchId.Value,
                                ex.Message);
                    }
                    catch (Exception statusException)
                    {
                        _logger.LogError(
                            statusException,
                            "Nije moguće označiti " +
                            "ImportBatch {ImportBatchId} " +
                            "kao Failed.",
                            importBatchId.Value);
                    }
                }

                throw;
            }
        }

        private static void ValidateFile(
            IFormFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(
                    nameof(file),
                    "Excel fajl nije prosleđen.");
            }

            if (file.Length == 0)
            {
                throw new ArgumentException(
                    "Excel fajl je prazan.");
            }

            var extension =
                Path.GetExtension(file.FileName);

            if (!extension.Equals(
                    ".xlsx",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Dozvoljeni su samo .xlsx fajlovi.");
            }
        }

        private static Dictionary<string, int>
            CreateColumnMap(
                IXLRow headerRow)
        {
            var columnMap =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var cell
                     in headerRow.CellsUsed())
            {
                var columnName =
                    cell.GetString().Trim();

                if (!string.IsNullOrWhiteSpace(
                        columnName))
                {
                    columnMap[columnName] =
                        cell.Address.ColumnNumber;
                }
            }

            return columnMap;
        }

        private static void ValidateRequiredColumns(
            Dictionary<string, int> columnMap)
        {
            foreach (var requiredColumn
                     in RequiredExcelColumns)
            {
                if (!columnMap.ContainsKey(
                        requiredColumn))
                {
                    throw new ArgumentException(
                        $"Excel nema obaveznu kolonu " +
                        $"'{requiredColumn}'.");
                }
            }
        }

        private static DataTable
            CreateDataTable()
        {
            var table =
                new DataTable(
                    "KomercijalaImportStaging");

            table.Columns.Add(
                "ImportBatchId",
                typeof(int));

            table.Columns.Add(
                "Tip",
                typeof(string));

            table.Columns.Add(
                "Godina",
                typeof(int));

            table.Columns.Add(
                "Mesec",
                typeof(int));

            table.Columns.Add(
                "Nedelja",
                typeof(int));

            AddNullableColumn(
            table,
           "IdKampanja1",
            typeof(string));

            AddNullableColumn(
                table,
                "IdKampanja2",
                typeof(string));

            AddNullableColumn(
                table,
                "CmUtice",
                typeof(string));

            table.Columns.Add(
                "Odeljenje",
                typeof(string));

            table.Columns.Add(
                "Kategorija",
                typeof(string));

            table.Columns.Add(
                "RobnaGrupa",
                typeof(string));

            table.Columns.Add(
                "Sifra",
                typeof(string));

            table.Columns.Add(
                "Artikal",
                typeof(string));

            AddNullableColumn(
                table,
                "PDV",
                typeof(decimal));

            table.Columns.Add(
                "Dobavljac",
                typeof(string));

            AddNullableColumn(
                table,
                "PliUzvoz",
                typeof(string));

            table.Columns.Add(
                "Kolicina",
                typeof(decimal));

            AddNullableColumn(
                table,
                "MPVrednost",
                typeof(decimal));

            table.Columns.Add(
                "MPBezPDV",
                typeof(decimal));

            AddNullableColumn(
                table,
                "RUC1",
                typeof(decimal));

            AddNullableColumn(
                table,
                "RUC2",
                typeof(decimal));

            AddNullableColumn(
                table,
                "RUC12",
                typeof(decimal));

            table.Columns.Add(
                "NabavnaVrednost",
                typeof(decimal));

            return table;
        }

        private static void AddNullableColumn(
            DataTable table,
            string name,
            Type type)
        {
            var column =
                table.Columns.Add(
                    name,
                    type);

            column.AllowDBNull = true;
        }

        private static string GetString(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName)
        {
            return row
                .Cell(columnMap[columnName])
                .GetString()
                .Trim();
        }

        private static object GetNullableString(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName)
        {
            var text =
                row.Cell(columnMap[columnName])
                    .GetString()
                    .Trim();

            if (IsNullValue(text))
            {
                return DBNull.Value;
            }

            return text;
        }

        private static int GetInt(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName,
            int rowNumber)
        {
            var cell =
                row.Cell(
                    columnMap[columnName]);

            if (cell.TryGetValue<int>(
                    out var value))
            {
                return value;
            }

            var text =
                cell.GetString()
                    .Trim();

            if (int.TryParse(
                    text,
                    out value))
            {
                return value;
            }

            throw new ArgumentException(
                $"Kolona '{columnName}' " +
                $"u redu {rowNumber} " +
                $"nije validan ceo broj. " +
                $"Vrednost: '{text}'.");
        }

        private static object GetNullableInt(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName,
            int rowNumber)
        {
            var cell =
                row.Cell(
                    columnMap[columnName]);

            if (cell.IsEmpty())
            {
                return DBNull.Value;
            }

            if (cell.TryGetValue<int>(
                    out var value))
            {
                return value;
            }

            var text =
                cell.GetString()
                    .Trim();

            if (IsNullValue(text))
            {
                return DBNull.Value;
            }

            if (int.TryParse(
                    text,
                    out value))
            {
                return value;
            }

            throw new ArgumentException(
                $"Kolona '{columnName}' " +
                $"u redu {rowNumber} " +
                $"nije validan INT. " +
                $"Vrednost: '{text}'.");
        }

        private static decimal GetRequiredDecimal(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName,
            int rowNumber)
        {
            var result =
                TryParseDecimal(
                    row.Cell(
                        columnMap[columnName]));

            if (result.HasValue)
            {
                return result.Value;
            }

            var text =
                row.Cell(
                        columnMap[columnName])
                    .GetString()
                    .Trim();

            throw new ArgumentException(
                $"Kolona '{columnName}' " +
                $"u redu {rowNumber} " +
                $"nije validan decimalni broj. " +
                $"Vrednost: '{text}'.");
        }

        private static object GetNullableDecimal(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName,
            int rowNumber)
        {
            var cell =
                row.Cell(
                    columnMap[columnName]);

            if (cell.IsEmpty())
            {
                return DBNull.Value;
            }

            var text =
                cell.GetString()
                    .Trim();

            if (IsNullValue(text))
            {
                return DBNull.Value;
            }

            var result =
                TryParseDecimal(cell);

            if (result.HasValue)
            {
                return result.Value;
            }

            throw new ArgumentException(
                $"Kolona '{columnName}' " +
                $"u redu {rowNumber} " +
                $"nije validan decimalni broj. " +
                $"Vrednost: '{text}'.");
        }

        private static decimal? TryParseDecimal(
            IXLCell cell)
        {
            if (cell.TryGetValue<decimal>(
                    out var value))
            {
                return value;
            }

            var text =
                cell.GetString()
                    .Trim();

            if (decimal.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.GetCultureInfo(
                        "sr-RS"),
                    out value))
            {
                return value;
            }

            if (decimal.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                return value;
            }

            return null;
        }

        private static bool IsNullValue(
            string text)
        {
            return string.IsNullOrWhiteSpace(text)
                || text == "-"
                || text == "/"
                || text.Equals(
                    "NULL",
                    StringComparison.OrdinalIgnoreCase)
                || text.Equals(
                    "N/A",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEmptyRow(
            IXLRow row,
            Dictionary<string, int> columnMap)
        {
            foreach (var columnName
                     in RequiredExcelColumns)
            {
                if (!row.Cell(
                        columnMap[columnName])
                    .IsEmpty())
                {
                    return false;
                }
            }

            return true;
        }
    }
}