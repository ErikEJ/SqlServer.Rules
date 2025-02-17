CREATE FUNCTION [dbo].[fn_WithSchemaBinding]
(
	@param1 varchar(max)
)
RETURNS varchar(max) WITH SCHEMABINDING
AS
BEGIN
	RAISERROR('foo', 19, 1)
	RETURN @param1
END
