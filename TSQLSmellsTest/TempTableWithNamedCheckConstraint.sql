CREATE PROCEDURE [dbo].[TempTableWithNamedCheckConstraint]
	
AS
	Set nocount on;


	Create table #1
	(
		cola integer check(cola > 1)
	)

	Create table #2
	(
		cola integer Constraint [chk] check (cola > 1)
	);

RETURN 0

-- SML040
