CREATE PROCEDURE [dbo].[stp_InitializeTable]
	@TableName VARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS(SELECT 1 FROM [sys].[tables] WHERE name = @TableName)
		BEGIN
			
			DECLARE @SQL NVARCHAR(MAX);

			SET @SQL = N'CREATE TABLE ' + @TableName + 
					    '(
							Id INT IDENTITY(1,1) PRIMARY KEY
						 )';

			EXEC(@SQL); 

		END


	RETURN 0;
END
