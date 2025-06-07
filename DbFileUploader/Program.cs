using DbFileUploader.ConsoleUI;

namespace DbFileUploader;

internal class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided. Please provide a file to upload.");
            return;
        }
        string fileName = args[0];
        string fileType = Path.GetExtension(fileName).ToLowerInvariant();

        bool uploaded;
        switch (fileType)
        {
            case ".csv":
                var csvUploadHandler = new CsvHandler(args);
                uploaded = await csvUploadHandler.UploadFile();
                break;
            case ".json":

                var jsonUploadHandler = new JsonHandler(args);
                uploaded = await jsonUploadHandler.UploadFile();
                break;
            default:
                Console.WriteLine($"Unsupported file type: {fileType}. Supported types are .csv, .json, and .xml.");
                break;
        }

    }
}
