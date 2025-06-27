using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UploaderLibrary;

namespace DbFileUploader.ConsoleUI;
public class JsonHandler : InputHandler
{
    private readonly bool _isArray;
    private readonly IHandlerServices<Dictionary<string, object>> _handler;
    private IUploaderSaveHandler<Dictionary<string, object>> _uploader;
    public Dictionary<string, object> JsonData { get; set; } = new Dictionary<string, object>();

    public JsonHandler(Dictionary<string, string> arguments) : base(arguments)
    {
        var services = JsonDependencyInjection.ConfigureServices(_config);
        var provider = services.BuildServiceProvider();

        //Get Operator Input
        _isArray = GetIsArray();
        _handler = provider.GetRequiredService<IHandlerServices<Dictionary<string, object>>>();
        JsonData = GetJsonData(arguments["file"]);

        //Processing File
        var db = provider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = provider.GetRequiredService<IUploaderData>();
        _uploader = provider.GetRequiredService<IUploaderSaveHandler<Dictionary<string, object>>>();

        //using StreamReader reader = new StreamReader(jsonFilePath);
        //string json = reader.ReadToEnd();

        //var result = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        //Console.WriteLine();
    }

    public Dictionary<string, object> GetJsonData(string jsonFilePath)
    {
        Dictionary<string, object> jsonData = new Dictionary<string, object>();
        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"JSON file not found: {jsonFilePath}");
            return jsonData;
        }

        jsonData = _handler.FormatData(jsonFilePath, new { isArray = _isArray });
        if (jsonData.Count == 0)
        {
            Console.WriteLine("JSON file is empty or not formatted correctly");
        }
        return jsonData;
    }

    private bool GetIsArray()
    {
        bool isValid = false;
        bool isArray = false;

        var configSection = _config.GetSection("JsonDetails:IsArray");
        if (configSection.Exists())
        {
            isArray = configSection.Get<bool>();
            isValid = true;
        }

        while (!isValid)
        {
            Console.WriteLine($"Is the Json file an array of objects?(Y/N)");
            string? response = Console.ReadLine();
            if (response != null)
            {
                switch (response.ToUpper())
                {
                    case "Y":
                        isArray = true;
                        isValid = true;
                        break;
                    case "N":
                        isArray = false;
                        isValid = true;
                        break;
                    default:
                        Console.WriteLine("response must be 'Y' or 'N'");
                        break;
                }
            }
        }

        return isArray;
    }

    internal async Task<bool> UploadFile()
    {
        throw new NotImplementedException();
    }
}
