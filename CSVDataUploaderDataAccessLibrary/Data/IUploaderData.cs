namespace CSVDataUploaderDataAccessLibrary.Data;

public interface IUploaderData
{
    Task DeleteData(string? tableName = null);
    Task SaveData(List<string[]> records, int startingIndex);
}