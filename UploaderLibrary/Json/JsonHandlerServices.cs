using System.Text.Json;

namespace UploaderLibrary.Json;
public class JsonHandlerServices : IHandlerServices<List<Dictionary<string, object>>>
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
                if (targetFields.Count > 0)
                {
                    if (kvp.Value is IEnumerable<object?> enumerable && kvp.Value is not string)
                    {
                        if (recursiveSearch)
                        {
                            Dictionary<string, object?> foundFields = RecursiveSearch(enumerable, targetFields);
                            foreach (var foundKvp in foundFields)
                            {
                                //defer to non-recursive search if the key already exists
                                filteredRecord.TryAdd(foundKvp.Key, foundKvp.Value);
                            }
                        }
                    }
                    else if (targetFields.Contains(kvp.Key.Trim()))
                    {
                        filteredRecord[kvp.Key] = kvp.Value;
                    }
                }
                else if (kvp.Value is not IEnumerable<object?> || kvp.Value is string)
                {
                    filteredRecord[kvp.Key] = kvp.Value;
                }
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
        bool isArray;
        List<Dictionary<string, object?>> importData = new List<Dictionary<string, object?>>();

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
            importData = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json) ?? new List<Dictionary<string, object?>>();
        }
        else
        {
            var singleObject = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
            if (singleObject != null)
            {
                importData.Add(singleObject);
            }
        }

        if (importData.Count == 0)
        {
            throw new JsonException("No data found in the JSON file.");
        }

        return importData;
    }

    private Dictionary<string, object?> RecursiveSearch(IEnumerable<object?> enumerable, List<string> targetFields)
    {
        Dictionary<string, object?> foundFields = new Dictionary<string, object?>();
        foreach (var item in enumerable)
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
                foreach (var kvp in nestedFields)
                {
                    foundFields[kvp.Key] = kvp.Value;
                }
            }

        }
        return foundFields;
    }
}
