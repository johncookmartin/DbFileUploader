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

    public async Task SaveData(Dictionary<string, object> records, dynamic? parameters)
    {
        // Implementation for saving data goes here
        // This is a placeholder method and should be implemented as needed
        await Task.CompletedTask;
    }
}
