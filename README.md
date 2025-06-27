# DbFileUploader

A .NET console application that uploads file data to a SQL Server database.

Currently supports:
- CSV files  
- SSMS extracts (via CSV)  
- JSON support is in development  

---

## Usage

At a minimum, the application must be called with a file path as an argument.  
It must either:
- Be run in a directory containing an `appsettings.json` file with a connection string, **or**  
- Be called with a config file that contains a connection string.

### Command Line Arguments

The application uses `--argument` style arguments:

| Argument   | Description |
|------------|-------------|
| `--file`   | Path to the file to upload. **Required.** |
| `--db`     | Name of the database to upload data to. |
| `--config` | Path to the config file (if not using `appsettings.json`). |
| `--delete` | Boolean value indicating whether to delete any existing data in the table before inserting new data. |
| `--table`  | Name of the table to upload data to. |

### Example

```bash
FileUploaderConsoleApp.exe --file "data.csv" --db "MyDatabase" --table "TargetTable" --delete true --config "myconfig.json"
```

---

## Configuration File (JSON)

The config file supports the following structure:

### Root Properties

| Property          | Type    | Description |
|-------------------|---------|-------------|
| `TableName`       | string  | Name of the table to upload data to. |
| `DbName`          | string  | Name of the database to upload data to. |
| `DeletePrevious`  | bool    | If true, previous data in the target table will be deleted before upload. |
| `ConnectionStrings` | object | Standard connection strings object; system will use `"Default"` key. |
| `Columns`         | array   | List of column definitions (see **Columns** section below). |
| `CsvDetails`      | object  | CSV-specific settings (see **CsvDetails** section below). |

### CsvDetails

| Property         | Type    | Description |
|------------------|---------|-------------|
| `SkipHeaderLines`| int     | Number of lines to skip at the start of the CSV file. |
| `HasHeaders`     | bool    | Whether the file has headers. |
| `HasIdentity`    | bool    | Whether the table in sql has identity or not. |

### Columns

If a list of columns is provided, the application will attempt to save data to the defined table and columns.

| Property   | Type    | Description |
|------------|---------|-------------|
| `ColumnIndex` | int   | Position of the column in the input data. |
| `Name`        | string | Column name in the database. |
| `DataType`    | string | SQL data type. |
| `IsNullable`  | bool   | Whether the column accepts null values. |

### Example Config File

```json
{
  "TableName": "MyTargetTable",
  "DbName": "MyDatabase",
  "DeletePrevious": true,
  "ConnectionStrings": {
    "Default": "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;"
  },
  "CsvDetails": {
    "SkipHeaderLines": 1,
    "HasHeaders": true,
    "HasIdentity": true
  },
  "Columns": [
    {
      "ColumnIndex": 0,
      "Name": "FirstName",
      "DataType": "NVARCHAR(100)",
      "IsNullable": false
    },
    {
      "ColumnIndex": 1,
      "Name": "LastName",
      "DataType": "NVARCHAR(100)",
      "IsNullable": true
    }
  ]
}
```

---

## Database Setup

If no `Columns` section is provided in the config file, the application requires the `FileUploadsDB` database (included in this repository).  

- The application will upload data in an **Entity-Attribute-Value (EAV)** model.
- It can optionally create a new table and upload the data into it automatically.

---

## Roadmap

- [x] Support for CSV files  
- [x] Support for SSMS extracts  
- [ ] Support for JSON files (coming soon)  

---

## Notes

- The application requires a valid SQL Server connection string with permissions to insert data.
- CSV files must be UTF-8 encoded.

