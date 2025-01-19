
namespace CSVDataUploaderDataAccessLibrary.Data;

public interface ISqlDataAccess
{
    Task ExecuteDataAsync<T>(string storedProcedure, T parameters, string connectionStringName = "Default");
    Task<IEnumerable<T>> QueryDataAsync<T, U>(string storedProcedure, U parameters, string connectionStringName = "Default");
}