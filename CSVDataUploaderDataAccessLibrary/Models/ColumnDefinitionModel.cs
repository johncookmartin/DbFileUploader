namespace CSVDataUploaderDataAccessLibrary.Models;
public class ColumnDefinitionModel
{
    public int ColumnIndex { get; set; }
    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
}
