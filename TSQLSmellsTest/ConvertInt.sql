
CREATE PROCEDURE dbo.ConvertInt
AS
Set nocount on;
SELECT Col1,CONVERT(varchar(255),DateCol,120),'$(Test1)' as newcol
FROM [dbo].[TestTableSSDT]
WHERE CAST(Col1 AS VARCHAR(10)) ='22'

-- SML006
