CREATE TABLE [Purchasing].[ProductVendor] (
    [ProductID]        INT       NOT NULL,
    [BusinessEntityID] INT       NOT NULL,
    [AverageLeadTime]  INT       NOT NULL,
    [StandardPrice]    MONEY     NOT NULL,
    [LastReceiptCost]  MONEY     NULL,
    [LastReceiptDate]  DATETIME  NULL,
    [MinOrderQty]      INT       NOT NULL,
    [MaxOrderQty]      INT       NOT NULL,
    [OnOrderQty]       INT       NULL,
    [UnitMeasureCode]  NCHAR (3) NOT NULL,
    [ModifiedDate]     DATETIME  CONSTRAINT [DF_ProductVendor_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProductVendor_ProductID_BusinessEntityID] PRIMARY KEY CLUSTERED ([ProductID] ASC, [BusinessEntityID] ASC),
    CONSTRAINT [CK_ProductVendor_AverageLeadTime] CHECK ([AverageLeadTime]>=(1)),
    CONSTRAINT [CK_ProductVendor_LastReceiptCost] CHECK ([LastReceiptCost]>(0.00)),
    CONSTRAINT [CK_ProductVendor_MaxOrderQty] CHECK ([MaxOrderQty]>=(1)),
    CONSTRAINT [CK_ProductVendor_MinOrderQty] CHECK ([MinOrderQty]>=(1)),
    CONSTRAINT [CK_ProductVendor_OnOrderQty] CHECK ([OnOrderQty]>=(0)),
    CONSTRAINT [CK_ProductVendor_StandardPrice] CHECK ([StandardPrice]>(0.00)),
    CONSTRAINT [FK_ProductVendor_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID]),
    CONSTRAINT [FK_ProductVendor_UnitMeasure_UnitMeasureCode] FOREIGN KEY ([UnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode]),
    CONSTRAINT [FK_ProductVendor_Vendor_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Purchasing].[Vendor] ([BusinessEntityID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'INDEX', @level2name = N'IX_ProductVendor_UnitMeasureCode';


GO

CREATE NONCLUSTERED INDEX [IX_ProductVendor_UnitMeasureCode]
    ON [Purchasing].[ProductVendor]([UnitMeasureCode] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'INDEX', @level2name = N'IX_ProductVendor_BusinessEntityID';


GO

CREATE NONCLUSTERED INDEX [IX_ProductVendor_BusinessEntityID]
    ON [Purchasing].[ProductVendor]([BusinessEntityID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [LastReceiptCost] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductVendor_LastReceiptCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The minimum quantity that should be ordered.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'MaxOrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Vendor.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductVendor_Vendor_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to Vendor.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The maximum quantity that should be ordered.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'MinOrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [OnOrderQty] >= (0)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductVendor_OnOrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the product was last received by the vendor.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'LastReceiptDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductVendor_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [MaxOrderQty] >= (1)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductVendor_MaxOrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductVendor_UnitMeasure_UnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The selling price when last purchased.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'LastReceiptCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [AverageLeadTime] >= (1)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductVendor_AverageLeadTime';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Cross-reference table mapping vendors with the products they supply.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The product''s unit of measure.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'UnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [StandardPrice] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductVendor_StandardPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The vendor''s usual selling price.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'StandardPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductVendor_Product_ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The quantity currently on order.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'OnOrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'PK_ProductVendor_ProductID_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [MinOrderQty] >= (1)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductVendor_MinOrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The average span of time (in days) between placing an order with the vendor and receiving the purchased product.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ProductVendor', @level2type = N'COLUMN', @level2name = N'AverageLeadTime';


GO

