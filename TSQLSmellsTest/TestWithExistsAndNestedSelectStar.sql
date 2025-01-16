CREATE PROCEDURE dbo.TestWithExistsAndNestedSelectStar
AS
Set nocount on ;
IF EXISTS(SELECT * FROM dbo.TestTableSSDT) BEGIN
	SELECT * FROM dbo.TestTableSSDT
end
GO

--SML005
