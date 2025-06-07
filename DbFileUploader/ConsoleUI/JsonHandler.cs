using DbFileUploader.Configuration;
using DbFileUploaderDataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace DbFileUploader.ConsoleUI;
public class JsonHandler : InputHandler
{
    public JsonHandler(string[] args) : base(args)
    {
        var config = AppConfiguration.BuildConfiguration(configFilePath);

        TableImportSchemaModel? tableImportSchema = config.GetSection("TableImportSchema").Get<TableImportSchemaModel>();
        if (tableImportSchema == null)
        {
            Console.WriteLine("There is not a valid TableImportSchema in the config file.");
            return;
        }
    }
}
