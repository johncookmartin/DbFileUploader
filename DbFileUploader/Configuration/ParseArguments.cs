namespace DbFileUploader.Configuration;
public static class ParseArguments
{
    public static Dictionary<string, string> ArgParse(string[] args)
    {
        var result = new Dictionary<string, string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-r")
            {
                result["recursive"] = "true"; // Short option for recursive search
            }
            else if (args[i].StartsWith("--"))
            {
                string key = args[i].Substring(2).ToLowerInvariant();
                string value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : string.Empty;
                result[key] = value;
            }
            else if (i == 0)
            {
                result["file"] = args[i]; // Assume the first argument is the file path
            }
        }

        return result;
    }
}
