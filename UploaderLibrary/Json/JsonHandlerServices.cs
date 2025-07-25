using System.Text.Json;

namespace UploaderLibrary.Json;
public class JsonHandlerServices : IHandlerServices<List<Dictionary<string, object?>>>
{
    public List<Dictionary<string, object?>> FormatData(string filePath, dynamic parameters)
    {
        List<Dictionary<string, object?>> importData = ImportData(filePath);

        List<string> targetFields = GetTargetFields(parameters);
        bool recursiveSearch = GetRecursiveSearch(parameters);

        foreach (var record in importData)
        {
            Dictionary<string, object?> filteredRecord = new Dictionary<string, object?>();
            foreach (var kvp in record)
            {

            }
            record.Clear();
            foreach (var kvp in filteredRecord)
            {
                record[kvp.Key] = kvp.Value;
            }
        }

        return importData;

    }

    private List<string> GetTargetFields(dynamic parameters)
    {
        List<string> targetFields = new List<string>();

        dynamic type = parameters.GetType();
        dynamic prop = type.GetProperty("TargetFields");
        if (prop != null)
        {
            dynamic value = prop.GetValue(parameters, null);
            targetFields = (value is List<string> list) ? list : new List<string>();

        }

        return targetFields;
    }

    private bool GetRecursiveSearch(dynamic parameters)
    {
        bool recursiveSearch = false;
        dynamic type = parameters.GetType();
        dynamic prop = type.GetProperty("RecursiveSearch");
        if (prop != null)
        {
            dynamic value = prop.GetValue(parameters, null);
            recursiveSearch = Convert.ToBoolean(value);
        }
        return recursiveSearch;
    }

    private List<Dictionary<string, object?>> ImportData(string filePath)
    {
        using StreamReader reader = new StreamReader(filePath);
        string json = reader.ReadToEnd();

        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object && root.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Invalid JSON format: Expected an object or an array of objects.");
        }

        List<Dictionary<string, object?>> importData = new();

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("Invalid JSON format: Expected item in array to be a JSON object.");
                }

                importData.Add(ConvertJsonObject(element));
            }
        }
        else
        {
            importData.Add(ConvertJsonObject(root));
        }

        if (importData.Count == 0)
        {
            throw new JsonException("No data found in the JSON file.");
        }

        return importData;
    }

    private List<object?> ConvertJsonArray(JsonElement root)
    {
        List<object?> arrayData = new();
        foreach (var element in root.EnumerateArray())
        {
            arrayData.Add(ConvertJsonObject(element));
        }
        return arrayData;
    }

    private Dictionary<string, object?> ConvertJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ConvertJsonValue(property.Value);
        }
        return result;
    }

    private object? ConvertJsonValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String when value.TryGetDateTime(out DateTime dateTime) => dateTime,
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt32(out var i) => i,
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => ConvertJsonArray(value),
            JsonValueKind.Object => ConvertJsonObject(value),
            _ => value.ToString(),
        };
    }

    private Dictionary<string, object?> RecursiveSearch(IEnumerable<object?> element, List<string> targetFields)
    {
        Dictionary<string, object?> foundFields = new Dictionary<string, object?>();
        foreach (var item in element)
        {
            if (item is KeyValuePair<string, object?> kvp)
            {
                if (kvp.Value is IEnumerable<object?> nestedEnumerable && kvp.Value is not string)
                {
                    var nestedFields = RecursiveSearch(nestedEnumerable, targetFields);
                    foreach (var nestedKvp in nestedFields)
                    {
                        foundFields.TryAdd(nestedKvp.Key, nestedKvp.Value);

                    }
                }
                else if (targetFields.Contains(kvp.Key.Trim()))
                {
                    foundFields.TryAdd(kvp.Key, kvp.Value);
                }
            }
            else if (item is IEnumerable<object?> nestedEnumerable)
            {
                var nestedFields = RecursiveSearch(nestedEnumerable, targetFields);
                foreach (var nestedField in nestedFields)
                {
                    foundFields[nestedField.Key] = nestedField.Value;
                }
            }

        }
        return foundFields;
    }
}
