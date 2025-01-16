/* Default is Range UNBOUNDED PRECEDING ( Defining the frame )*/

Create Procedure dbo.RangeWindow
as
set nocount on 
select sum(IdCol) over(partition by Col1
	                      order by Col2 
	                 Range UNBOUNDED PRECEDING) as RollingBalance
  from dbo.TestTableSSDT
order by Col1,Col2;

--SML025
