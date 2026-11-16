Create procedure dbo.ifExists
as
Set nocount on;
if(Select count(*) from dbo.TestTableSSDT)>0 begin 
   print 'Rows exist'
end

-- SRP0023