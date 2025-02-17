CREATE PROCEDURE [dbo].[Procedure3]
	@param1 int = 0,
	@param2 int,
	@ilovelamp VARCHAR = NULL
AS
	DECLARE @notused int

	DECLARE @foo table (
		id int
	)
	insert into @foo select 1

	SELECT id FROM @foo 	

	SELECT TOP 10 Table1.FirstName, Table1.LastName
	FROM Table1
	LEFT JOIN Table2 t2
		--ON t2.Table1Id = Table1.Table1Id
		ON Table1.Table1Id = t2.Tbl1Id
			AND Table1.Table1Id = t2.Tbl1Id
	INNER JOIN Table4 t3 
		ON Table1.Table1Id = t3.Id
	WHERE t2.Id IS NULL AND  t2.EnteredDate >= CONVERT(VARCHAR(20), '1/1/1970')

	select t2.* from Table2 t2, @foo f where t2.Id * 2 = f.id * 2 / @param1


	select t1.Table1Id, t1.FirstName, t1.LastName, t1.Age 
	from Table1 t1 
	inner join Table2 t2 on 1 = 1
	INNER JOIN Table1 t3 on 1 = 1
	INNER JOIN Table2 t4 on 1 = 1
	INNER JOIN Table1 t5 on 1 = 1
	INNER JOIN Table2 t6 on 1 = 1
	INNER JOIN Table1 t7 on 1 = 1
	inner join Table2 t8 on 1 = 1
	INNER JOIN Table1 t9 on 1 = 1
	INNER JOIN Table2 t10 on 1 = 1
	INNER JOIN Table1 t11 on 1 = 1
	INNER JOIN Table2 t12 on 1 = 1
	INNER JOIN Table1 t13 on 1 = 1
	INNER JOIN View1 vw1 on 1 <> 1
	WHERE t1.Table1Id = NULL

RETURN 0
