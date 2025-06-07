using DbFileUploaderDataAccessLibrary.Data;
using DbFileUploaderDataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbFileUploader.Configuration;
public static class JsonDependencyInjection
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


        return services;
    }
}
