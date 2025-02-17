CREATE TRIGGER [dbo_table2_trigger_2] ON dbo.table2 AFTER INSERT AS
	SELECT * FROM Inserted;
GO