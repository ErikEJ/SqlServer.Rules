CREATE TABLE [Sales].[Currency] (
    [CurrencyCode] NCHAR (3)    NOT NULL,
    [Name]         [dbo].[Name] NOT NULL,
    [ModifiedDate] DATETIME     CONSTRAINT [DF_Currency_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Currency_CurrencyCode] PRIMARY KEY CLUSTERED ([CurrencyCode] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency', @level2type = N'INDEX', @level2name = N'AK_Currency_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Currency_Name]
    ON [Sales].[Currency]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The ISO code for the Currency.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency', @level2type = N'COLUMN', @level2name = N'CurrencyCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Currency name.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency', @level2type = N'CONSTRAINT', @level2name = N'DF_Currency_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency', @level2type = N'CONSTRAINT', @level2name = N'PK_Currency_CurrencyCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Lookup table containing standard ISO currencies.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'Currency';


GO

