using ClosedXML.Excel;
using System.Data;
using System.Globalization;

namespace GomexPraksa.KomercijalaStaging
{
    public class StagingService : IStagingService
    {
        private readonly IStagingRepo _stagingRepo;

        private static readonly string[] RequiredExcelColumns =
        {
            "TIP",
            "GODINA",
            "MESEC",
            "NEDELJA",
            "ODELJENJE",
            "KATEGORIJA",
            "ROBNA_GRUPA",
            "SIFRA",
            "ARTIKAL",
            "DOBAVLJAC",
            "KOLICINA",
            "MP_BEZ_PDV",
            "RUC1",
            "RUC2",
            "RUC12",
            "NABAVNA_VRED"
        };

        public StagingService(
            IStagingRepo stagingRepo)
        {
            _stagingRepo = stagingRepo;
        }

        public async Task<int> ImportExcelAsync(
            IFormFile file)
        {
            // =====================================================
            // 1. PROVERA FAJLA
            // =====================================================

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

            // =====================================================
            // 2. PROVERA EKSTENZIJE
            // =====================================================

            var extension =
                Path.GetExtension(file.FileName);

            if (!extension.Equals(
                    ".xlsx",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Dozvoljeni su samo .xlsx fajlovi.");
            }

            // =====================================================
            // 3. OTVARANJE EXCELA
            // =====================================================

            await using var stream =
                file.OpenReadStream();

            using var workbook =
                new XLWorkbook(stream);

            var worksheet =
                workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                throw new ArgumentException(
                    "Excel fajl nema worksheet.");
            }

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

            // =====================================================
            // 4. MAPIRANJE EXCEL HEADERA
            // =====================================================

            var columnMap =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var cell in firstRow.CellsUsed())
            {
                var columnName =
                    cell.GetString().Trim();

                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    columnMap[columnName] =
                        cell.Address.ColumnNumber;
                }
            }

            // =====================================================
            // 5. PROVERA OBAVEZNIH KOLONA
            // =====================================================

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

            // =====================================================
            // 6. DATATABLE
            // =====================================================

            var table =
                CreateDataTable();

            // =====================================================
            // 7. ČITANJE REDOVA
            // =====================================================

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

                    dataRow["Dobavljac"] =
                        GetString(
                            row,
                            columnMap,
                            "DOBAVLJAC");

                    dataRow["Kolicina"] =
                        GetDecimal(
                            row,
                            columnMap,
                            "KOLICINA",
                            rowNumber);

                    dataRow["MPBezPDV"] =
                        GetDecimal(
                            row,
                            columnMap,
                            "MP_BEZ_PDV",
                            rowNumber);

                    dataRow["RUC1"] =
                        GetDecimal(
                            row,
                            columnMap,
                            "RUC1",
                            rowNumber);

                    dataRow["RUC2"] =
                        GetDecimal(
                            row,
                            columnMap,
                            "RUC2",
                            rowNumber);

                    dataRow["RUC12"] =
                        GetDecimal(
                            row,
                            columnMap,
                            "RUC12",
                            rowNumber);

                    dataRow["NabavnaVrednost"] =
                        GetDecimal(
                            row,
                            columnMap,
                            "NABAVNA_VRED",
                            rowNumber);

                    table.Rows.Add(dataRow);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"Greška u Excel redu {rowNumber}: " +
                        ex.Message,
                        ex);
                }
            }

            // =====================================================
            // 8. PROVERA REDOVA
            // =====================================================

            if (table.Rows.Count == 0)
            {
                throw new ArgumentException(
                    "Excel nema nijedan red za import.");
            }

            // =====================================================
            // 9. SQL BULK COPY
            // =====================================================

            await _stagingRepo
                .BulkInsertStagingAsync(table);

            // =====================================================
            // 10. BROJ UBAČENIH REDOVA
            // =====================================================

            return table.Rows.Count;
        }

        // =========================================================
        // DATATABLE
        // =========================================================

        private static DataTable CreateDataTable()
        {
            var table =
                new DataTable(
                    "KomercijalaImportStaging");

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

            table.Columns.Add(
                "Dobavljac",
                typeof(string));

            table.Columns.Add(
                "Kolicina",
                typeof(decimal));

            table.Columns.Add(
                "MPBezPDV",
                typeof(decimal));

            table.Columns.Add(
                "RUC1",
                typeof(decimal));

            table.Columns.Add(
                "RUC2",
                typeof(decimal));

            table.Columns.Add(
                "RUC12",
                typeof(decimal));

            table.Columns.Add(
                "NabavnaVrednost",
                typeof(decimal));

            return table;
        }

        // =========================================================
        // STRING
        // =========================================================

        private static string GetString(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName)
        {
            var columnNumber =
                columnMap[columnName];

            return row
                .Cell(columnNumber)
                .GetString()
                .Trim();
        }

        // =========================================================
        // INT
        // =========================================================

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
                cell.GetString().Trim();

            if (int.TryParse(
                    text,
                    out value))
            {
                return value;
            }

            throw new ArgumentException(
                $"Kolona '{columnName}' " +
                $"u redu {rowNumber} " +
                $"nije validan ceo broj.");
        }

        // =========================================================
        // DECIMAL
        // =========================================================

        private static decimal GetDecimal(
            IXLRow row,
            Dictionary<string, int> columnMap,
            string columnName,
            int rowNumber)
        {
            var cell =
                row.Cell(
                    columnMap[columnName]);

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
                    CultureInfo.GetCultureInfo("sr-RS"),
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

            throw new ArgumentException(
                $"Kolona '{columnName}' " +
                $"u redu {rowNumber} " +
                $"nije validan decimalni broj.");
        }

        // =========================================================
        // PRAZAN RED
        // =========================================================

        private static bool IsEmptyRow(
            IXLRow row,
            Dictionary<string, int> columnMap)
        {
            foreach (var columnName
                     in RequiredExcelColumns)
            {
                var cell =
                    row.Cell(
                        columnMap[columnName]);

                if (!cell.IsEmpty())
                {
                    return false;
                }
            }

            return true;
        }
    }
}