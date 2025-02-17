CREATE PROCEDURE [dbo].[Injection]
	@param1 varchar(255)
AS
SET NOCOUNT ON;
	exec('Select * from table where name like '''+@param1+'''');
RETURN 0
go
CREATE PROCEDURE [dbo].[Injection2]
	@param1 varchar(255)
AS
SET NOCOUNT ON;
	declare @SQL Nvarchar(1024)
	Select @Sql = 'Select * from table where name like '''+upper(@param1)+''''
    exec(@sql);
RETURN 0
go
CREATE PROCEDURE [dbo].[Injection3]
	@param1 varchar(255)
AS
SET NOCOUNT ON;
	declare @SQL Nvarchar(1024)
	Select @Sql = 'Select * from table where name like '''+upper(@param1)+''''
    exec sp_executesql @Stmt = @sql;
RETURN 0
go
CREATE PROCEDURE [dbo].[Clean]
	@param1 varchar(255)
AS
SET NOCOUNT ON;
	declare @SQL Nvarchar(1024)
	Select @Sql = 'Select * from table where name like ''@param1'''
    exec sp_executesql @Stmt = @sql,
	                   @Params = N'@Param1 varchar(255)',
					   @param1 = @param1;
RETURN 0
go




CREATE PROCEDURE [dbo].[Injection3a]

       @param1 varchar(255)

AS
SET NOCOUNT ON;

       declare @SQL Nvarchar(1024)
       declare @tbl table(id int)
       Select @Sql = 'Select * from table where name like '''+upper(@param1)+''''
       insert @tbl(id)
    exec sp_executesql @Stmt = @sql;

RETURN 0

Go

CREATE FUNCTION dbo.fn_justcopy(@param varchar(255))

RETURNS varchar(255)

AS

BEGIN

       RETURN @param

END

go

CREATE PROCEDURE [dbo].[Injection2a]

       @param1 varchar(255)

AS

SET NOCOUNT ON;

       declare @SQL Nvarchar(1024)

       declare @param2 varchar(255)

       select @param2=dbo.fn_justcopy(@param1)

       Select @Sql = 'Select * from table where name like '''+CAST(@param1 as varchar(200))+''''

    exec(@sql);

RETURN 0

-- SML043
