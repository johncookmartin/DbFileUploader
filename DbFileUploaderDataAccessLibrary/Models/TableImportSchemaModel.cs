using DbFileUploaderDataAccessLibraryModels;

namespace DbFileUploaderDataAccessLibrary.Models;
public class TableImportSchemaModel
{
    public string DatabaseName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<ColumnDefinitionModel> Columns { get; set; } = new List<ColumnDefinitionModel>();
    public bool CreateTable { get; set; } = false;

}
