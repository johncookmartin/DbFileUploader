using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UploaderLibrary;

namespace DbFileUploader.ConsoleUI;
public class JsonHandler : InputHandler
{
    private readonly IHandlerServices<List<Dictionary<string, object>>> _handler;
    private IUploaderSaveHandler<Dictionary<string, object>> _uploader;
    public List<Dictionary<string, object>> JsonData { get; set; } = new();

    public JsonHandler(Dictionary<string, string> arguments) : base(arguments)
    {
        var services = JsonDependencyInjection.ConfigureServices(_config);
        var provider = services.BuildServiceProvider();

        //Get Data
        _handler = provider.GetRequiredService<IHandlerServices<List<Dictionary<string, object>>>>();
        JsonData = GetJsonData(arguments["file"]);

        //Preparing to process file
        var db = provider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = provider.GetRequiredService<IUploaderData>();
        _uploader = provider.GetRequiredService<IUploaderSaveHandler<Dictionary<string, object>>>();
    }

    public List<Dictionary<string, object>> GetJsonData(string jsonFilePath)
    {
        List<Dictionary<string, object>> jsonData = new();
        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"JSON file not found: {jsonFilePath}");
            return jsonData;
        }

        jsonData = _handler.FormatData(jsonFilePath, new { });
        if (jsonData.Count == 0)
        {
            Console.WriteLine("JSON file is empty or not formatted correctly");
        }
        return jsonData;
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
