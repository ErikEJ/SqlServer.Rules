CREATE FUNCTION [dbo].[NoSchemaBinding]
(
	@param1 varchar(max)
)
RETURNS varchar(max)
AS
BEGIN
	RETURN @param1
END
