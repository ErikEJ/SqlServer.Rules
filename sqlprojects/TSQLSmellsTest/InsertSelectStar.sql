
CREATE PROCEDURE dbo.InsertSelectStar
AS
 set nocount on;
    INSERT INTO dbo.TestTableSSDT([IdCol],Col1,Col2,Col3,DateCol)
	SELECT * from dbo.TestTableSSDT;
