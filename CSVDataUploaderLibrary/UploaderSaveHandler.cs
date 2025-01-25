using CSVDataUploaderDataAccessLibrary.Data;
using CSVDataUploaderDataAccessLibrary.Models;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CSVDataUploaderLibrary;
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

    public async Task SaveData(List<string[]> records, int startingIndex)
    {
        int colNum = _tableImportSchema.Columns.Count;
        int insertCount = 0;

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
                    parameters.Add($"@{colName}", GenerateParamValue(record, rowIndex, colIndex));
                }

                int id = await _db.SaveRow(parameters);
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

    private dynamic? GenerateParamValue(string[] record, int rowIndex, int colIndex)
    {
        string value = record[colIndex].ToString().Trim('"');
        string colType = _tableImportSchema.Columns[colIndex].DataType;
        bool isNullable = _tableImportSchema.Columns[colIndex].IsNullable;
        bool isValid = false;
        string warningMessage = $"Warning: {colType} value expected but not recieved in row: {rowIndex} col: {colIndex}. " +
                    $"Will import default value.";

        if (colType.Contains("datetime", StringComparison.OrdinalIgnoreCase))
        {
            isValid = DateTime.TryParse(value, out DateTime dateTimeValue);
            if (!isValid)
            {
                if (!isNullable)
                {
                    _logger.LogWarning(warningMessage);
                    return DateTime.MinValue;
                }
                else
                {
                    return null;
                }

            }
            return dateTimeValue;
        }
        else if (colType.Contains("decimal", StringComparison.OrdinalIgnoreCase))
        {
            isValid = decimal.TryParse(value, out decimal decimalValue);
            if (!isValid)
            {
                if (!isNullable)
                {
                    _logger.LogWarning(warningMessage);
                    return decimal.Zero;
                }
                else
                {
                    return null;
                }
            }
            return decimalValue;
        }
        else if (colType.Contains("int", StringComparison.OrdinalIgnoreCase))
        {
            if (!isNullable)
            {
                _logger.LogWarning(warningMessage);
                return 0;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return value;
        }
    }
}
