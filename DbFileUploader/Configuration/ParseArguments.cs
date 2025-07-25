namespace DbFileUploader.Configuration;
public static class ParseArguments
{
    public static Dictionary<string, string> ArgParse(string[] args)
    {
        var result = new Dictionary<string, string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-f" || args[i] == "--file")
            {
                result["file"] = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : string.Empty;
            }
            else if (args[i] == "-r" || args[i] == "--recursive")
            {
                result["recursive"] = "true";
            }
            else if (args[i] == "-d" || args[i] == "--delete")
            {
                result["delete"] = "true";
            }
            else if (args[i] == "--fields")
            {
                for (i++; i < args.Length && !args[i].StartsWith("--"); i++)
                {
                    if (!result.ContainsKey("fields"))
                    {
                        result["fields"] = string.Empty;
                    }
                    result["fields"] += args[i] + ",";
                }
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
