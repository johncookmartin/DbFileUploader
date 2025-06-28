
namespace UploaderLibrary;

public interface IHandlerServices<T>
{
    public T FormatData(string filePath, dynamic parameters);
}