namespace DbFileUploader;

internal class Program
{
    public static object CSVDataUploader { get; private set; }

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
                CSVDataUploader.Program.Main(args);
                break;
            case ".json":
                JsonDataUploader.Program.Main(args);
                break;
            default:
                Console.WriteLine($"Unsupported file type: {fileType}. Supported types are .csv, .json, and .xml.");
                break;
        }

    }
}
