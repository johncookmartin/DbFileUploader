CREATE PROCEDURE [dbo].[stp_SaveColumn]
	@TableId INT,
	@ColumnName VARCHAR(255),
	@ColumnType VARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ColumnId INT

	IF NOT EXISTS
	(
		SELECT 1
		FROM TableColumns
		WHERE ColumnName = @ColumnName
		      AND TableId = @TableId
	)
		BEGIN
			INSERT INTO TableColumns
			(ColumnName, ColumnType)
			VALUES
			(@ColumnName, @ColumnType)
		END

	SELECT @ColumnId = Id
	FROM TableColumns
	WHERE ColumnName = @ColumnName
	      AND TableId = @TableId

	RETURN 0;
END
