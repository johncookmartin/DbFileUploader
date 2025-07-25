using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UploaderLibrary;

namespace DbFileUploader.ConsoleUI;
public class CsvHandler : InputHandler
{
    public List<string[]> Records { get; set; } = new List<string[]>();
    private readonly int _skipHeaderLines;
    private readonly bool _hasHeaders;
    private readonly IHandlerServices<List<string[]>> _handler;
    private readonly IUploaderSaveHandler<List<string[]>> _uploader;
    private int _tableId;

    public CsvHandler(Dictionary<string, string> arguments) : base(arguments)
    {

        var services = CsvDependencyInjection.ConfigureServices(_config);
        var provider = services.BuildServiceProvider();

        //Get Operator Input
        _skipHeaderLines = GetSkipHeaderLines();
        _hasHeaders = GetHasHeaders();
        _handler = provider.GetRequiredService<IHandlerServices<List<string[]>>>();
        Records = GetCSVData(arguments["file"]);

        //Processing File
        var db = provider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = provider.GetRequiredService<IUploaderData>();
        _uploader = provider.GetRequiredService<IUploaderSaveHandler<List<string[]>>>();
    }

    public async Task CreateTable()
    {
        bool hasDefinition = _config.GetSection("Columns").Exists();
        if (!hasDefinition)
        {
            bool isValid = false;
            while (!isValid)
            {
                Console.WriteLine("Do you want to create this table in the database based on the CSV file inputs? (Y/N)");
                string? response = Console.ReadLine();
                if (response != null)
                {
                    switch (response.ToUpper())
                    {
                        case "Y":
                            Console.WriteLine("Creating table based on CSV file inputs.");
                            await _uploader.CreateData(_tableId);
                            isValid = true;
                            break;
                        case "N":
                            Console.WriteLine("Skipping table creation.");
                            isValid = true;
                            break;
                        default:
                            Console.WriteLine("Response must be 'Y' or 'N'");
                            break;
                    }
                }
            }
        }
    }

    public List<string[]> GetCSVData(string csvFilePath)
    {
        List<string[]> records = new();

        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"CSV file not found: {csvFilePath}");
            return records;
        }

        records = _handler.FormatData(csvFilePath, new { IncludeHeaders = _hasHeaders });
        if (records.Count == 0)
        {
            Console.WriteLine("CSV file is empty");
        }

        return records;

    }

    public bool GetHasHeaders()
    {
        bool hasHeaders = true;
        bool isValid = false;

        var configSection = _config.GetSection("CsvDetails:HasHeaders");
        if (configSection.Exists())
        {
            hasHeaders = configSection.Get<bool>();
            isValid = true;
        }

        while (!isValid)
        {
            Console.WriteLine($"Does the file have specified headers included?(Y/N)");
            string? response = Console.ReadLine();
            if (response != null)
            {
                switch (response.ToUpper())
                {
                    case "Y":
                        hasHeaders = true;
                        isValid = true;
                        break;
                    case "N":
                        hasHeaders = false;
                        isValid = true;
                        break;
                    default:
                        Console.WriteLine("response must be 'Y' or 'N'");
                        break;
                }
            }

        }

        return hasHeaders;
    }

    public int GetSkipHeaderLines()
    {
        int skipHeaderLines = 0;
        bool headerLinesEntered = false;

        var configSection = _config.GetSection("CsvDetails:SkipHeaderLines");
        if (configSection.Exists())
        {
            skipHeaderLines = configSection.Get<int>();
            headerLinesEntered = true;
        }

        while (!headerLinesEntered)
        {
            Console.WriteLine("Enter Leading Lines to Skip (If using headers for mapping don't skip the headers): ");
            string? headerLineString = Console.ReadLine();
            headerLinesEntered = int.TryParse(headerLineString, out skipHeaderLines);
            if (!headerLinesEntered)
            {
                Console.WriteLine("That is not a valid number. Please enter a valid number.");
            }
        }

        return skipHeaderLines;
    }

    public async Task<bool> UploadFile()
    {
        if (Records.Count == 0)
        {
            Console.WriteLine("No records found in the CSV file.");
            return false;
        }

        if (_config.GetValue<bool>("DeletePrevious"))
        {
            try
            {
                await _uploader.DeleteTableData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting previous data: {ex.Message}");
                return false;
            }
        }

        try
        {
            _tableId = await _uploader.SaveData(Records, new { StartingIndex = _skipHeaderLines, HasHeaders = _hasHeaders });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading data: {ex.Message}");
            return false;
        }
        return true;
    }
}
