CREATE PROCEDURE dbo.TestWithExistsAndNestedSelectStarInlineIF
AS
Set nocount on 
IF EXISTS(SELECT * FROM dbo.TestTableSSDT) SELECT * FROM dbo.TestTableSSDT
GO

--SML005
