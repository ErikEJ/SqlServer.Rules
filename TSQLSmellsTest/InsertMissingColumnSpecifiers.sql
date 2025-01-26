
CREATE PROCEDURE dbo.InsertMissingColumnSpecifiers
AS
SET NOCOUNT ON; 
    INSERT INTO dbo.TestTableSSDT
	SELECT [IdCol],Col1,Col2,Col3,DateCol FROM dbo.TestTableSSDT;

	INSERT INTO dbo.TestTableSSDT([IdCol],Col1,Col2,Col3,DateCol)
	SELECT [IdCol],Col1,Col2,Col3,DateCol FROM dbo.TestTableSSDT;

-- SML012, SRD0009, SRD0013
