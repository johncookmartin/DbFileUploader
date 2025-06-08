CREATE PROCEDURE [dbo].[stp_SaveColumn]
	@TableId INT,
	@ColumnName VARCHAR(255),
	@ColumnType VARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS
	(
		SELECT 1
		FROM TableColumns
		WHERE ColumnName = @ColumnName
		      AND TableId = @TableId
	)
		BEGIN
			INSERT INTO TableColumns
			(TableId, ColumnName, ColumnType)
			VALUES
			(@TableId, @ColumnName, @ColumnType)
		END

	SELECT Id AS ColumnId
	FROM TableColumns
	WHERE ColumnName = @ColumnName
	      AND TableId = @TableId;

	RETURN 0;
END
