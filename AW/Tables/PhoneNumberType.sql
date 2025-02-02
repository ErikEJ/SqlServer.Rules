CREATE TABLE [Person].[PhoneNumberType] (
    [PhoneNumberTypeID] INT          IDENTITY (1, 1) NOT NULL,
    [Name]              [dbo].[Name] NOT NULL,
    [ModifiedDate]      DATETIME     CONSTRAINT [DF_PhoneNumberType_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_PhoneNumberType_PhoneNumberTypeID] PRIMARY KEY CLUSTERED ([PhoneNumberTypeID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PhoneNumberType', @level2type = N'CONSTRAINT', @level2name = N'PK_PhoneNumberType_PhoneNumberTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PhoneNumberType', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Type of phone number of a person.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PhoneNumberType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for telephone number type records.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PhoneNumberType', @level2type = N'COLUMN', @level2name = N'PhoneNumberTypeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the telephone number type', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PhoneNumberType', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'PhoneNumberType', @level2type = N'CONSTRAINT', @level2name = N'DF_PhoneNumberType_ModifiedDate';


GO

