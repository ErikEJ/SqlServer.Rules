CREATE PROCEDURE [dbo].[TempTableWithNamedPK]
	
AS
	SET nocount on;

	Create table #1
	(
		ID integer primary key
	)

	Create table #2
	(
		ID integer,
		Constraint [PKID] primary key (ID)
	);

RETURN 0

--SML038
