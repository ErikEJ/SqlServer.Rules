CREATE PROCEDURE dbo.SelectStarFromViewInProc
AS
Set nocount on;
SELECT * FROM dbo.ViewWithOrder;
go

-- SML005
