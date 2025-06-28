using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace UploaderLibrary.Csv;
public class CsvHandlerService : IHandlerServices<List<string[]>>
{

    public List<string[]> FormatData(string filePath, dynamic parameters)
    {
        bool includeHeaders = getIncludeHeaders(parameters);

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

    private bool getIncludeHeaders(dynamic parameters)
    {
        bool includeHeaders = false;

        dynamic type = parameters.GetType();
        dynamic prop = type.GetProperty("IncludeHeaders");
        if (prop != null)
        {
            dynamic value = prop.GetValue(parameters, null);
            includeHeaders = Convert.ToBoolean(value);
        }
        return includeHeaders;
    }

}
