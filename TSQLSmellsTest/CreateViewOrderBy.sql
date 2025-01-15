Create view dbo.ViewWithOrder
as
Select top(100000) [TestTableSSDT].Col1
from [dbo].[TestTableSSDT]
order by [TestTableSSDT].Col1

--SML028
