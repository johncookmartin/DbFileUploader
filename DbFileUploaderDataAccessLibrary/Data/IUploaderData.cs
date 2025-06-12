
namespace DbFileUploaderDataAccessLibrary.Data;

public interface IUploaderData
{
    Task CreateData(int tableId);
    Task DeleteTableData(string tableName);
    Task<int> SaveData(string dbName, string tableName, Dictionary<string, object> data);
    Task<int> SaveToExisting(string dbName, string tableName, Dictionary<string, object?> data);
}