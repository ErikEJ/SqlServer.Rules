CREATE PROCEDURE dbo.MultiCteTest
    @pInsertCount INT = 0 OUTPUT
AS 
BEGIN
set nocount on ;
	WITH 
	successfulOrder AS (
		SELECT	* from dbo.TestTableSSDT
	)
	INSERT  INTO dbo.TestTableSSDT
		(
			Col1
		)
	SELECT  col1
	from successfulOrder;
	
END;

--SML005
