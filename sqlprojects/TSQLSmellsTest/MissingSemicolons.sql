CREATE PROCEDURE dbo.Test1

AS

SET NOCOUNT ON

SELECT FOO FROM BAR --violation

BEGIN TRY;
    SELECT 1 --violation
END TRY
BEGIN CATCH;  
    SELECT 2;
END CATCH --violation

CREATE TABLE t1 (
  ColumnOne int, 
  INDEX IX_ColumnOne NONCLUSTERED (ColumnOne)) --violation

CREATE INDEX i1 ON t1 (col1) --violation

IF NOT EXISTS(SELECT * FROM SYS.INDEXES	WHERE OBJECT_ID = OBJECT_ID('dbo.Foo') AND [name] = 'IX_FooIndex')
BEGIN;
    CREATE UNIQUE NONCLUSTERED INDEX IX_FooIndex ON dbo.Foo(Name)
    WITH(DATA_COMPRESSION = PAGE, ONLINE = ON) -- violation
END;

IF NOT EXISTS(SELECT * FROM SYS.INDEXES	WHERE OBJECT_ID = OBJECT_ID('dbo.Foo'))
BEGIN
    SELECT 1;
END

BEGIN
    RECEIVE TOP (1) * FROM ExpenseQueue --violation
END

GO

CREATE VIEW [foo].[bar] AS
SELECT a, b FROM foo.baz -- violation
GO

CREATE FUNCTION dbo.SelectReturnTypeFunction ()
RETURNS TABLE
AS
RETURN
(
	WITH cte AS
	(
		SELECT c1, c2 FROM t1
	)

	SELECT c1, c2 FROM cte -- should not trigger an error
) -- violation

GO

-- SRD0068
