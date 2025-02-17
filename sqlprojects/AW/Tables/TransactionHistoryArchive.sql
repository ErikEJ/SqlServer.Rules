CREATE TABLE [Production].[TransactionHistoryArchive] (
    [TransactionID]        INT       NOT NULL,
    [ProductID]            INT       NOT NULL,
    [ReferenceOrderID]     INT       NOT NULL,
    [ReferenceOrderLineID] INT       CONSTRAINT [DF_TransactionHistoryArchive_ReferenceOrderLineID] DEFAULT ((0)) NOT NULL,
    [TransactionDate]      DATETIME  CONSTRAINT [DF_TransactionHistoryArchive_TransactionDate] DEFAULT (getdate()) NOT NULL,
    [TransactionType]      NCHAR (1) NOT NULL,
    [Quantity]             INT       NOT NULL,
    [ActualCost]           MONEY     NOT NULL,
    [ModifiedDate]         DATETIME  CONSTRAINT [DF_TransactionHistoryArchive_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_TransactionHistoryArchive_TransactionID] PRIMARY KEY CLUSTERED ([TransactionID] ASC),
    CONSTRAINT [CK_TransactionHistoryArchive_TransactionType] CHECK (upper([TransactionType])='P' OR upper([TransactionType])='S' OR upper([TransactionType])='W')
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'INDEX', @level2name = N'IX_TransactionHistoryArchive_ProductID';


GO

CREATE NONCLUSTERED INDEX [IX_TransactionHistoryArchive_ProductID]
    ON [Production].[TransactionHistoryArchive]([ProductID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'INDEX', @level2name = N'IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID';


GO

CREATE NONCLUSTERED INDEX [IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID]
    ON [Production].[TransactionHistoryArchive]([ReferenceOrderID] ASC, [ReferenceOrderLineID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'CONSTRAINT', @level2name = N'DF_TransactionHistoryArchive_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Line number associated with the purchase order, sales order, or work order.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'ReferenceOrderLineID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'CONSTRAINT', @level2name = N'DF_TransactionHistoryArchive_ReferenceOrderLineID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time of the transaction.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'TransactionDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'CONSTRAINT', @level2name = N'DF_TransactionHistoryArchive_TransactionDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'W = Work Order, S = Sales Order, P = Purchase Order', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'TransactionType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Transactions for previous years.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for TransactionHistoryArchive records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'TransactionID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product quantity.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'Quantity';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product identification number. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [TransactionType]=''p'' OR [TransactionType]=''s'' OR [TransactionType]=''w'' OR [TransactionType]=''P'' OR [TransactionType]=''S'' OR [TransactionType]=''W''', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'CONSTRAINT', @level2name = N'CK_TransactionHistoryArchive_TransactionType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'CONSTRAINT', @level2name = N'PK_TransactionHistoryArchive_TransactionID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product cost.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'ActualCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Purchase order, sales order, or work order identification number.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'TransactionHistoryArchive', @level2type = N'COLUMN', @level2name = N'ReferenceOrderID';


GO

