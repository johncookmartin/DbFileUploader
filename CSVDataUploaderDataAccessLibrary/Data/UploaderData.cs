using CSVDataUploaderDataAccessLibrary.Models;
using Dapper;
using System.Text;

namespace CSVDataUploaderDataAccessLibrary.Data;
public class UploaderData : IUploaderData
{
    private readonly ISqlDataAccess _db;
    private readonly TableImportSchemaModel _tableImportSchema;

    public UploaderData(ISqlDataAccess db, TableImportSchemaModel tableImportSchema)
    {
        _db = db;
        _tableImportSchema = tableImportSchema;
    }

    public async Task SaveData(List<string[]> records, int skipHeaderLines, bool deletePrevious = true)
    {
        if (deletePrevious)
        {
            Console.WriteLine($"Deleting all records from {_tableImportSchema.TableName}");
            await _db.ExecuteDataAsync<dynamic>(GenerateDeleteQuery(), new { });
        }

        int colNum = _tableImportSchema.Columns.Count();
        int insertCount = 0;

        //skip headerlines
        for (int rowIndex = skipHeaderLines; rowIndex < records.Count; rowIndex++)
        {
            var record = records[rowIndex];

            if (record.Length != colNum)
            {
                Console.WriteLine($"Skipping line {rowIndex} due to column mismatch");
                continue;
            }
            else
            {
                var parameters = new DynamicParameters();

                for (int colIndex = 0; colIndex < colNum; colIndex++)
                {
                    string colName = _tableImportSchema.Columns[colIndex].Name;
                    string value = record[colIndex].ToString().Trim('"');
                    string colType = _tableImportSchema.Columns[colIndex].DataType;

                    if (colType.Contains("datetime", StringComparison.OrdinalIgnoreCase))
                    {
                        DateTime.TryParse(value, out DateTime dateTimeValue);
                        parameters.Add($"@{colName}", dateTimeValue);
                    }
                    else if (colType.Contains("decimal", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal.TryParse(value, out decimal decimalValue);
                        parameters.Add($"@{colName}", decimalValue);
                    }
                    else if (colType.Contains("int", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(value, out int intValue);
                        parameters.Add($"@{colName}", intValue);
                    }
                    else
                    {
                        parameters.Add($"@{colName}", value);
                    }
                }

                //Console.WriteLine($"Writing Row: {rowIndex}");
                int id = (await _db.QueryDataAsync<int, dynamic>(GenerateInsertQuery(), parameters)).First();
                if (id > 0)
                {
                    insertCount++;
                }
                else
                {
                    Console.WriteLine($"Unknown error trying to save row: {rowIndex}");
                }
                //Console.WriteLine($"Saved Row {rowIndex} with Id: {id}");
            }
        }

        Console.WriteLine($"Saved {insertCount} rows from original {records.Count()}");
    }

    private string GenerateDeleteQuery()
    {
        string sql = $@"DELETE FROM {_tableImportSchema.TableName};";

        return sql;
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
}
