namespace UploaderLibrary.Json;
public class JsonHandlerServices : IHandlerServices<Dictionary<string, object>>
{
    public Dictionary<string, object> FormatData(string filePath, dynamic? parameters)
    {
        // Example formatting: Convert all keys to lowercase
        var formattedData = new Dictionary<string, object>();
        //foreach (var kvp in data)
        //{
        //    formattedData[kvp.Key.ToLower()] = kvp.Value;
        //}
        return formattedData;
    }
}
