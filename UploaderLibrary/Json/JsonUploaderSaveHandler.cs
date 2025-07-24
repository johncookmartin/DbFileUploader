using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploaderLibrary.Json;
public class JsonUploaderSaveHandler : IUploaderSaveHandler<Dictionary<string, object?>>
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

    public async Task CreateData(int tableId)
    {
        string tableName = _config.GetValue<string>("TableName")!;
        string dbName = _config.GetValue<string>("DbName")!;
        _logger.LogInformation($"Creating table {tableName} in database {dbName}");
        await _db.CreateData(tableId);
    }

    public async Task DeleteTableData()
    {
        string tableName = _config.GetValue<string>("TableName")!;
        string dbName = _config.GetValue<string>("DbName")!;
        _logger.LogInformation($"Deleting all previous data from {tableName}");
        await _db.DeleteTableData(tableName, dbName);
    }

    public async Task<int> SaveData(Dictionary<string, object?> records, dynamic parameters)
    {
        string tableName = _config.GetValue<string>("TableName")!;
        string dbName = _config.GetValue<string>("DbName")!;
        await _db.SaveToExisting(tableName, dbName, records);
        return 0;

    }
}
