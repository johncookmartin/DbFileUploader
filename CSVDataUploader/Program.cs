using CSVDataUploader.ConsoleUI;
using CSVDataUploaderDataAccessLibrary.Data;
using CSVDataUploaderDataAccessLibrary.Models;
using CSVDataUploaderLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSVDataUploader;

internal class Program
{
    static async Task Main(string[] args)
    {

        string configFilePath = InputHandler.GetConfigFile(args);

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile(configFilePath, optional: false);

        IConfiguration config = builder.Build();
        TableImportSchemaModel? tableImportSchema = config.GetSection("TableImportSchema").Get<TableImportSchemaModel>();
        if (tableImportSchema == null)
        {
            Console.WriteLine("There is not a valid TableImportSchema in the config file.");
            return;
        }

        var services = new ServiceCollection();

        services.AddLogging(loggingConfig =>
        {
            loggingConfig.AddConsole();
        });

        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton(tableImportSchema);

        services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        services.AddSingleton<IUploaderData, UploaderData>();

        services.AddSingleton<UploaderSaveHandler, UploaderSaveHandler>();

        var serviceProvider = services.BuildServiceProvider();

        int skipHeaderLines = InputHandler.GetSkipHeaderLines(config);
        List<string[]> records = InputHandler.GetCSVData(args, config);
        if (records.Count == 0)
        {
            return;
        }
        bool deletePrevious = InputHandler.GetDeletePrevious(config, tableImportSchema.TableName);

        var db = serviceProvider.GetRequiredService<ISqlDataAccess>();
        var uploaderData = serviceProvider.GetRequiredService<IUploaderData>();
        var uploader = serviceProvider.GetRequiredService<UploaderSaveHandler>();

        if (deletePrevious)
        {
            await uploader.DeleteTableData();
        }
        await uploader.SaveData(records, skipHeaderLines);

    }
}
