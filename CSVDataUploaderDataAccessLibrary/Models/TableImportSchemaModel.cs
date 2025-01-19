namespace CSVDataUploaderDataAccessLibrary.Models;
public class TableImportSchemaModel
{
    public string TableName { get; set; }
    public List<ColumnDefinitionModel> Columns { get; set; }

}
