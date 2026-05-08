Create Procedure dbo.ExplicitRangeWindowSrp0028Test
as
SET NOCOUNT ON;

select sum(t.IdCol) over(partition by t.Col1
                          order by t.Col2
                          RANGE UNBOUNDED PRECEDING) as RollingBalance
from dbo.TestTableSSDT t;
