﻿CREATE TRIGGER [dbo_table2_trigger_1] ON dbo.table2 AFTER INSERT NOT FOR REPLICATION AS
	SELECT * FROM Inserted;
GO