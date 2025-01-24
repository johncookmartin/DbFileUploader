using CSVDataUploaderDataAccessLibrary.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CSVDataUploaderDataAccessLibrary.Data;
public class UploaderData : IUploaderData
{
    private readonly ISqlDataAccess _db;
    private readonly TableImportSchemaModel _tableImportSchema;
    private readonly ILogger<UploaderData> _logger;

    public UploaderData(ISqlDataAccess db, TableImportSchemaModel tableImportSchema, ILogger<UploaderData> logger)
    {
        _db = db;
        _tableImportSchema = tableImportSchema;
        _logger = logger;
    }
    public async Task DeleteData(string? tableName = null)
    {
        tableName = tableName ?? _tableImportSchema.TableName;
        await _db.ExecuteDataAsync<dynamic>($@"DELETE FROM {tableName};", new { });
        _logger.LogInformation($"Deleting all previous data from {tableName}");
    }

    public async Task SaveData(List<string[]> records, int startingIndex)
    {

        int colNum = _tableImportSchema.Columns.Count();
        int insertCount = 0;

        //skip headerlines
        for (int rowIndex = startingIndex; rowIndex < records.Count; rowIndex++)
        {
            var record = records[rowIndex];

            if (record.Length != colNum)
            {
                _logger.LogWarning($"Warning: cannot import {rowIndex} due to column mismatch");
            }
            else
            {
                var parameters = new DynamicParameters();

                for (int colIndex = 0; colIndex < colNum; colIndex++)
                {
                    string colName = _tableImportSchema.Columns[colIndex].Name;
                    string value = record[colIndex].ToString().Trim('"');
                    string colType = _tableImportSchema.Columns[colIndex].DataType;

                    parameters.Add($"@{colName}", GenerateParamValue(value, colType, rowIndex, colIndex));
                }

                int id = (await _db.QueryDataAsync<int, dynamic>(GenerateInsertQuery(), parameters)).First();
                if (id > 0)
                {
                    insertCount++;
                }
                else
                {
                    _logger.LogWarning($"Warning: unknown error trying to save row: {rowIndex}");
                }
            }
        }

        _logger.LogInformation($"Saved {insertCount} rows from original {records.Count()}");
    }

    private string GenerateInsertQuery()
    {
        string sql;

        List<ColumnDefinitionModel> columns = _tableImportSchema.Columns;

        var columnsBuilder = new StringBuilder();
        var paramsBuilder = new StringBuilder();

        for (int i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                columnsBuilder.Append(", ");
                paramsBuilder.Append(", ");
            }

            columnsBuilder.Append(columns[i].Name);
            paramsBuilder.Append($"@{columns[i].Name}");
        }

        sql = $@"INSERT INTO {_tableImportSchema.TableName} ({columnsBuilder}) VALUES ({paramsBuilder}); SELECT SCOPE_IDENTITY();";

        return sql;
    }

    private dynamic GenerateParamValue(string value, string colType, int rowIndex, int colIndex)
    {
        bool isValid = false;
        string warningMessage = $"Warning: {colType} value expected but not recieved in row: {rowIndex} col: {colIndex}. " +
                    $"Will import default value.";

        if (colType.Contains("datetime", StringComparison.OrdinalIgnoreCase))
        {
            isValid = DateTime.TryParse(value, out DateTime dateTimeValue);
            if (!isValid)
            {
                _logger.LogWarning(warningMessage);
                dateTimeValue = DateTime.MinValue;
            }
            return dateTimeValue;
        }
        else if (colType.Contains("decimal", StringComparison.OrdinalIgnoreCase))
        {
            isValid = decimal.TryParse(value, out decimal decimalValue);
            if (!isValid)
            {
                _logger.LogWarning(warningMessage);
                decimalValue = decimal.Zero;
            }
            return decimalValue;
        }
        else if (colType.Contains("int", StringComparison.OrdinalIgnoreCase))
        {
            isValid = int.TryParse(value, out int intValue);
            if (!isValid)
            {
                _logger.LogWarning(warningMessage);
                intValue = 0;
            }
            return intValue;
        }
        else
        {
            return value;
        }
    }
}
