CREATE PROCEDURE [dbo].[Procedure5]
AS

declare @d4 datetime2(7);
declare @id int;

select sysdatetime() as d1;

select DATEADD(dd, 1, getdate()) as d2;

insert into dbo.Table6
	(Id, FirstName, LastName, CreatedDate, ModifiedDate)
values
	(1, 'John', 'Smith', getdate(), sysdatetime());


insert into dbo.Table6
	(Id, FirstName, LastName, CreatedDate, ModifiedDate)
values
	(2, 'Bob', 'Jones', GETDATE(), SYSDATETIME());

insert into dbo.Table6
	(Id, FirstName, LastName, CreatedDate, ModifiedDate)
select	3 as Id, 'Fred' as FirstName, 'Miller' as LastName, getdate() as CreatedDate, sysdatetime() as ModifiedDate;

update dbo.Table6
set	ModifiedDate = SysDateTime()
where	Id = 2;

set @d4 = sysdatetime();  -- Rule 64 should not find this.

select @@IDENTITY as i1;

select @id = @@IDENTITY;

if object_id('tempdb..#Person') is not null
	drop table #Person
create table #Person
(
	Id int not null,
	FirstName varchar(50) collate database_default,
	LasName varchar(50)
)


return 0


