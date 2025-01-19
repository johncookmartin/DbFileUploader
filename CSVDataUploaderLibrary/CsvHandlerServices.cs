using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CSVDataUploaderLibrary;
public static class CsvHandlerServices
{
    public static List<string[]> FormatCSV(string filePath, bool hasHeaders = false)
    {
        var records = new List<string[]>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeaders,
        };

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {

            while (csv.Read())
            {
                var record = csv.Parser.Record;
                if (record != null)
                {
                    records.Add(record);
                }
            }
        }

        return records;
    }
}
