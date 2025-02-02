CREATE TABLE [Production].[Culture] (
    [CultureID]    NCHAR (6)    NOT NULL,
    [Name]         [dbo].[Name] NOT NULL,
    [ModifiedDate] DATETIME     CONSTRAINT [DF_Culture_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Culture_CultureID] PRIMARY KEY CLUSTERED ([CultureID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture', @level2type = N'INDEX', @level2name = N'AK_Culture_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Culture_Name]
    ON [Production].[Culture]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Culture description.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for Culture records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture', @level2type = N'COLUMN', @level2name = N'CultureID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture', @level2type = N'CONSTRAINT', @level2name = N'DF_Culture_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture', @level2type = N'CONSTRAINT', @level2name = N'PK_Culture_CultureID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Lookup table containing the languages in which some AdventureWorks data is stored.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Culture';


GO

