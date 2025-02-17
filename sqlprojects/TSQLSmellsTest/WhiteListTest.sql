
Create Table [WhiteListTest]
(
	col1 integer


)
GO
EXEC sys.sp_addextendedproperty @name=N'WhiteList', @value=N'Ignore' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'WhiteListTest'
GO


Create Procedure [WhiteListTestProc]
as
Select * from [WhiteListTest]
go
EXEC sys.sp_addextendedproperty @name=N'WhiteList', @value=N'Ignore' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'PROCEDURE',@level1name=N'WhiteListTestProc'
GO
