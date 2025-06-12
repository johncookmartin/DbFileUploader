using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace UploaderLibrary.Csv;
public static class CsvHandlerServices
{
    public static List<string[]> FormatCSV(string filePath, bool includeHeaders = false)
    {
        var records = new List<string[]>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = includeHeaders,
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
