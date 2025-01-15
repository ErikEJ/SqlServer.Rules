
CREATE PROCEDURE dbo.TestWithExists
AS
Set nocount on 
IF EXISTS(SELECT * FROM dbo.TestTableSSDT) BEGIN
	SELECT Idcol from dbo.TestTableSSDT
end

--SML005
