CREATE PROCEDURE [dbo].[stp_DeleteTableData]
	@TableName NVARCHAR(255),
	@Database NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX);

	IF @Database = 'FileUploads' AND EXISTS(SELECT 1 FROM TableNames WHERE TableName = @TableName)
		BEGIN

			DECLARE @TableId INT;
			SELECT @TableId = Id FROM TableNames WHERE TableName = @TableName;

			DELETE FROM TableValues
			WHERE TableId = @TableId;

			DELETE FROM TableRows
			WHERE TableId = @TableId;
		END

	SET @SQL = N'
		IF EXISTS
		(
			SELECT 1
			FROM ' + QUOTENAME(@Database) + '.sys.tables
			WHERE name = @TableName
		)
		BEGIN
			DELETE FROM ' + QUOTENAME(@Database) + '.dbo.' + QUOTENAME(@TableName) + ';
		END';

	EXEC sp_executesql @SQL, N'@TableName NVARCHAR(255)', @TableName;


	RETURN 0;
END
