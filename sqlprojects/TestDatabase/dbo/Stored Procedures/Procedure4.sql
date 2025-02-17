-- GLOBAL IGNORE SRD0016

CREATE PROCEDURE [dbo].[Procedure4] 
	@param1 int = 0,	
	@param2 int			
AS
	SET NOCOUNT ON

	IF (SELECT COUNT(*) FROM dbo.Table1 t WHERE t.[Table1Id] > 0 AND t.[Table1Id2] > 0) =  0 BEGIN RETURN END;

	INSERT INTO Table1
	SELECT 'FirstName', 'LastName'

	UPDATE Table1 SET Table1Id = 11, FirstName = 'BLAH'

	UPDATE t1  
	SET t1.Table1Id = 11, t1.FirstName = 'BLAH'
	FROM Table1 t1
	INNER JOIN Table2 on 1 = 1

	select Age, 
		FirstName, 
		LastName, 
		[AgeDiff] = DATEADD(yy, -(Age), getdate())  -- year erroneously shows up as a column violation for SRD0028
	into Table99
	from Table1 
	inner join table2 on 1 = 1
	WHERE Age / 2 < 20
		AND DATEADD(d, @param1, GETDATE()) > GETDATE()

	DELETE FROM Table1

RETURN 0
