namespace SubScan.Tests;

using ClosedXML.Excel;
using System.Reflection;

public class FileScannerTests
{
    private readonly List<string> tempFiles = new();

    [Fact]
    public void ReturnsExpectedEntries_ForSampleFile()
    {
        var filePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "TestData", "SubawardBudgetExample1.xlsx");

        var results = FileScanner.ReadSubawardEntries(filePath).ToList();

        Assert.Equal(4, results.Count);
        Assert.Equal("Indiana", results[0].Name);
        Assert.Equal("Mayo", results[1].Name);
        Assert.Equal("Purdue", results[2].Name);
        Assert.Equal("Florida", results[3].Name);
    }

    [Fact]
    public void ReturnsEntries_WhenNameIsInColumnB()
    {
        var path = CreateWorkbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "G.1";
            sheet.Cell(2, 2).Value = "Subaward: Acme Corp";
        });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Single(results);
        Assert.Equal("Acme Corp", results[0].Name);
    }

    [Fact]
    public void ReturnsColumnC_WhenNameIsNotInColumnB()
    {
        var path = CreateWorkbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "G.1";
            sheet.Cell(2, 2).Value = "Subaward:";
            sheet.Cell(2, 3).Value = "Beta LLC";
        });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Single(results);
        Assert.Equal("Beta LLC", results[0].Name);
    }

    [Fact]
    public void ReturnsDefault_WhenNameIsNotInColumnBOrC()
    {
        var path = CreateWorkbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "G.1";
            sheet.Cell(2, 2).Value = "Subaward:";
        });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Single(results);
        Assert.Equal("No name found!", results[0].Name);
    }

    [Fact]
    public void StopsAtSectionH()
    {
        var path = CreateWorkbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "G.1";
            sheet.Cell(2, 2).Value = "Subaward: Acme Corp";
            sheet.Cell(3, 1).Value = "H.1";
            sheet.Cell(4, 2).Value = "Subaward: Should Not Appear";
        });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Single(results);
    }

    [Fact]
    public void ReturnsNothing_WhenNoSectionG()
    {
        var path = CreateWorkbook(sheet =>
        {
            sheet.Cell(1, 2).Value = "Subaward: Acme Corp";
        });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void ReturnsNothing_WhenSheetIsEmpty()
    {
        var path = CreateWorkbook(_ => { });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void HandlesMultipleSectionGBlocks()
    {
        var path = CreateWorkbook(sheet =>
        {
            sheet.Cell(1, 1).Value = "G.1";
            sheet.Cell(2, 2).Value = "Subaward: First";
            sheet.Cell(3, 1).Value = "H.1";
            sheet.Cell(4, 1).Value = "G.2";
            sheet.Cell(5, 2).Value = "Subaward: Second";
        });

        var results = FileScanner.ReadSubawardEntries(path).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("First", results[0].Name);
        Assert.Equal("Second", results[1].Name);
    }

    public void Dispose()
    {
        foreach (var f in tempFiles)
            if (File.Exists(f)) File.Delete(f);
    }


    private string CreateWorkbook(Action<IXLWorksheet> populate)
    {
        var path = Path.GetTempFileName() + ".xlsx";
        using var wb = new XLWorkbook();
        var sheet = wb.AddWorksheet("Sheet1");
        populate(sheet);
        wb.SaveAs(path);
        tempFiles.Add(path);
        return path;
    }
}