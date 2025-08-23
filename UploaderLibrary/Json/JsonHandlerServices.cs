using System.Text.Json;

namespace UploaderLibrary.Json;
public class JsonHandlerServices : IHandlerServices<List<Dictionary<string, object?>>>
{
    public List<Dictionary<string, object?>> FormatData(string filePath, dynamic parameters)
    {
        List<Dictionary<string, object?>> importData = ImportData(filePath);

        List<string> targetFields = GetTargetFields(parameters);
        HashSet<string> targetFieldsSet = new HashSet<string>(targetFields.Select(f => f.Trim()), StringComparer.OrdinalIgnoreCase);
        bool recursiveSearch = GetRecursiveSearch(parameters);

        foreach (var record in importData)
        {
            Dictionary<string, object?> filteredRecord = new Dictionary<string, object?>();
            foreach (var kvp in record)
            {
                if (targetFieldsSet.Count > 0)
                {
                    if (kvp.Value is IDictionary<string, object?> dict && recursiveSearch)
                    {
                        Dictionary<string, object?> nestedRecords = RecursiveDictSearch(kvp.Key, dict, targetFieldsSet);
                        foreach (var nestedRecord in nestedRecords)
                        {
                            filteredRecord.TryAdd(nestedRecord.Key, nestedRecord.Value);
                        }
                    }
                    else if (kvp.Value is IEnumerable<object?> enumerable && kvp.Value is not string && recursiveSearch)
                    {
                        Dictionary<string, object?> nestedRecords = RecursiveSearch(kvp.Key, enumerable, targetFieldsSet);
                        foreach (var nestedRecord in nestedRecords)
                        {
                            filteredRecord.TryAdd(nestedRecord.Key, nestedRecord.Value);
                        }
                    }
                    else if (targetFieldsSet.Contains(kvp.Key.Trim()))
                    {
                        if (kvp.Value != null)
                        {
                            Type type = kvp.Value.GetType();
                            if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal))
                            {
                                filteredRecord.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }
                }
                else
                {
                    if (kvp.Value != null)
                    {
                        Type type = kvp.Value.GetType();
                        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal))
                        {
                            filteredRecord.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            // Overwrite the original record with the filtered one
            record.Clear();
            foreach (var kvp in filteredRecord)
            {
                record[kvp.Key] = kvp.Value;
            }
        }

        return importData;

    }

    private Dictionary<string, object?> RecursiveDictSearch(string key, IDictionary<string, object?> dict, HashSet<string> targetFieldsSet)
    {
        Dictionary<string, object?> foundRecords = new();

        foreach (var kvp in dict)
        {
            if (kvp.Value is IDictionary<string, object?> innerDict)
            {
                Dictionary<string, object?> nestedRecords = RecursiveDictSearch(kvp.Key, innerDict, targetFieldsSet);
                foreach (var nestedRecord in nestedRecords)
                {
                    foundRecords.TryAdd(nestedRecord.Key, nestedRecord.Value);
                }
            }
            else if (kvp.Value is IEnumerable<object?> enumerable && kvp.Value is not string)
            {
                Dictionary<string, object?> nestedRecords = RecursiveSearch(kvp.Key, enumerable, targetFieldsSet);
                foreach (var nestedRecord in nestedRecords)
                {
                    foundRecords.TryAdd(nestedRecord.Key, nestedRecord.Value);
                }
            }
            else if (targetFieldsSet.Contains(kvp.Key.Trim()))
            {
                if (kvp.Value != null)
                {
                    Type type = kvp.Value.GetType();
                    if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal))
                    {
                        foundRecords.Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }
        return foundRecords;
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
        dynamic prop = type.GetProperty("IsRecursive");
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
            if (element.ValueKind != JsonValueKind.Object)
            {
                arrayData.Add(ConvertJsonValue(element));
            }
            else
            {
                arrayData.Add(ConvertJsonObject(element));
            }

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

    private Dictionary<string, object?> RecursiveSearch(string key, IEnumerable<object?> value, HashSet<string> targetFieldsSet)
    {

        Dictionary<string, object?> foundFields = new Dictionary<string, object?>();

        foreach (var item in value)
        {
            switch (item)
            {
                case IDictionary<string, object?> dict:
                    foreach (var innerKvp in dict)
                    {
                        if (innerKvp.Value is IEnumerable<object?> subEnumerable && innerKvp.Value is not string)
                        {
                            var nested = RecursiveSearch(innerKvp.Key, subEnumerable, targetFieldsSet);
                            foreach (var result in nested)
                            {
                                // for now, if duplicates are found we will keep the first one
                                foundFields.TryAdd(result.Key, result.Value);
                            }
                        }
                        else if (targetFieldsSet.Contains(innerKvp.Key.Trim()))
                        {
                            // for now, if duplicates are found we will keep the first one
                            foundFields.TryAdd(innerKvp.Key, innerKvp.Value);
                        }
                    }
                    break;

                case IEnumerable<object?> nestedEnumerable when item is not string:
                    var nonNullItem = nestedEnumerable.FirstOrDefault(x => x != null);

                    if (nonNullItem is not null)
                    {
                        Type type = nonNullItem.GetType();
                        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal))
                        {
                            if (targetFieldsSet.Contains(key.Trim()))
                            {
                                // for now, we just convert the list to a string representation
                                foundFields.TryAdd(key, string.Join(", ", nestedEnumerable));
                            }
                        }
                        else
                        {
                            var nestedFromList = RecursiveSearch(key, nestedEnumerable, targetFieldsSet);
                            foreach (var nestedKvp in nestedFromList)
                            {
                                // for now, if duplicates are found we will keep the first one
                                foundFields.TryAdd(nestedKvp.Key, nestedKvp.Value);
                            }
                        }
                    }
                    break;
            }
        }

        return foundFields;
    }
}
