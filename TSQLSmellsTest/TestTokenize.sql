CREATE PROCEDURE [dbo].[TestTokenize]
	@param1 int = 0,
	@param2 int
AS
	SELECT * from [LocalServer].[SalesDB].[dbo].[SomeTable]
RETURN 0

-- SRP0005
