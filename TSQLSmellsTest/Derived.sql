Create procedure dbo.derived
as

Set nocount on;
Select 1
 from  dbo.TestTableSSDT
 left join (Select top(100) percent Col1
             from  dbo.TestTableSSDT
			order by Col2)
			Derived
 on Derived.Col1 = TestTableSSDT.Col1;




 Select 1
 from  dbo.TestTableSSDT
 left join (Select top(9999) Col1
             from  dbo.TestTableSSDT
			order by Col1)
			Derived
 on Derived.Col1 = TestTableSSDT.Col1;

 -- SML035
