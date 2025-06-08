CREATE PROCEDURE [dbo].[stp_CreateColumns]
	@TableId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Database VARCHAR(255),
	        @TableName VARCHAR(255);

	SELECT @Database = ISNULL([Database], DB_NAME()),
	       @TableName = TableName
	FROM TableNames
	WHERE Id = @TableId;

	DECLARE @SQL NVARCHAR(MAX);

	DECLARE @ColumnName VARCHAR(255),
	        @ColumnType VARCHAR(255)

	DECLARE column_cursor CURSOR FOR
	SELECT ColumnName, ColumnType
	FROM TableColumns
	WHERE TableId = @TableId;

	OPEN column_cursor

	FETCH NEXT FROM column_cursor INTO @ColumnName, @ColumnType;

	WHILE @@FETCH_STATUS = 0
		BEGIN

			SET @SQL = N'
				
				IF NOT EXISTS
				(
					SELECT 1
					FROM ' + QUOTENAME(@Database) + '.sys.columns c
					INNER JOIN ' + QUOTENAME(@Database) + '.sys.tables t
					ON c.object_id = t.object_id
					WHERE t.name = ''' + @TableName + '''
					      AND c.name = ''' + @ColumnName + '''
				)
					BEGIN

						ALTER TABLE ' + QUOTENAME(@Database) + '.dbo.' + QUOTENAME(@TableName) + '
						ADD ' + QUOTENAME(@ColumnName) + ' ' + @ColumnType + ' NULL;

					END
			'

			EXEC sp_executesql @SQL;

			FETCH NEXT FROM column_cursor INTO @ColumnName, @ColumnType;
		END

	CLOSE column_cursor;
	DEALLOCATE column_cursor;

	RETURN 0;
END
