CREATE VIEW [dbo].[neq_ignored] AS
	-- GLOBAL IGNORE SRP0006
	SELECT table1_id FROM [dbo].[table1] WHERE [data] <> 'what am I doing?'
GO