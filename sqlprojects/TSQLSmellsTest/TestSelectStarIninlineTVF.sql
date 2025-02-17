/* Whitelist */
CREATE FUNCTION dbo.TestSelectStarIninlineTVF()
RETURNS TABLE
AS
RETURN(
	SELECT * FROM dbo.TestTableSSDT
)