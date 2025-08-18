CREATE PROCEDURE dbo.CteAlias
AS
WITH cte AS
(
	SELECT c1, c2 FROM dbo.t1
)
SELECT c.c1, c.c2
FROM cte c
