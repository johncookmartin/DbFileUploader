using DbFileUploaderDataAccessLibrary.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        return services;
    }
}
