CREATE PROCEDURE [dbo].[TempTableWithNamedDefConstraint]
	
AS
	Set nocount on;


	Create table #1
	(
		cola integer default (1)
	)

	Create table #2
	(
		cola integer Constraint [ColaDef] default (1)
	);

RETURN 0

-- SML039
