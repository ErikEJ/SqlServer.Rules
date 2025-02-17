CREATE PROCEDURE [dbo].[TempTableWithNamedFK]
	
AS
	Set nocount on;


	Create table #Parent
	(
		ID integer primary key
	)

	Create table #2
	(
		PID integer--,
		--constraint foreign key references #Parent(id)
		--  Is it even possible to add and unnamed fK constraint ?!
	)

	Create table #1
	(
		PID integer constraint [fk] foreign key references #Parent(id)
	)

RETURN 0
