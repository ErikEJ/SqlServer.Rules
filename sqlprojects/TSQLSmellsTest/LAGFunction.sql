Create Procedure dbo.LagTest
as
SET NOCOUNT ON;
Select LAG(Col3)  OVER (Partition by Col1
	                          Order by Col2) as LagMonth
  from dbo.TestTableSSDT
order by Col1,Col2;