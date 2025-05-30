using CSVDataUploaderDataAccessLibrary.Data;
using CSVDataUploaderDataAccessLibrary.Models;
using CSVDataUploaderLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbFileUploader.Configuration;
public static class CsvDependencyInjection
{
    public static IServiceCollection ConfigureServices(IConfiguration config, TableImportSchemaModel schema)
    {
        var services = new ServiceCollection();

        services.AddLogging(loggingConfig =>
        {
            loggingConfig.AddConsole();
        });

        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton(schema);

        services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        services.AddSingleton<IUploaderData, UploaderData>();

        services.AddSingleton<UploaderSaveHandler, UploaderSaveHandler>();

        return services;
    }

}
