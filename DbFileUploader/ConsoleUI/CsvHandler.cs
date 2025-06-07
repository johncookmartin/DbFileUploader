using CSVDataUploaderLibrary;
using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbFileUploader.ConsoleUI;
public class CsvHandler : InputHandler
{
    public bool DeletePrevious { get; set; }
    public List<string[]> Records { get; set; } = new List<string[]>();
    public int SkipHeaderLines { get; set; }
    public UploaderSaveHandler Uploader { get; set; }

    public CsvHandler(string[] args) : base(args)
    {

        var services = CsvDependencyInjection.ConfigureServices(config, schemaModel);
        var provider = services.BuildServiceProvider();

        //Get Operator Input
        SkipHeaderLines = GetSkipHeaderLines(config);
        Records = GetCSVData(args, config);
        DeletePrevious = GetDeletePrevious(config, schemaModel.TableName);

        //Processing File
        var db = provider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = provider.GetRequiredService<IUploaderData>();
        Uploader = provider.GetRequiredService<UploaderSaveHandler>();
    }

    public List<string[]> GetCSVData(string[] args, IConfiguration config)
    {
        List<string[]> records = new();

        string csvFilePath;
        if (args.Length == 0)
        {
            Console.WriteLine("No File Detected.");
            csvFilePath = GetFilePath();
        }


        csvFilePath = args[0];
        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"CSV file not found: {csvFilePath}");
            return records;
        }


        bool hasHeaders = GetHasHeaders(config);
        records = CsvHandlerServices.FormatCSV(csvFilePath, hasHeaders);
        if (records.Count == 0)
        {
            Console.WriteLine("CSV file is empty");
        }

        return records;

    }

    public bool GetHasHeaders(IConfiguration config)
    {
        bool hasHeaders = true;
        bool isValid = false;

        var configSection = config.GetSection("CsvDetails:HasHeaders");
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

    public int GetSkipHeaderLines(IConfiguration config)
    {
        int skipHeaderLines = 0;
        bool headerLinesEntered = false;

        var configSection = config.GetSection("CsvDetails:SkipHeaderLines");
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

        if (DeletePrevious)
        {
            try
            {
                await Uploader.DeleteTableData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting previous data: {ex.Message}");
                return false;
            }
        }

        try
        {
            await Uploader.SaveData(Records, SkipHeaderLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading data: {ex.Message}");
            return false;
        }
        return true;
    }
}
