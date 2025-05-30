using Microsoft.Extensions.Configuration;

namespace DbFileUploader.Configuration;
public static class AppConfiguration
{
    public static IConfiguration BuildConfiguration(string configFilePath)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile(configFilePath, optional: false);
        IConfiguration config = builder.Build();
        return config;
    }
}
