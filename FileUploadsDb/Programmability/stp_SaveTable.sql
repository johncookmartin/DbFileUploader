CREATE PROCEDURE [dbo].[stp_SaveTable]
	@TableName VARCHAR(255),
	@Database VARCHAR(255) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TableId INT

	IF NOT EXISTS(SELECT 1 FROM [dbo].[TableNames] WHERE TableName = @TableName AND [Database] = ISNULL(@Database, DB_NAME()))
		BEGIN
			INSERT INTO TableNames
			(TableName, [Database], CreatedDate)
			VALUES
			(@TableName, ISNULL(@Database, DB_NAME()), CURRENT_TIMESTAMP);
		END
	
	SELECT @TableId = Id
	FROM TableNames
	WHERE TableName = @TableName
			AND [Database] = ISNULL(@Database, DB_NAME())

	SELECT @TableId AS TableId

	RETURN 0;
END
