
CREATE PROCEDURE dbo.ConvertInt2
AS
SET NOCOUNT ON
SELECT CONVERT(varchar(255),DateCol,120)
FROM dbo.TestTableSSDT
WHERE '22'  =CAST(Col1 AS VARCHAR(10))

-- SML006
