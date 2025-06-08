using DbFileUploaderDataAccessLibraryModels;

namespace DbFileUploaderDataAccessLibrary.Models;
public class TableImportSchemaModel
{
    public string DatabaseName { get; set; }
    public string TableName { get; set; }
    public List<ColumnDefinitionModel> Columns { get; set; }
    public bool CreateTable { get; set; }

}
