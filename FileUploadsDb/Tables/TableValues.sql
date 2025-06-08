CREATE TABLE [dbo].[TableValues]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[TableId] INT NOT NULL,
	[RowId] INT NOT NULL,
	[ColumnId] INT NOT NULL,
	[Value] NVARCHAR(MAX),
	CONSTRAINT FK_TableValues_TableName FOREIGN KEY ([TableId]) REFERENCES TableNames ([Id]),
	CONSTRAINT FK_TableValues_TableRows FOREIGN KEY ([RowId]) REFERENCES TableRows ([Id]),
	CONSTRAINT FK_TableValues_TableColumns FOREIGN KEY ([ColumnId]) REFERENCES TableColumns ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
)
