CREATE PROCEDURE dbo.Procedure6
	@param1 INT = 0,
	@param2 INT
AS
BEGIN
	SELECT @param1, @param2;

	--https://en.wikipedia.org/wiki/Correlated_subquery
	SELECT t1.*
	FROM dbo.Table1 t1
	WHERE t1.Age > (
		SELECT AVG (t2.Age) 
		FROM dbo.Table1 t2 
		WHERE RTRIM(t1.Table1Id2) = RTRIM(t2.Table1Id2)
	);

	SELECT t1.*, 
		(
			SELECT AVG (t2.Age) 
			FROM dbo.Table1 t2 
			WHERE RTRIM(t1.Table1Id2) = RTRIM(t2.Table1Id2)
		) AS AverageAge
	FROM dbo.Table1 t1
	RETURN 0;
END
