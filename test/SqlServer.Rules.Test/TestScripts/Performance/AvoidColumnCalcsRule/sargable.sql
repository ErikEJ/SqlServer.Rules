CREATE VIEW [dbo].[sargable] AS
	SELECT table1_id FROM [dbo].[table1] WHERE table1_id > 10 

GO