using Dapper;
using DbFileUploaderDataAccessLibrary.Data;
using DbFileUploaderDataAccessLibrary.Models;
using Microsoft.Extensions.Logging;

namespace JsonUploaderLibrary;
public class UploaderSaveHandler
{
    private readonly IUploaderData _db;
    private readonly TableImportSchemaModel _tableImportSchema;
    private readonly ILogger<UploaderSaveHandler> _logger;

    public UploaderSaveHandler(IUploaderData db, TableImportSchemaModel tableImportSchema, ILogger<UploaderSaveHandler> logger)
    {
        _db = db;
        _tableImportSchema = tableImportSchema;
        _logger = logger;
    }

    public async Task DeleteTableData(string? tableName = null)
    {
        tableName = tableName ?? _tableImportSchema.TableName;
        _logger.LogInformation($"Deleting all previous data from {tableName}");
        await _db.DeleteTableData(tableName);
    }

    public async Task SaveData(Dictionary<string, object> data, int? colNum = null)
    {
        colNum = colNum ?? _tableImportSchema.Columns?.Count ?? data.Count();

        var parameters = new DynamicParameters();

        if (colNum == null && _tableImportSchema.Columns == null)
        {
            foreach (var element in data)
            {

            }
        }
    }
}
