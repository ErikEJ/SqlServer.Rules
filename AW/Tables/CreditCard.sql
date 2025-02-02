CREATE TABLE [Sales].[CreditCard] (
    [CreditCardID] INT           IDENTITY (1, 1) NOT NULL,
    [CardType]     NVARCHAR (50) NOT NULL,
    [CardNumber]   NVARCHAR (25) NOT NULL,
    [ExpMonth]     TINYINT       NOT NULL,
    [ExpYear]      SMALLINT      NOT NULL,
    [ModifiedDate] DATETIME      CONSTRAINT [DF_CreditCard_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_CreditCard_CreditCardID] PRIMARY KEY CLUSTERED ([CreditCardID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'INDEX', @level2name = N'AK_CreditCard_CardNumber';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_CreditCard_CardNumber]
    ON [Sales].[CreditCard]([CardNumber] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Credit card expiration year.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'COLUMN', @level2name = N'ExpYear';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Customer credit card information.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Credit card number.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'COLUMN', @level2name = N'CardNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'CONSTRAINT', @level2name = N'DF_CreditCard_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for CreditCard records.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'COLUMN', @level2name = N'CreditCardID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Credit card expiration month.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'COLUMN', @level2name = N'ExpMonth';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'CONSTRAINT', @level2name = N'PK_CreditCard_CreditCardID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Credit card name.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'CreditCard', @level2type = N'COLUMN', @level2name = N'CardType';


GO

