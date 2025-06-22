using System.Text;
using System.Text.Json;


namespace DbFileUploaderDataAccessLibrary.Data;
public class UploaderData : IUploaderData
{
    private readonly ISqlDataAccess _db;
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
        [typeof(DateTime)] = "DATETIME2(0)",
        [typeof(DateTime?)] = "DATETIME2(0)",
        [typeof(byte[])] = "VARBINARY(MAX)"
    };

    public UploaderData(ISqlDataAccess db)
    {
        _db = db;
    }
    public async Task DeleteTableData(string tableName, string dbName)
    {
        await _db.ExecuteDataAsync<dynamic>("stp_DeleteTableData", new { TableName = tableName, Database = dbName });
    }

    public async Task<int> SaveData(string dbName, string tableName, Dictionary<string, object> data)
    {
        int tableId = (await _db.QueryDataAsync<int, dynamic>(
            "stp_SaveTable",
            new
            {
                TableName = tableName,
                Database = dbName
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

        return tableId;
    }

    public async Task<int> SaveToExisting(string dbName, string tableName, Dictionary<string, object?> data)
    {
        var columnsBuilder = new StringBuilder();
        var paramsBuilder = new StringBuilder();

        int i = 0;
        foreach (var entry in data)
        {
            if (i > 0)
            {
                columnsBuilder.Append(", ");
                paramsBuilder.Append(", ");
            }

            columnsBuilder.Append(entry.Key);

            string appendValue = (entry.Value == null) ? "NULL" : entry.Value.ToString()!;
            paramsBuilder.Append(appendValue);

            i++;
        }

        string json = JsonSerializer.Serialize(data);

        string sql = $@"INSERT INTO {dbName}.dbo.{tableName} ({columnsBuilder}) VALUES ({paramsBuilder}); SELECT SCOPE_IDENTITY();";
        int id = (await _db.QueryDataAsync<int, dynamic>(sql, JsonSerializer.Deserialize<dynamic>(json))).FirstOrDefault(0);

        return id;
    }

    public async Task CreateData(int tableId)
    {
        await _db.ExecuteDataAsync<dynamic>("stp_CreateTable", new { TableId = tableId });
        await _db.ExecuteDataAsync<dynamic>("stp_CreateColumns", new { TableId = tableId });
        await _db.ExecuteDataAsync<dynamic>("stp_CreateValues", new { TableId = tableId });
    }


}
