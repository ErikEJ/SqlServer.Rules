CREATE TABLE [Person].[ContactType] (
    [ContactTypeID] INT          IDENTITY (1, 1) NOT NULL,
    [Name]          [dbo].[Name] NOT NULL,
    [ModifiedDate]  DATETIME     CONSTRAINT [DF_ContactType_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ContactType_ContactTypeID] PRIMARY KEY CLUSTERED ([ContactTypeID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType', @level2type = N'INDEX', @level2name = N'AK_ContactType_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_ContactType_Name]
    ON [Person].[ContactType]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Lookup table containing the types of business entity contacts.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Contact type description.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType', @level2type = N'CONSTRAINT', @level2name = N'PK_ContactType_ContactTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType', @level2type = N'CONSTRAINT', @level2name = N'DF_ContactType_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for ContactType records.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'ContactType', @level2type = N'COLUMN', @level2name = N'ContactTypeID';


GO

