CREATE VIEW [dbo].[View2]
	AS 
	
	SELECT v1.Table1Id, v1.FirstName, v1.LastName, v1.Age  
	FROM View1 v1
	WHERE LastName LIKE '%foo%' 
		OR 1<>0
