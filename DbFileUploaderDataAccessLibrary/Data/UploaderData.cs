using DbFileUploaderDataAccessLibrary.Models;
using DbFileUploaderDataAccessLibraryModels;
using System.Text;


namespace DbFileUploaderDataAccessLibrary.Data;
public class UploaderData : IUploaderData
{
    private readonly ISqlDataAccess _db;
    private readonly TableImportSchemaModel _tableImportSchema;

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


}
