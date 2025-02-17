CREATE PROCEDURE [dbo].[SingleLineComment]
	@param1 int = 0,
	@param2 int
AS
SET NOCOUNT on;
-- Single Line comments are bad as sciprting with sp_helptext may screw them up
	SELECT @param1, @param2
RETURN 0
