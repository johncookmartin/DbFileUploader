using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploaderLibrary.Csv;

namespace DbFileUploader.Configuration;
public static class CsvDependencyInjection
{
    public static IServiceCollection ConfigureServices(IConfiguration config)
    {
        var services = new ServiceCollection();

        services.AddLogging(loggingConfig =>
        {
            loggingConfig.AddConsole();
        });

        services.AddSingleton<IConfiguration>(config);

        services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        services.AddSingleton<IUploaderData, UploaderData>();

        services.AddSingleton<CsvUploaderSaveHandler, CsvUploaderSaveHandler>();

        return services;
    }

}
