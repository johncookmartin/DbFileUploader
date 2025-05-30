using DbFileUploader.ConsoleUI;

namespace DbFileUploader;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided. Please provide a file to upload.");
            return;
        }
        string fileName = args[0];
        string fileType = Path.GetExtension(fileName).ToLowerInvariant();

        switch (fileType)
        {
            case ".csv":
                var uploadHandler = new CsvHandler(args);
                uploadHandler.UploadFile();
                break;
            case ".json":
                throw new NotImplementedException("JSON file upload is not implemented yet.");
                break;
            default:
                Console.WriteLine($"Unsupported file type: {fileType}. Supported types are .csv, .json, and .xml.");
                break;
        }

    }
}
