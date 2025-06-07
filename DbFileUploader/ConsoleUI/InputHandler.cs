using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace DbFileUploader.ConsoleUI;
public class InputHandler
{
    public IConfiguration config { get; set; }
    public TableImportSchemaModel schemaModel { get; set; }
    public InputHandler(string[] args)
    {
        var configFilePath = GetConfigFile(args);
        config = AppConfiguration.BuildConfiguration(configFilePath);

        TableImportSchemaModel? importSchemaModel = config.GetSection("TableImportSchema").Get<TableImportSchemaModel>();
        if (importSchemaModel == null)
        {
            Console.WriteLine("There is not a valid TableImportSchema in the config file.");
            return;
        }

        schemaModel = importSchemaModel;

    }
    public string GetConfigFile(string[] args)
    {
        string? filePath = null;

        if (args.Length == 2)
        {
            filePath = args[1];
        }

        filePath = GetFilePath(filePath);

        return filePath;
    }

    public bool GetDeletePrevious(IConfiguration config, string tableName)
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

    public string GetFilePath(string? filePath = null)
    {
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
    public string GetTableName()
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
