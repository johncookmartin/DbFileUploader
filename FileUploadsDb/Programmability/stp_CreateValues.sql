CREATE PROCEDURE [dbo].[stp_CreateValues]
	@TableId INT,
	@FileUploadsDb NVARCHAR(255) = 'FileUploads'
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Database VARCHAR(255),
	        @TableName VARCHAR(255)

	SELECT @Database = ISNULL([Database], DB_NAME()),
	       @TableName = TableName
	FROM TableNames
	WHERE Id = @TableId;

	DECLARE @SQL NVARCHAR(MAX),
			@SqlColumns NVARCHAR(MAX);

	SELECT @SqlColumns = STRING_AGG(QUOTENAME(ColumnName), ',')
	FROM TableColumns c
	INNER JOIN TableNames n
	ON c.TableId = n.Id
	WHERE n.Id = @TableId

	SET @SQL = N'
	
		INSERT INTO ' + QUOTENAME(@Database) + '.dbo.' + QUOTENAME(@TableName) + '
		('+@SqlColumns+')
		SELECT *
		FROM
		(
			SELECT c.ColumnName,
				   v.Value
			FROM ' + QUOTENAME(@FileUploadsDb) + '.dbo.TableRows r
			INNER JOIN ' + QUOTENAME(@FileUploadsDb) + '.dbo.TableValues v ON v.RowId = r.Id
			INNER JOIN ' + QUOTENAME(@FileUploadsDb) + '.dbo.TableColumns c ON v.ColumnId = c.Id
			INNER JOIN ' + QUOTENAME(@FileUploadsDb) + '.dbo.TableNames n ON c.TableId = n.Id
			WHERE n.Id = ' + CAST(@TableId AS NVARCHAR(10)) + '
		) AS SourceTable
		PIVOT
		(
			MAX(Value)
			FOR ColumnName IN (' + @SqlColumns + ')
		) AS PivotTable;
	
	'
	
	EXEC sp_executesql @SQL;

	RETURN 0;
END
