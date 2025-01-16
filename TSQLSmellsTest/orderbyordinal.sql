
Create Procedure dbo.SelectFromOrdinal
as
SET nocount on;
SELECT Col1,col2,col3
 FROM dbo.TestTableSSDT ORDER BY 1,2,3

 --SML007
