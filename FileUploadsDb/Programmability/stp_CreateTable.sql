CREATE PROCEDURE [dbo].[stp_CreateTable]
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

	SET @SQL = N'

		IF NOT EXISTS
		(
			SELECT 1
			FROM ' + QUOTENAME(@Database) + '.sys.tables
			WHERE name = ''' + @TableName + '''
		)
			BEGIN

				CREATE TABLE ' + QUOTENAME(@Database) + '.dbo.' + QUOTENAME(@TableName) + '
				(
					Id INT ' + CASE WHEN EXISTS(SELECT 1 FROM TableColumns WHERE TableId = @TableId AND ColumnName = 'Id') THEN '' ELSE 'IDENTITY(1,1) ' END + 'PRIMARY KEY NOT NULL
				)
			END
	';

	EXEC sp_executesql @SQL;

	RETURN 0;
END
