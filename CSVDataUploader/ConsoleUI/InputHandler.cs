using CSVDataUploaderLibrary;
using Microsoft.Extensions.Configuration;

namespace CSVDataUploader.ConsoleUI;
public static class InputHandler
{
    public static string GetConfigFile(string[] args)
    {
        string? filePath = null;

        if (args.Length == 2)
        {
            filePath = args[1];
        }

        while (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("Enter Config File Path: ");
            filePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Please enter a file path.");
            }
        }

        return filePath;
    }
    public static List<string[]> GetCSVData(string[] args, IConfiguration config)
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

    public static bool GetDeletePrevious(IConfiguration config, string tableName)
    {
        bool deletePrevious = true;
        bool isValid = false;

        var configSection = config.GetSection("CsvDetails:DeletePrevious");
        if (configSection.Exists())
        {
            deletePrevious = configSection.Get<bool>();
            isValid = true;
        }

        while (!isValid)
        {
            Console.WriteLine($"Do you wish to delete previous data in {tableName}?(Y/N)");
            string? response = Console.ReadLine();
            if (response != null)
            {
                switch (response.ToUpper())
                {
                    case "Y":
                        deletePrevious = true;
                        isValid = true;
                        break;
                    case "N":
                        deletePrevious = false;
                        isValid = true;
                        break;
                    default:
                        Console.WriteLine("response must be 'Y' or 'N'");
                        break;
                }
            }

        }

        return deletePrevious;
    }

    public static string GetFilePath()
    {
        string? filePath = null;
        do
        {
            Console.WriteLine("Enter File Path: ");
            filePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Please enter a file path.");
            }
        }
        while (string.IsNullOrWhiteSpace(filePath));

        return filePath;
    }

    public static bool GetHasHeaders(IConfiguration config)
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

    public static int GetSkipHeaderLines(IConfiguration config)
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

    public static string GetTableName()
    {
        string? tableName = null;
        do
        {
            Console.WriteLine("Enter Name of Table to Import To: ");
            tableName = Console.ReadLine();
            if (tableName == null)
            {
                Console.WriteLine("Please enter a table name.");
            }
        }
        while (tableName == null);

        return tableName;
    }
}
