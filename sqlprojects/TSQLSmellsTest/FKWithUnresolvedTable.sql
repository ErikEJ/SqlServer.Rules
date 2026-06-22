CREATE TABLE [dbo].[FKWithUnresolvedTable]
(
    [ID] INT NOT NULL PRIMARY KEY,
    [RefID] INT NOT NULL,
    CONSTRAINT [FK_FKWithUnresolvedTable_RefID] FOREIGN KEY ([RefID]) REFERENCES [dbo].[NonExistentExternalTable] ([ID]),
    INDEX [IX_FKWithUnresolvedTable_RefID] NONCLUSTERED ([RefID])
)
