CREATE PROCEDURE dbo.TestWithNoLockNoWith
as
Set nocount on
SELECT idcol FROM dbo.TestTableSSDT (NOLOCK)

--SML003
