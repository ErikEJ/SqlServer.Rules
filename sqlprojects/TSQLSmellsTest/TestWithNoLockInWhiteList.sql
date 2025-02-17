CREATE PROCEDURE dbo.TestWithNoLockInWhiteList
AS
set nocount on;
SELECT idcol FROM dbo.TestTableSSDT WITH(NOLOCK)

go
EXEC sys.sp_addextendedproperty @name=N'WhiteList', @value=N'Ignore' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'PROCEDURE',@level1name=N'TestWithNoLockInWhiteList'