CREATE PROCEDURE [dbo].[stp_InsertColumn]
	@TableName VARCHAR(255),
	@ColumnName VARCHAR(255),
	@DataType VARCHAR(255),
	@ColumnData VARCHAR(MAX),
	@Id INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX);
	DECLARE @OutputId INT;

	IF NOT EXISTS
	(
		SELECT 1 
		FROM [sys].[columns] c
		INNER JOIN [sys].[tables] t
		ON c.object_id = t.object_id
		WHERE t.name = @TableName
		      AND c.name = @ColumnName
	)
		BEGIN

			SET @SQL = N'ALTER TABLE ' + QUOTENAME(@TableName) +
					   ' ADD ' + QUOTENAME(@ColumnName) + ' ' + @DataType + ' NULL;';

			EXEC(@SQL);

		END

	IF @Id IS NULL
		BEGIN
		
			SET @SQL = N'INSERT INTO ' + QUOTENAME(@TableName) +
						'(' + QUOTENAME(@ColumnName) + ') VALUES (' + @ColumnData + ')' +
					   ' SET @OutputId = SCOPE_IDENTITY()';

			EXEC sp_executesql @SQL, N'@OutputId INT OUTPUT', @OutputId = @OutputId OUTPUT;
			SET @Id = @OutputId
		END
	ELSE
		BEGIN
			
			SET @SQL = N'UPDATE ' + QUOTENAME(@TableName) + 
			           ' SET ' + QUOTENAME(@ColumnName) + ' = ' + @ColumnData +
					   ' WHERE Id = ' + CAST(@Id AS NVARCHAR(10));

			EXEC(@SQL);

		END

	SELECT @Id AS Id

	RETURN 0;
END
