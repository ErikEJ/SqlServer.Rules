CREATE TABLE [Person].[AddressType] (
    [AddressTypeID] INT              IDENTITY (1, 1) NOT NULL,
    [Name]          [dbo].[Name]     NOT NULL,
    [rowguid]       UNIQUEIDENTIFIER CONSTRAINT [DF_AddressType_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]  DATETIME         CONSTRAINT [DF_AddressType_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_AddressType_AddressTypeID] PRIMARY KEY CLUSTERED ([AddressTypeID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'INDEX', @level2name = N'AK_AddressType_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_AddressType_Name]
    ON [Person].[AddressType]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'INDEX', @level2name = N'AK_AddressType_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_AddressType_rowguid]
    ON [Person].[AddressType]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Address type description. For example, Billing, Home, or Shipping.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'CONSTRAINT', @level2name = N'DF_AddressType_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for AddressType records.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'COLUMN', @level2name = N'AddressTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'CONSTRAINT', @level2name = N'PK_AddressType_AddressTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Types of addresses stored in the Address table. ', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'AddressType', @level2type = N'CONSTRAINT', @level2name = N'DF_AddressType_ModifiedDate';


GO

