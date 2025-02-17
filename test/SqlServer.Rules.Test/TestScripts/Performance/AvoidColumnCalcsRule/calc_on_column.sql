CREATE VIEW [dbo].[calc_on_calumn] AS
	SELECT table1_id FROM [dbo].[table1] WHERE table1_id * 500  > 5000
GO