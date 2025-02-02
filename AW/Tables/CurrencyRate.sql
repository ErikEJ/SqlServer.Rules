CREATE TABLE [Sales].[CurrencyRate] (
    [CurrencyRateID]   INT       IDENTITY (1, 1) NOT NULL,
    [CurrencyRateDate] DATETIME  NOT NULL,
    [FromCurrencyCode] NCHAR (3) NOT NULL,
    [ToCurrencyCode]   NCHAR (3) NOT NULL,
    [AverageRate]      MONEY     NOT NULL,
    [EndOfDayRate]     MONEY     NOT NULL,
    [ModifiedDate]     DATETIME  CONSTRAINT [DF_CurrencyRate_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_CurrencyRate_CurrencyRateID] PRIMARY KEY CLUSTERED ([CurrencyRateID] ASC),
    CONSTRAINT [FK_CurrencyRate_Currency_FromCurrencyCode] FOREIGN KEY ([FromCurrencyCode]) REFERENCES [Sales].[Currency] ([CurrencyCode]),
    CONSTRAINT [FK_CurrencyRate_Currency_ToCurrencyCode] FOREIGN KEY ([ToCurrencyCode]) REFERENCES [Sales].[Currency] ([CurrencyCode])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'INDEX', @level2name = N'AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode]
    ON [Sales].[CurrencyRate]([CurrencyRateDate] ASC, [FromCurrencyCode] ASC, [ToCurrencyCode] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'CONSTRAINT', @level2name = N'PK_CurrencyRate_CurrencyRateID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for CurrencyRate records.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'CurrencyRateID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Final exchange rate for the day.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'EndOfDayRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'CONSTRAINT', @level2name = N'DF_CurrencyRate_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Exchange rate was converted to this currency code.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'ToCurrencyCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the exchange rate was obtained.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'CurrencyRateDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Currency exchange rates.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Currency.ToCurrencyCode.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'CONSTRAINT', @level2name = N'FK_CurrencyRate_Currency_ToCurrencyCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Currency.FromCurrencyCode.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'CONSTRAINT', @level2name = N'FK_CurrencyRate_Currency_FromCurrencyCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Exchange rate was converted from this currency code.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'FromCurrencyCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Average exchange rate for the day.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CurrencyRate', @level2type = N'COLUMN', @level2name = N'AverageRate';


GO

