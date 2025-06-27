using DbFileUploader.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbFileUploader.ConsoleUI;
public class JsonHandler : InputHandler
{
    private readonly bool _isArray;
    public Dictionary<string, object> JsonData { get; set; } = new Dictionary<string, object>();
    public JsonHandler(Dictionary<string, string> arguments) : base(arguments)
    {
        var services = JsonDependencyInjection.ConfigureServices(_config);
        var provider = services.BuildServiceProvider();

        //Get Operator Input
        _isArray = GetIsArray();
        JsonData = GetJsonData(arguments["file"]);

        //using StreamReader reader = new StreamReader(jsonFilePath);
        //string json = reader.ReadToEnd();

        //var result = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        //Console.WriteLine();

        GetIsArray();

    }

    public Dictionary<string, object> GetJsonData(string jsonFilePath)
    {
        Dictionary<string, object> jsonData = new Dictionary<string, object>();
        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"JSON file not found: {jsonFilePath}");
            return jsonData;
        }

        jsonData = JsonHandlerServices.FormatJson(jsonFilePath, _isArray);
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
