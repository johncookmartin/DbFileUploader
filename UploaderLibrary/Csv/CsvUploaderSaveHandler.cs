using DbFileUploaderDataAccessLibrary.Data;
using DbFileUploaderDataAccessLibraryModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploaderLibrary.Csv;
public class CsvUploaderSaveHandler
{
    private readonly IUploaderData _db;
    private readonly IConfiguration _tableConfig;
    private readonly ILogger<CsvUploaderSaveHandler> _logger;

    public CsvUploaderSaveHandler(IUploaderData db, IConfiguration tableConfig, ILogger<CsvUploaderSaveHandler> logger)
    {
        _db = db;
        _tableConfig = tableConfig;
        _logger = logger;
    }

    public async Task SaveData(List<string[]> records, int startingIndex, bool hasHeaders)
    {
        bool hasDefinition = _tableConfig.GetSection("Columns").Exists();
        if (hasDefinition)
        {
            await SaveDataDefined(records, startingIndex);
        }
        else
        {
            await SaveDataDynamically(records, startingIndex, hasHeaders);
        }

    }

    private async Task SaveDataDefined(List<string[]> records, int startingIndex)
    {
        ColumnDefinitionModel[] columns = _tableConfig.GetSection("Columns").Get<ColumnDefinitionModel[]>()!;
        bool hasIdentity = _tableConfig.GetSection("CsvDetails").GetValue<bool>("HasIdentity");
        int colNum = columns.Length;
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
                Dictionary<string, object?> rowData = new Dictionary<string, object?>();

                for (int colIndex = 0; colIndex < colNum; colIndex++)
                {
                    string colName = columns[colIndex].Name;
                    object? colValue = GenerateParamValueDefined(record, columns, rowIndex, colIndex);
                    rowData[colName] = colValue;
                }

                int id = await _db.SaveToExisting(
                    _tableConfig.GetValue<string>("DbName")!,
                    _tableConfig.GetValue<string>("TableName")!,
                    rowData);

                if (id > 0 || !hasIdentity)
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

    private async Task SaveDataDynamically(List<string[]> records, int startingIndex, bool hasHeaders)
    {
        int insertCount = 0;
        List<string> columns = new List<string>();

        for (int rowIndex = 0; rowIndex < records.Count; rowIndex++)
        {
            Dictionary<string, object?> rowData = new Dictionary<string, object?>();

            for (int colIndex = 0; colIndex < records[rowIndex].Length; colIndex++)
            {
                if (hasHeaders && rowIndex == 0)
                {
                    columns.Add(records[rowIndex][colIndex]);
                    _logger.LogInformation($"Generated column name {records[rowIndex][colIndex]} from header");
                }
                else
                {
                    string colName = colIndex < columns.Count ? columns[colIndex] : $"Column{colIndex + 1}";
                    object? colValue = GenerateParamValueDefined(records[rowIndex], null, rowIndex, colIndex);
                    rowData[colName] = colValue;
                }
            }
            if (rowData.Count > 0)
            {
                int id = await _db.SaveData(
                    _tableConfig.GetValue<string>("DbName")!,
                    _tableConfig.GetValue<string>("TableName")!,
                    rowData);

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
    private object? GenerateParamValueDefined(string[] record, ColumnDefinitionModel[]? columns, int rowIndex, int colIndex)
    {
        string value = record[colIndex].ToString().Trim('"');
        string? colType = columns == null ? "string" : columns[colIndex].DataType;
        bool isNullable = columns == null ? true : columns[colIndex].IsNullable;

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
            isValid = int.TryParse(value, out int intValue);
            if (!isValid)
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
            return intValue;
        }
        else
        {
            return value;
        }
    }

    public async Task DeleteTableData()
    {
        string tableName = _tableConfig.GetValue<string>("TableName")!;
        string dbName = _tableConfig.GetValue<string>("DbName")!;
        _logger.LogInformation($"Deleting all previous data from {tableName}");
        await _db.DeleteTableData(tableName, dbName);

    }

}
