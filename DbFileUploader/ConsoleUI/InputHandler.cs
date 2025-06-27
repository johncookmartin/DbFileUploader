using DbFileUploader.Configuration;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DbFileUploader.ConsoleUI;
public class InputHandler
{
    protected readonly IConfiguration _config;
    public InputHandler(Dictionary<string, string> arguments)
    {
        string? configFilePath = GetConfigFile(arguments);
        bool hasConnectionString = TryCheckConfig(configFilePath, "ConnectionStrings", out string? connectionString);
        if (!TryCheckConfig(configFilePath, "TableName", out string? tableName) || string.IsNullOrWhiteSpace(tableName))
        {
            tableName = GetTableName(arguments);
        }
        if (!TryCheckConfig(configFilePath, "DbName", out string? dbName) || string.IsNullOrWhiteSpace(dbName))
        {
            dbName = GetDbName(arguments);
        }
        if (!TryCheckConfig(configFilePath, "DeletePrevious", out bool deletePrevious))
        {
            deletePrevious = GetDeletePrevious(arguments, tableName)!;
        }

        _config = AppConfiguration.BuildConfiguration(configFilePath, tableName, dbName, deletePrevious, hasConnectionString);

    }

    public bool TryCheckConfig<T>(string? configFilePath, string propertyName, out T? propertyValue)
    {
        bool hasProperty = false;
        propertyValue = default(T);
        if (configFilePath != null)
        {
            var jsonText = File.ReadAllText(configFilePath);
            using var jsonDoc = JsonDocument.Parse(jsonText);
            hasProperty = jsonDoc.RootElement.TryGetProperty(propertyName, out var tryValue);

            if (hasProperty)
            {
                try
                {
                    if (typeof(T) == typeof(bool))
                    {
                        propertyValue = (T)(object)tryValue.GetBoolean();
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        propertyValue = (T)(object)tryValue.GetInt64();
                    }
                    else
                    {
                        propertyValue = (T?)(object?)tryValue.GetString();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error parsing property '{propertyName}' in config file: {ex.Message}");
                    hasProperty = false;
                }
            }
        }
        return hasProperty;
    }
    public string GetDbName(Dictionary<string, string> arguments)
    {
        bool isValid = arguments.TryGetValue("db", out var dbName);
        bool includeDbName = isValid;
        while (!isValid || string.IsNullOrWhiteSpace(dbName))
        {
            Console.WriteLine("No DbName detected, did you want to include one?(Y/N)");
            string? response = Console.ReadLine();
            if (response != null)
            {
                switch (response.ToUpper())
                {
                    case "Y":
                        includeDbName = true;
                        isValid = true;
                        break;
                    case "N":
                        isValid = true;
                        Console.WriteLine("FileUploads will be used as Db");
                        dbName = "FileUploads";
                        break;
                    default:
                        Console.WriteLine("response must be 'Y' or 'N'");
                        break;
                }
            }
        }

        while (string.IsNullOrWhiteSpace(dbName))
        {
            Console.WriteLine("Enter DbName: ");
            dbName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dbName))
            {
                Console.WriteLine("Please enter a DbName.");
            }
        }

        return dbName;
    }

    public string? GetConfigFile(Dictionary<string, string> arguments)
    {
        bool isValid = arguments.TryGetValue("config", out var filePath);
        bool includeConfig = isValid;

        while (!isValid)
        {
            Console.WriteLine("No config file detected, did you want to include one?(Y/N)");
            string? response = Console.ReadLine();
            if (response != null)
            {
                switch (response.ToUpper())
                {
                    case "Y":
                        includeConfig = true;
                        isValid = true;
                        break;
                    case "N":
                        isValid = true;
                        break;
                    default:
                        Console.WriteLine("response must be 'Y' or 'N'");
                        break;
                }
            }
        }

        if (includeConfig)
        {
            do
            {
                filePath = GetFilePath(filePath);
                if (!string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = Path.ChangeExtension(filePath, ".json");
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"Could not find json file at {filePath}");
                        filePath = null;
                    }
                }

            }
            while (filePath == null);

        }

        return filePath;
    }

    public bool GetDeletePrevious(Dictionary<string, string> arguments, string tableName)
    {
        bool deletePrevious = false;
        bool isValid = arguments.TryGetValue("delete", out var deletePrevString);
        isValid = isValid && bool.TryParse(deletePrevString, out deletePrevious);

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
    public string GetTableName(Dictionary<string, string> arguments)
    {
        arguments.TryGetValue("table", out var tableName);

        while (string.IsNullOrWhiteSpace(tableName))
        {
            Console.WriteLine("Enter Name of Table to Import To: ");
            tableName = Console.ReadLine();
            if (tableName == null)
            {
                Console.WriteLine("Please enter a table name.");
            }
        }

        return tableName;
    }
}
