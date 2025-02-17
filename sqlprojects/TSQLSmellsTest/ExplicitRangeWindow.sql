Create Procedure dbo.ExplicitRangeWindow
as
SET NOCOUNT ON;
/* Default is Range UNBOUNDED PRECEDING ( Defining the frame )*/
select sum(TestTableSSDT.IdCol) over(partition by TestTableSSDT.Col1 
	                      order by TestTableSSDT.Col2 
	                 Range UNBOUNDED PRECEDING) as RollingBalance
  from [dbo].[TestTableSSDT]
order by Col1,Col2;

-- SML025
