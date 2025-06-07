using Microsoft.Extensions.Configuration;

namespace DbFileUploader.ConsoleUI;
public class JsonHandler : InputHandler
{
    public bool isArray { get; set; } = false;
    public JsonHandler(string[] args) : base(args)
    {
        //using StreamReader reader = new StreamReader(jsonFilePath);
        //string json = reader.ReadToEnd();

        //var result = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        //Console.WriteLine();

        GetIsArray();

    }

    private void GetIsArray()
    {
        bool isValid = false;

        var configSection = config.GetSection("JsonDetails:IsArray");
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
    }

    internal async Task<bool> UploadFile()
    {
        throw new NotImplementedException();
    }
}
