using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploaderLibrary.Json;
public class JsonUploaderSaveHandler : IUploaderSaveHandler<Dictionary<string, object>>
{
    private readonly IUploaderData _db;
    private readonly IConfiguration _config;
    private readonly ILogger<JsonUploaderSaveHandler> _logger;

    public JsonUploaderSaveHandler(IUploaderData db, IConfiguration config, ILogger<JsonUploaderSaveHandler> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }
    public async Task DeleteTableData()
    {
        string tableName = _config.GetValue<string>("TableName")!;
        string dbName = _config.GetValue<string>("DbName")!;
        _logger.LogInformation($"Deleting all previous data from {tableName}");
        await _db.DeleteTableData(tableName, dbName);
    }

    public async Task SaveData(Dictionary<string, object?> records, dynamic? parameters)
    {
        List<string> targetFields = parameters!.GetValue("TargetFields");
        await SaveDataHelper(records, _config.GetValue<string>("TableName")!, targetFields);

    }

    private async Task SaveDataHelper(Dictionary<string, object?> records, string tableName, List<string> targetFields, int? rowId = null)
    {
        if (records == null || records.Count == 0)
        {
            _logger.LogWarning("No records to save. Skipping save operation.");
            return;
        }

        Dictionary<string, Dictionary<string, object?>> objectsToSave = new();
        Dictionary<string, List<object?>> listsToSave = new();
        Dictionary<string, object> rowData = new();
        foreach (var kvp in records)
        {
            string key = kvp.Key.Trim();
            object? value = kvp.Value;

            if (value == null)
            {
                _logger.LogWarning($"Skipping key {key} with null value.");
                continue;
            }
            else if (value is Dictionary<string, object>)
            {
                var nextRecord = (Dictionary<string, object?>)value;
                objectsToSave.Add($"{tableName}{key}", nextRecord);
                rowData.Add("has_" + key, true);
            }
            else if (value is List<object>)
            {
                var nextRecords = (List<object?>)value;
                listsToSave.Add($"{tableName}{key}", nextRecords);
                rowData.Add("has_" + key, true);
            }
            else if (targetFields.Count > 0)
            {
                if (targetFields.Contains(key))
                {
                    rowData.Add(key, value);
                }
            }
            else
            {
                rowData.Add(key, value);
            }
        }

        if (rowData.Count > 0)
        {
            int id = await _db.SaveData(_config.GetValue<string>("DbName")!, tableName, rowData);
            if (id > 0)
            {
                _logger.LogInformation($"Saved row with ID {id} to {tableName}");
                foreach (var kvp in objectsToSave)
                {
                    var subRecord = kvp.Value;
                    await SaveDataHelper(subRecord, kvp.Key, targetFields, id);
                }
                foreach (var kvp in listsToSave)
                {
                    SaveSubList(kvp.Key, kvp.Value, id, tableName, targetFields);
                }
            }
            else
            {
                _logger.LogWarning($"Failed to save row to {tableName}");
            }
        }
    }

    private void SaveSubList(string key, List<object?> value, int id, string tableName, List<string> targetFields)
    {
        foreach (var item in value)
        {
            if (item is Dictionary<string, object> subRecord)
            {
                SaveDataHelper(subRecord, key, targetFields, id);
            }
            else if (item is List<object> subList)
            {
                SaveSubList(key, subList, id, tableName, targetFields);
            }
            else
            {
                _logger.LogWarning($"Skipping unsupported type {item.GetType()} for key {key}");
            }
        }
    }
}
