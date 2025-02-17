/* Default is Range UNBOUNDED PRECEDING ( Defining the frame )*/
Create Procedure dbo.ImplicitRangeWindow
as
Set nocount on
select sum(TestTableSSDT.Col1) over(partition by TestTableSSDT.IdCol
	                      order by TestTableSSDT.Col2
	                 ) as RollingBalance,
       row_number() over(partition by TestTableSSDT.IdCol
	                      order by TestTableSSDT.Col2 ) as Rown
  from dbo.TestTableSSDT
order by Col1,Col2;

-- SML026
