CREATE PROCEDURE [dbo].[SelectTopNoParen]
	
AS
	set nocount on;
	Select top 100 Col1 from dbo.TestTableSSDT;
RETURN 0

-- SML034, SRD0014
