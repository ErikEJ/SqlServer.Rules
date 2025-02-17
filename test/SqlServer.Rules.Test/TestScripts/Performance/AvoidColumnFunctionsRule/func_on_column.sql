CREATE VIEW [dbo].[func_on_calumn] AS
	SELECT table1_id FROM [dbo].[table1] WHERE dateadd(day,30,[modified_date]) < '2000-01-31 00:00:00.000'
GO