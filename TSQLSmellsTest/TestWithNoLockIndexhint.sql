CREATE PROCEDURE dbo.TestWithNoLockIndexhint
AS
Set nocount on 
SELECT idcol FROM dbo.TestTableSSDT WITH(NOLOCK,INDEX = 0)
GO

--SML003,SML045
