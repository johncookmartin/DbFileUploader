namespace CSVDataUploaderDataAccessLibrary.Data;

public interface IUploaderData
{
    Task SaveData(List<string[]> records, int skipHeaderLines, bool deletePrevious = true);
}