namespace SubScan;

using ClosedXML.Excel;
using System;

/// <summary>
/// Represents a single subaward entry with a name and an associated amount.
/// </summary>
/// <param name="Name">The name of the subaward recipient.</param>
/// <param name="Amount">The amount associated with the subaward.</param>
internal record SubawardEntry(string Name, double Amount);

/// <summary>
/// Provides methods for reading subaward entry data from Excel files.
/// </summary>
/// <remarks>This class is intended for single, internal use and is not thread-safe. It processes Excel worksheets to
/// extract subaward recipient and amount information, specifically from sections marked with 'G.' in the worksheet. The
/// worksheet name is not required to be consistent, as the first worksheet is always used.</remarks>
internal class FileScanner
{
    /// <summary>
    /// Reads subaward entries from the first worksheet of the specified Excel file, extracting entries found within section G.
    /// </summary>
    /// <remarks>The method processes only the first worksheet in the Excel file, regardless of its name.
    /// Section G is identified by rows starting with "G." and ends when a row starting with "H." is encountered. Only
    /// entries within section G are returned.</remarks>
    /// <param name="filePath">The path to the Excel file to read. The file must exist and be accessible.</param>
    /// <returns>An enumerable collection of SubawardEntry objects representing the recipients and amounts found in section G of
    /// the worksheet. The collection will be empty if no entries are found.</returns>
    internal static IEnumerable<SubawardEntry> ReadSubawardEntries(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        // Worksheet name wasn't consistent, going with the First. 
        var sheet = workbook.Worksheets.First(); 
        var rowCount = sheet.LastRowUsed()?.RowNumber() ?? 0;

        bool inSectionG = false;

        for (int row = 1; row <= rowCount; row++)
        {
            var sectionMarker = sheet.Cell(row, 1).Value.ToString();

            if (sectionMarker.StartsWith("H."))
            {
                inSectionG = false;
            }

            if (inSectionG)
            {
                if (TryGetRecipient(sheet, row, out string recipient))
                {
                    var total = GetFirstCalculatedValue(sheet.Row(row));
                    yield return new SubawardEntry(recipient, double.TryParse(total, out double amount) ? amount : 0);
                }
            }

            if (sectionMarker.StartsWith("G."))
            {
                inSectionG = true;
            }
        }
    }

    /// <summary>
    /// Attempts to extract the recipient name from the specified worksheet row if the cell in column B starts with "Subaward:".
    /// </summary>
    /// <remarks>If the recipient name is not present after the colon in column B, the method attempts to read
    /// it from column C. If no name is found, the out parameter is set to "No name found!".</remarks>
    /// <param name="sheet">The worksheet to read cell values from.</param>
    /// <param name="row">The one-based row index to examine for recipient information.</param>
    /// <param name="recipient">When this method returns, contains the extracted recipient name if found; otherwise, an empty string.</param>
    /// <returns>true if a recipient name was successfully extracted; otherwise, false.</returns>
    private static bool TryGetRecipient(IXLWorksheet sheet, int row, out string recipient)
    {
        var columnB = sheet.Cell(row, 2).Value.ToString().Trim();

        if (columnB.StartsWith("Subaward:"))
        {
            // Check if the name is at the end of this column, otherwise read column C.
            var split = columnB.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length > 1)
            {
                recipient = split[1].Trim();
            }
            else
            {
                // Read column C instead.
                var colC = sheet.Cell(row, 3).Value.ToString().Trim();
                recipient = string.IsNullOrEmpty(colC) ? "No name found!" : colC;
            }
            return true;
        }

        recipient = string.Empty;
        return false;
    }

    /// <summary>
    /// Retrieves the value of the first cell in the specified row that contains a formula.
    /// </summary>
    /// <param name="row">The row to search for cells with formulas. Cannot be null.</param>
    /// <returns>A string representing the value of the first cell with a formula in the row; or null if no such cell exists.</returns>
    private static string? GetFirstCalculatedValue(IXLRow row)
    {
        return row.CellsUsed()
                  .FirstOrDefault(c => c.HasFormula)
                  ?.Value.ToString();
    }
}
