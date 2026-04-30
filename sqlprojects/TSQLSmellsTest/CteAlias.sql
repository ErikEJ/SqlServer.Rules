CREATE PROCEDURE dbo.CteAlias
AS
WITH cte AS
(
	SELECT c1, c2 FROM dbo.t1
)
SELECT c3.c1, c3.c2
FROM cte c3
