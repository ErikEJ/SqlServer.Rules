CREATE TABLE [Person].[EmailAddress] (
    [BusinessEntityID] INT              NOT NULL,
    [EmailAddressID]   INT              IDENTITY (1, 1) NOT NULL,
    [EmailAddress]     NVARCHAR (50)    NULL,
    [rowguid]          UNIQUEIDENTIFIER CONSTRAINT [DF_EmailAddress_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]     DATETIME         CONSTRAINT [DF_EmailAddress_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_EmailAddress_BusinessEntityID_EmailAddressID] PRIMARY KEY CLUSTERED ([BusinessEntityID] ASC, [EmailAddressID] ASC),
    CONSTRAINT [FK_EmailAddress_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'INDEX', @level2name = N'IX_EmailAddress_EmailAddress';


GO

CREATE NONCLUSTERED INDEX [IX_EmailAddress_EmailAddress]
    ON [Person].[EmailAddress]([EmailAddress] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Person.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'CONSTRAINT', @level2name = N'FK_EmailAddress_Person_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'E-mail address for the person.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'COLUMN', @level2name = N'EmailAddress';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Where to send a person email.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'CONSTRAINT', @level2name = N'DF_EmailAddress_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'CONSTRAINT', @level2name = N'PK_EmailAddress_BusinessEntityID_EmailAddressID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Person associated with this email address.  Foreign key to Person.BusinessEntityID', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'COLUMN', @level2name = N'BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'CONSTRAINT', @level2name = N'DF_EmailAddress_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. ID of this email address.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'EmailAddress', @level2type = N'COLUMN', @level2name = N'EmailAddressID';


GO

