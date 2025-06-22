
using System.Data;

namespace DbFileUploaderDataAccessLibrary.Data;

public interface ISqlDataAccess
{
    Task ExecuteDataAsync<T>(string storedProcedure, T parameters, string connectionStringName = "Default", CommandType commandType = CommandType.StoredProcedure);
    Task<IEnumerable<T>> QueryDataAsync<T, U>(string storedProcedure, U parameters, string connectionStringName = "Default", CommandType commandType = CommandType.StoredProcedure);
}