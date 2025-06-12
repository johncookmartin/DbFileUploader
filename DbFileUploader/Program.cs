using DbFileUploader.ConsoleUI;
using static DbFileUploader.Configuration.ParseArguments;

namespace DbFileUploader;

internal class Program
{
    static async Task Main(string[] args)
    {

        var arguments = ArgParse(args);

        if (arguments.Count == 0)
        {
            Console.WriteLine("No arguments provided. Please provide a file to upload.");
            return;
        }

        arguments.TryGetValue("file", out var fileToUpload);
        bool fileToUploadFound = (fileToUpload != null && !string.IsNullOrWhiteSpace(fileToUpload));

        if (!fileToUploadFound)
        {
            fileToUpload = args[0];
            arguments["file"] = fileToUpload;

        }

        fileToUploadFound = (fileToUpload != null && !string.IsNullOrWhiteSpace(fileToUpload));
        if (!fileToUploadFound || !File.Exists(fileToUpload))
        {
            Console.WriteLine($"File not found: {fileToUpload}. Please provide a valid file path.");
            return;
        }

        string fileType = Path.GetExtension(fileToUpload).ToLowerInvariant();
        bool uploaded;
        switch (fileType)
        {
            case ".csv":
                var csvUploadHandler = new CsvHandler(arguments);
                uploaded = await csvUploadHandler.UploadFile();
                break;
            case ".json":

                var jsonUploadHandler = new JsonHandler(arguments);
                uploaded = await jsonUploadHandler.UploadFile();
                break;
            default:
                Console.WriteLine($"Unsupported file type: {fileType}. Supported types are .csv, .json, and .xml.");
                break;
        }

    }
}
