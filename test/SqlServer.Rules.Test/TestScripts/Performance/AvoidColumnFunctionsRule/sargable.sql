CREATE VIEW [dbo].[sargable] AS
	SELECT table1_id FROM [dbo].[table1] WHERE [modified_date] < dateadd(day,-31,'2000-01-31 00:00:00.000')
GO