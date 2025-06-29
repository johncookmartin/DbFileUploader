using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UploaderLibrary;

namespace DbFileUploader.ConsoleUI;
public class JsonHandler : InputHandler
{
    private readonly IHandlerServices<List<Dictionary<string, object?>>> _handler;
    private IUploaderSaveHandler<Dictionary<string, object?>> _uploader;
    public List<Dictionary<string, object?>> JsonData { get; set; } = new();
    public List<string> TargetFields { get; set; } = new List<string>();
    public bool IsRecursive { get; set; }

    public JsonHandler(Dictionary<string, string> arguments) : base(arguments)
    {
        var services = JsonDependencyInjection.ConfigureServices(_config);
        var provider = services.BuildServiceProvider();

        //Get Data
        TargetFields = GetTargetFields(arguments);
        IsRecursive = GetIsRecursive(arguments);
        _handler = provider.GetRequiredService<IHandlerServices<List<Dictionary<string, object?>>>>();
        JsonData = GetJsonData(arguments["file"]);

        //Preparing to process file
        var db = provider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = provider.GetRequiredService<IUploaderData>();
        _uploader = provider.GetRequiredService<IUploaderSaveHandler<Dictionary<string, object?>>>();
    }

    public bool GetIsRecursive(Dictionary<string, string> arguments)
    {
        bool isRecursive = false;

        if (arguments.TryGetValue("recursive", out var argValue))
        {
            isRecursive = true;
        }
        else
        {
            isRecursive = _config.GetValue<bool>("JsonDetails:IsRecursive", false);
        }

        return isRecursive;

    }

    public List<Dictionary<string, object?>> GetJsonData(string jsonFilePath)
    {
        List<Dictionary<string, object?>> jsonData = new();
        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"JSON file not found: {jsonFilePath}");
            return jsonData;
        }

        jsonData = _handler.FormatData(jsonFilePath, new { TargetFields });
        if (jsonData.Count == 0)
        {
            Console.WriteLine("JSON file is empty or not formatted correctly");
        }
        return jsonData;
    }

    public List<string> GetTargetFields(Dictionary<string, string> arguments)
    {
        List<string> targetFields = new List<string>();

        if (arguments.TryGetValue("fields", out var argFields))
        {
            if (!string.IsNullOrWhiteSpace(argFields))
            {
                targetFields = argFields.Split(',').Select(f => f.Trim()).ToList();
                Console.WriteLine("Using target fields from arguments: " + string.Join(", ", targetFields));
                return targetFields;
            }
        }

        var configSection = _config.GetSection("JsonDetails:TargetFields");
        if (configSection.Exists())
        {
            targetFields = configSection.Get<List<string>>() ?? new List<string>();
            Console.WriteLine("Using target fields from configuration: " + string.Join(", ", targetFields));
            return targetFields;
        }

        Console.WriteLine("No target fields specified in configuration. Using all fields from JSON data.");
        return targetFields;
    }

    public async Task<bool> UploadFile()
    {
        if (JsonData.Count == 0)
        {
            Console.WriteLine("No data to upload. Please check the JSON file.");
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

        foreach (var record in JsonData)
        {
            try
            {
                await _uploader.SaveData(record, new { });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                return false;
            }
        }


        return true;
    }
}
