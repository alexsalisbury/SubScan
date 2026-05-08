namespace SubScan.Tests;

public class SubAwardServiceTests : IDisposable
{
    private readonly string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public SubAwardServiceTests() => Directory.CreateDirectory(tempDir);

    [Fact]
    public void ReturnsXlsxFiles()
    {
        File.WriteAllText(Path.Combine(tempDir, "a.xlsx"), "");
        File.WriteAllText(Path.Combine(tempDir, "b.xlsx"), "");

        var results = SubawardScannerService.GetFiles(tempDir).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, f => Assert.EndsWith(".xlsx", f));
    }

    [Fact]
    public void IgnoresNonXlsxFiles()
    {
        File.WriteAllText(Path.Combine(tempDir, "a.xlsx"), "");
        File.WriteAllText(Path.Combine(tempDir, "b.csv"), "");
        File.WriteAllText(Path.Combine(tempDir, "c.txt"), "");

        var results = SubawardScannerService.GetFiles(tempDir).ToList();

        Assert.Single(results);
    }

    [Fact]
    public void ReturnsEmpty_WhenDirectoryIsEmpty()
    {
        var results = SubawardScannerService.GetFiles(tempDir).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void ReturnsEmpty_WhenDirectoryDoesNotExist()
    {
        var results = SubawardScannerService.GetFiles(Path.Combine(tempDir, "nonexistent")).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void ReturnsSingleEntry()
    {
        var entries = new List<SubawardEntry> { new("Acme", 1000.00) };

        var result = SubawardScannerService.CalculateSummaries(entries);

        Assert.Single(result);
        Assert.Equal(1000.00, result["Acme"]);
    }

    [Fact]
    public void SumsEntriesForSameRecipient()
    {
        var entries = new List<SubawardEntry>
        {
            new("Acme", 1000.00),
            new("Acme", 500.00)
        };

        var result = SubawardScannerService.CalculateSummaries(entries);

        Assert.Equal(1500.00, result["Acme"]);
    }

    [Fact]
    public void HandlesMutipleRecipients()
    {
        var entries = new List<SubawardEntry>
        {
            new("Acme", 1000.00),
            new("Beta LLC", 750.00)
        };

        var result = SubawardScannerService.CalculateSummaries(entries);

        Assert.Equal(2, result.Count);
        Assert.Equal(1000.00, result["Acme"]);
        Assert.Equal(750.00, result["Beta LLC"]);
    }

    [Fact]
    public void ReturnsEmpty_WhenNoEntries()
    {
        var result = SubawardScannerService.CalculateSummaries(new List<SubawardEntry>());

        Assert.Empty(result);
    }

    [Fact]
    public void HandlesMissingName()
    {
        var entries = new List<SubawardEntry> { new("No name found!", 500.00) };

        var result = SubawardScannerService.CalculateSummaries(entries);

        Assert.Equal(500.00, result["No name found!"]);
    }

    public void Dispose() => Directory.Delete(tempDir, recursive: true);
}
