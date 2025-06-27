namespace UploaderLibrary;

public interface IUploaderSaveHandler<T>
{
    Task DeleteTableData();
    Task SaveData(T records, dynamic? parameters);
}