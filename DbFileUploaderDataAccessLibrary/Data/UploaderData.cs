using DbFileUploaderDataAccessLibrary.Models;
using DbFileUploaderDataAccessLibraryModels;
using System.Text;


namespace DbFileUploaderDataAccessLibrary.Data;
public class UploaderData : IUploaderData
{
    private readonly ISqlDataAccess _db;
    private readonly TableImportSchemaModel _tableImportSchema;
    private static readonly Dictionary<Type, string> _typeMap = new()
    {
        [typeof(string)] = "NVARCHAR(MAX)",
        [typeof(int)] = "INT",
        [typeof(int?)] = "INT",
        [typeof(long)] = "BIGINT",
        [typeof(long?)] = "BIGINT",
        [typeof(bool)] = "BIT",
        [typeof(bool?)] = "BIT",
        [typeof(decimal)] = "DECIMAL(18,2)",
        [typeof(decimal?)] = "DECIMAL(18,2)",
        [typeof(double)] = "FLOAT",
        [typeof(double?)] = "FLOAT",
        [typeof(float)] = "REAL",
        [typeof(float?)] = "REAL",
        [typeof(DateTime)] = "DATETIME",
        [typeof(DateTime?)] = "DATETIME",
        [typeof(byte[])] = "VARBINARY(MAX)"
    };

    public UploaderData(ISqlDataAccess db, TableImportSchemaModel tableImportSchema)
    {
        _db = db;
        _tableImportSchema = tableImportSchema;
    }
    public async Task DeleteTableData(string tableName)
    {
        await _db.ExecuteDataAsync<dynamic>($@"DELETE FROM {tableName};", new { });
    }

    public async Task<int> SaveRow(dynamic parameters)
    {
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

        string sql = $@"INSERT INTO {_tableImportSchema.TableName} ({columnsBuilder}) VALUES ({paramsBuilder}); SELECT SCOPE_IDENTITY();";
        IEnumerable<int> idList = await _db.QueryDataAsync<int, dynamic>(sql, parameters);
        int id = idList.FirstOrDefault(0);

        return id;
    }

    public async Task SaveData(TableImportSchemaModel schemaModel, Dictionary<string, object> data, bool createTable = false)
    {
        int tableId = (await _db.QueryDataAsync<int, dynamic>(
            "stp_SaveTable",
            new
            {
                TableName = schemaModel.TableName,
                Database = schemaModel.DatabaseName
            })).First();

        int? rowId = null;

        foreach (var entry in data)
        {

            _typeMap.TryGetValue(entry.Value.GetType(), out string? sqlType);

            int columnId = (await _db.QueryDataAsync<int, dynamic>(
                "stp_SaveColumn",
                new
                {
                    TableId = tableId,
                    ColumnName = entry.Key,
                    ColumnType = sqlType ?? "NVARCHAR(MAX)"
                })).First();

            rowId = (await _db.QueryDataAsync<int, dynamic>(
                "stp_SaveValue",
                new
                {
                    TableId = tableId,
                    ColumnId = columnId,
                    Value = entry.Value.ToString(),
                    RowId = rowId
                })).First();
        }

        if (createTable)
        {
            await CreateTable(tableId);
            await CreateColumns(tableId);
            await CreateValues(tableId);
        }
    }

    private async Task CreateColumns(int tableId)
    {
        await _db.ExecuteDataAsync<dynamic>("stp_CreateColumns", new { TableId = tableId });
    }

    private async Task CreateTable(int tableId)
    {
        await _db.ExecuteDataAsync<dynamic>("stp_CreateTable", new { TableId = tableId });
    }

    private async Task CreateValues(int tableId)
    {
        await _db.ExecuteDataAsync<dynamic>("stp_CreateValues", new { TableId = tableId });
    }


}
