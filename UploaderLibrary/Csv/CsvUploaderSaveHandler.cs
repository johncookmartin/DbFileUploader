﻿using DbFileUploaderDataAccessLibrary.Data;
using DbFileUploaderDataAccessLibraryModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploaderLibrary.Csv;
public class CsvUploaderSaveHandler : IUploaderSaveHandler<List<string[]>>
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

    public async Task CreateData(int tableId)
    {
        string tableName = _tableConfig.GetValue<string>("TableName")!;
        string dbName = _tableConfig.GetValue<string>("DbName")!;
        _logger.LogInformation($"Creating table {tableName} in database {dbName}");
        await _db.CreateData(tableId);
    }

    public async Task<int> SaveData(List<string[]> records, dynamic parameters)
    {
        int startingIndex = getStartingIndex(parameters);
        bool hasHeaders = getHasHeaders(parameters);

        bool hasDefinition = _tableConfig.GetSection("Columns").Exists();
        int tableId = 0;
        if (hasDefinition)
        {
            await SaveDataDefined(records, startingIndex);
        }
        else
        {
            tableId = await SaveDataDynamically(records, startingIndex, hasHeaders);
        }

        return tableId;

    }

    private int getStartingIndex(dynamic parameters)
    {
        int startingIndex = 0;

        dynamic type = parameters.GetType();
        dynamic prop = type.GetProperty("StartingIndex");
        if (prop != null)
        {
            dynamic value = prop.GetValue(parameters, null);
            startingIndex = Convert.ToInt32(value);
        }
        return startingIndex;
    }

    private bool getHasHeaders(dynamic parameters)
    {
        bool hasHeaders = false;
        dynamic type = parameters.GetType();
        dynamic prop = type.GetProperty("HasHeaders");
        if (prop != null)
        {
            dynamic value = prop.GetValue(parameters, null);
            hasHeaders = Convert.ToBoolean(value);
        }
        return hasHeaders;
    }

    private async Task SaveDataDefined(List<string[]> records, int startingIndex)
    {
        ColumnDefinitionModel[] columns = _tableConfig.GetSection("Columns").Get<ColumnDefinitionModel[]>()!;
        bool hasIdentity = _tableConfig.GetValue<bool>("HasIdentity");
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

    private async Task<int> SaveDataDynamically(List<string[]> records, int startingIndex, bool hasHeaders)
    {
        int insertCount = 0;
        List<string> columns = new List<string>();
        int tableId = 0;

        for (int rowIndex = 0; rowIndex < records.Count; rowIndex++)
        {
            Dictionary<string, object?> rowData = new Dictionary<string, object?>();

            for (int colIndex = startingIndex; colIndex < records[rowIndex].Length; colIndex++)
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
                tableId = await _db.SaveData(
                    _tableConfig.GetValue<string>("DbName")!,
                    _tableConfig.GetValue<string>("TableName")!,
                    rowData);

                if (tableId > 0)
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
        return tableId;
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
