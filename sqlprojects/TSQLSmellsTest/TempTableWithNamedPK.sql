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

    CREATE TABLE #Table1 (
        ColumnOne  VARCHAR(10) NOT NULL,
        ColumnTwo  VARCHAR(10) NOT NULL,
        PRIMARY KEY (ColumnOne, ColumnOne)
    );

RETURN 0

-- SML038
