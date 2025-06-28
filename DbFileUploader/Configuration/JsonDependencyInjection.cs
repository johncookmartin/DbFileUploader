using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploaderLibrary;
using UploaderLibrary.Json;

namespace DbFileUploader.Configuration;
public static class JsonDependencyInjection
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

        services.AddSingleton<IHandlerServices<List<Dictionary<string, object>>>, JsonHandlerServices>();
        services.AddSingleton<IUploaderSaveHandler<Dictionary<string, object>>, JsonUploaderSaveHandler>();

        return services;
    }
}
