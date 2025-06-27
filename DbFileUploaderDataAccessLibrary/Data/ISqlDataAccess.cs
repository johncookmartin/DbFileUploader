using System.Data;

namespace DbFileUploaderDataAccessLibrary.Data;

public interface ISqlDataAccess
{
    Task ExecuteDataAsync<T>(string storedProcedure, T parameters, CommandType commandType = CommandType.StoredProcedure, string connectionStringName = "Default");
    Task<IEnumerable<T>> QueryDataAsync<T, U>(string storedProcedure, U parameters, CommandType commandType = CommandType.StoredProcedure, string connectionStringName = "Default");

}