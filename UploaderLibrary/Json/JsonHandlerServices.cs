using System.Text.Json;

namespace UploaderLibrary.Json;
public class JsonHandlerServices : IHandlerServices<List<Dictionary<string, object>>>
{
    public List<Dictionary<string, object>> FormatData(string filePath, dynamic? parameters)
    {
        bool isArray;
        var formattedData = new List<Dictionary<string, object>>();

        using StreamReader reader = new StreamReader(filePath);
        string json = reader.ReadToEnd();

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            isArray = true;
        }
        else if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Invalid JSON format: Expected an object or an array of objects.");
        }
        else
        {
            isArray = false;
        }

        if (isArray)
        {
            formattedData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new List<Dictionary<string, object>>();
        }
        else
        {
            var singleObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (singleObject != null)
            {
                formattedData.Add(singleObject);
            }
        }

        return formattedData;
    }
}
