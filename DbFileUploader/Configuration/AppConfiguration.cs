using Microsoft.Extensions.Configuration;

namespace DbFileUploader.Configuration;
public static class AppConfiguration
{
    public static IConfiguration BuildConfiguration(string? configFilePath, string tableName, string dbName, bool deletePrevious, bool hasConnectionString)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());
        try
        {

            builder.AddJsonFile("appsettings.json", optional: hasConnectionString);
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"appsettings.json not found. This is required for the database connection. " +
                $"If you do not want to use appsettings.json include ConnectionStrings in your config file", ex);
        }

        if (configFilePath != null && File.Exists(configFilePath))
        {
            builder.AddJsonFile(configFilePath, optional: true);
        }

        Dictionary<string, string> inMemoryConfig = new Dictionary<string, string>
        {
            { "TableName", tableName },
            { "DbName", dbName },
            { "DeletePrevious", deletePrevious.ToString() }
        };

        builder.AddInMemoryCollection(inMemoryConfig!);

        IConfiguration config = builder.Build();
        return config;

    }

}
