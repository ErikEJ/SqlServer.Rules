CREATE PROCEDURE dbo.CteJoinAfterEarlierSelect
AS
SELECT 1;
WITH set1 AS
(
    SELECT a1 = 'a'
)
SELECT tt.name
FROM sys.tables tt
INNER JOIN set1
ON set1.a1 = tt.name;
