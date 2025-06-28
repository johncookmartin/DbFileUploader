using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploaderLibrary.Json;
public class JsonUploaderSaveHandler : IUploaderSaveHandler<Dictionary<string, object>>
{
    private readonly IUploaderData _db;
    private readonly IConfiguration _config;
    private readonly ILogger<JsonUploaderSaveHandler> _logger;
    private readonly Dictionary<string, object?> _recordsToSave;

    public JsonUploaderSaveHandler(IUploaderData db, IConfiguration config, ILogger<JsonUploaderSaveHandler> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
        _recordsToSave = new Dictionary<string, object?>();
    }
    public async Task DeleteTableData()
    {
        string tableName = _config.GetValue<string>("TableName")!;
        string dbName = _config.GetValue<string>("DbName")!;
        _logger.LogInformation($"Deleting all previous data from {tableName}");
        await _db.DeleteTableData(tableName, dbName);
    }

    public async Task SaveData(Dictionary<string, object?> records, dynamic parameters)
    {
        List<string> targetFields = GetTargetFields(parameters);
        bool recursiveSearch = GetRecursiveSearch(parameters);

        foreach (var kvp in records)
        {
            var value = kvp.Value;
            if (value is IEnumerable<object?> enumerable && value is not string)
            {
                if (recursiveSearch)
                {
                    //search recursively for target fields in nested objects
                }
            }
            else if (targetFields.Count > 0)
            {
                if (targetFields.Contains(kvp.Key.Trim()))
                {
                    _recordsToSave[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                _recordsToSave[kvp.Key] = kvp.Value;
            }
        }

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


    private Dictionary<string, object?> GetTargetRecords(List<string> targetFields)
    {
        Dictionary<string, object?> targetRecords = new();
        foreach (var field in targetFields)
        {
            if (records.ContainsKey(field))
            {
                targetRecords[field] = records[field];
            }
            else if (recursiveSearch)
            {
                foreach (var kvp in records)
                {
                    if (kvp.Value is Dictionary<string, object?> nestedDict)
                    {
                        var nestedRecord = GetTargetRecords(new List<string> { field }, nestedDict, recursiveSearch);
                        if (nestedRecord.Count > 0)
                        {
                            targetRecords[field] = nestedRecord;
                        }
                    }
                }
            }
        }
        return targetRecords;
    }
}
