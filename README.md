# SubScan

A command-line tool that scans a folder of .xlsx files, extracts subaward entries from section G of each spreadsheet, and prints a summary of total amounts per recipient.

## About this application

This application:
- Is a .NET 10.0 console application.
- Accepts folder paths as first and only argument.
- Scans all Excel files in the given folder for specific data.
- Outputs results and summaries to the console.
- Is designed to be run from the command line without modification.

### How It Works
 
For each `.xlsx` file in the given folder, the tool reads the first worksheet and looks for rows between a cell starting with `G.` and a cell starting with `H.` in column A. Within that range, any row where column B starts with `Subaward:` will have its value extracted — either the remainder of column B, or column C if column B contains only the label. The total amount is read from the first formula-calculated cell in that row.

After all files are processed, totals are aggregated per recipient and printed as a summary.

### Assumptions:
- Windows 10 or later, maybe with .NET 10.0 installed. 
- Assuming 365 versions of Excel with .xlsx files only.
- Assuming the structure of the Excel files is consistent with the described format
- `PrintFile` and `PrintSummaries` functions are simple enough to not need testing.  No ROI seen.
- File counts and lengths are manageable within time and within memory constraints of a single process. Benchmarking not needed.
- Alphabetical listing of recipients is easy to read, but we may want to move the summary of malformed data to the front or end.

### Questions:
- Is the Subaward only in Section G? Can we optimize a bit?
- What if someone adds or removes a section and sections get relettered?
- How should the application handle unexpected or mandated changes in the Excel file structure?
- What existing licenses does the organization have for Excel file manipulation libraries?
- Who populates these? One person? A few staff? Or non-staff? How forgiving should the app be? Is a SOP needed?
- Is this manually run or scheduled to run automatically? At night? During the day?
- If manually, who runs this application? Is it a technical user or a non-technical user?
- If automated, how often are these processed? What is the expected volume of Excel files to be processed?
- What are the next features the users are likely to want? Should we design with those in mind? (e.g., output to CSV, email results, etc.)
- How should we handle files without subawards? Should we print them with a note, or skip them entirely?
- What if the name or amount is missing? Should we print a warning, skip the entry, or include it with a placeholder?
- How is logging handled here? 

### Usage 
```
SubScan.exe <folder path>
```

**Example:**
```
SubScan.exe "C:\temp\first"
```

The tool prints each .xlsx filename it finds followed by the subaward recipients found in it, then prints a combined summary of totals across all files sorted alphabetically by recipient name.

### Requirements
 
- [.NET 10](https://dotnet.microsoft.com/download)
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) (referenced via NuGet)

### Building
 
```
dotnet build
```
 
### Running Tests
 
```
dotnet test
```
 
Test fixtures are included under `Tests/TestData/` and are copied to the output directory automatically — no setup required.
