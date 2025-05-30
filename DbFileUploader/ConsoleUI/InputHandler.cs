namespace DbFileUploader.ConsoleUI;
public static class InputHandler
{
    public static string GetConfigFile(string[] args)
    {
        string? filePath = null;

        if (args.Length == 2)
        {
            filePath = args[1];
        }

        while (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("Enter Config File Path: ");
            filePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Please enter a file path.");
            }
        }

        return filePath;
    }
}
