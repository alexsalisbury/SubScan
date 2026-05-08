namespace SubScan;

/// <summary>
/// A simple class to scan a folder for subaward budget files, extract the relevant entries, and print summaries to the console.
/// </summary>
internal static class SubawardScannerService
{
    /// <summary>
    /// Processes subaward entries from the specified folder and outputs summaries to the console.
    /// </summary>
    /// <remarks>If no arguments are provided, usage information is displayed and no processing occurs. Each
    /// file in the specified folder is scanned for subaward entries, and summaries are printed to the console.</remarks>
    /// <param name="args">An array of command-line arguments. The first element must be the path to the folder containing the files to process.</param>
    internal static void Run(string[] args)
    {
        var entries = new List<SubawardEntry>();
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: program <folder path>");
            return;
        }

        foreach (var file in GetFiles(args[0]))
        {
            var fileEntries = FileScanner.ReadSubawardEntries(file).ToList();
            PrintFile(file, fileEntries);
            entries.AddRange(fileEntries);
        }

        var recipientTotals = CalculateSummaries(entries);
        PrintSummaries(recipientTotals);
    }

    /// <summary>
    /// Enumerates the paths of all .xlsx files in the specified folder.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to search for .xlsx files. The folder must exist; otherwise, no files are returned.</param>
    /// <returns>An enumerable collection of file paths for all .xlsx files found in the specified folder. If the folder does not
    /// exist or contains no .xlsx files, the collection is empty.</returns>
    internal static IEnumerable<string> GetFiles(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"Directory {folderPath} does not exist.");
            yield break;
        }

        foreach (var result in Directory.GetFiles(folderPath, "*.xlsx"))
        {
            yield return result;
        }
    }

    /// <summary>
    /// Calculates the total amount for each unique subaward name from the provided entries.
    /// </summary>
    /// <param name="entries">The collection of subaward entries to aggregate. Cannot be null.</param>
    /// <returns>A dictionary mapping each unique subaward name to the sum of its associated amounts. The dictionary will be
    /// empty if no entries are provided.</returns>
    internal static Dictionary<string, double> CalculateSummaries(IEnumerable<SubawardEntry> entries)
    {
        return entries
            .GroupBy(e => e.Name)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }

    /// <summary>
    /// Prints the file name and the names of all subaward entries to the console.
    /// </summary>
    /// <param name="filePath">The path of the file whose name will be printed. Cannot be null or empty.</param>
    /// <param name="entries">The collection of subaward entries whose names will be printed. Cannot be null.</param>
    private static void PrintFile(string filePath, IEnumerable<SubawardEntry> entries)
    {
        Console.WriteLine();
        Console.WriteLine(Path.GetFileName(filePath));
        Console.WriteLine("-------------------");

        foreach (var entry in entries)
        {
            Console.WriteLine(entry.Name);
        }
    }

    /// <summary>
    /// Writes a formatted summary of recipient totals to the console output.
    /// </summary>
    /// <remarks>The output lists recipients in alphabetical order, displaying each name and total amount formatted as currency.</remarks>
    /// <param name="recipientTotals">A dictionary containing recipient names as keys and their corresponding total amounts as values. Cannot be null.</param>
    private static void PrintSummaries(Dictionary<string, double> recipientTotals)
    {
        Console.WriteLine();
        Console.WriteLine("Summaries");
        Console.WriteLine("-------------------");

        foreach (var kvp in recipientTotals.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value:C}");
        }
    }
}
