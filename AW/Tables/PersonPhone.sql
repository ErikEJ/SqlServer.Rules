CREATE TABLE [Person].[PersonPhone] (
    [BusinessEntityID]  INT           NOT NULL,
    [PhoneNumber]       [dbo].[Phone] NOT NULL,
    [PhoneNumberTypeID] INT           NOT NULL,
    [ModifiedDate]      DATETIME      CONSTRAINT [DF_PersonPhone_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID] PRIMARY KEY CLUSTERED ([BusinessEntityID] ASC, [PhoneNumber] ASC, [PhoneNumberTypeID] ASC),
    CONSTRAINT [FK_PersonPhone_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID]),
    CONSTRAINT [FK_PersonPhone_PhoneNumberType_PhoneNumberTypeID] FOREIGN KEY ([PhoneNumberTypeID]) REFERENCES [Person].[PhoneNumberType] ([PhoneNumberTypeID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'INDEX', @level2name = N'IX_PersonPhone_PhoneNumber';


GO

CREATE NONCLUSTERED INDEX [IX_PersonPhone_PhoneNumber]
    ON [Person].[PersonPhone]([PhoneNumber] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Business entity identification number. Foreign key to Person.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'COLUMN', @level2name = N'BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'CONSTRAINT', @level2name = N'PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Telephone number and type of a person.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'CONSTRAINT', @level2name = N'DF_PersonPhone_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Telephone number identification number.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'COLUMN', @level2name = N'PhoneNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Kind of phone number. Foreign key to PhoneNumberType.PhoneNumberTypeID.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'COLUMN', @level2name = N'PhoneNumberTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Person.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'CONSTRAINT', @level2name = N'FK_PersonPhone_Person_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing PhoneNumberType.PhoneNumberTypeID.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PersonPhone', @level2type = N'CONSTRAINT', @level2name = N'FK_PersonPhone_PhoneNumberType_PhoneNumberTypeID';


GO

