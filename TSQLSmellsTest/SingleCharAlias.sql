CREATE PROCEDURE [dbo].[SingleCharAlias]

AS
set nocount on;
 Select a.Col1
  from (Select col1 from [dbo].TestTableSSDT) as a
RETURN 0

--SML011
