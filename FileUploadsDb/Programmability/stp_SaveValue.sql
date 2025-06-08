CREATE PROCEDURE [dbo].[stp_SaveValue]
	@TableId INT,
	@ColumnId INT,
	@Value NVARCHAR(MAX),
	@RowId INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @RowId IS NULL
		BEGIN
			
			INSERT INTO TableRows
			(EntryDate)
			VALUES
			(CURRENT_TIMESTAMP);

			SET @RowId = SCOPE_IDENTITY();

		END

	INSERT INTO TableValues
	(
		[TableId],
		[RowId],
		[ColumnId],
		[Value]
	)
	VALUES
	(
		@TableId,
		@RowId,
		@ColumnId,
		@Value
	)

	SELECT @RowId AS RowId

	RETURN 0;
END
