CREATE PROCEDURE [dbo].[ForceScan]
	
AS
set nocount on
	Select col1
	from dbo.TestTableSSDT with(forcescan);

RETURN 0

--SML044
