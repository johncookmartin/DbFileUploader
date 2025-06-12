using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UploaderLibrary.Csv;

namespace DbFileUploader.ConsoleUI;
public class CsvHandler : InputHandler
{
    public List<string[]> Records { get; set; } = new List<string[]>();
    private readonly int _skipHeaderLines;
    private readonly bool _hasHeaders;
    private readonly CsvUploaderSaveHandler _uploader;

    public CsvHandler(Dictionary<string, string> arguments) : base(arguments)
    {

        var services = CsvDependencyInjection.ConfigureServices(_config);
        var provider = services.BuildServiceProvider();

        //Get Operator Input
        _skipHeaderLines = GetSkipHeaderLines();
        _hasHeaders = GetHasHeaders();
        Records = GetCSVData(arguments["file"]);

        //Processing File
        var db = provider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = provider.GetRequiredService<IUploaderData>();
        _uploader = provider.GetRequiredService<CsvUploaderSaveHandler>();
    }

    public List<string[]> GetCSVData(string csvFilePath)
    {
        List<string[]> records = new();

        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"CSV file not found: {csvFilePath}");
            return records;
        }

        records = CsvHandlerServices.FormatCSV(csvFilePath, _hasHeaders);
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
            Console.WriteLine("Enter Header Lines to Skip: ");
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
            await _uploader.SaveData(Records, _skipHeaderLines, _hasHeaders);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading data: {ex.Message}");
            return false;
        }
        return true;
    }
}
