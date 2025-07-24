namespace UploaderLibrary;

public interface IUploaderSaveHandler<T>
{
    Task DeleteTableData();
    Task<int> SaveData(T records, dynamic parameters);
    Task CreateData(int tableId);
}