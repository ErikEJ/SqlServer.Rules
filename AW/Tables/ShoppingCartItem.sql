CREATE TABLE [Sales].[ShoppingCartItem] (
    [ShoppingCartItemID] INT           IDENTITY (1, 1) NOT NULL,
    [ShoppingCartID]     NVARCHAR (50) NOT NULL,
    [Quantity]           INT           CONSTRAINT [DF_ShoppingCartItem_Quantity] DEFAULT ((1)) NOT NULL,
    [ProductID]          INT           NOT NULL,
    [DateCreated]        DATETIME      CONSTRAINT [DF_ShoppingCartItem_DateCreated] DEFAULT (getdate()) NOT NULL,
    [ModifiedDate]       DATETIME      CONSTRAINT [DF_ShoppingCartItem_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ShoppingCartItem_ShoppingCartItemID] PRIMARY KEY CLUSTERED ([ShoppingCartItemID] ASC),
    CONSTRAINT [CK_ShoppingCartItem_Quantity] CHECK ([Quantity]>=(1)),
    CONSTRAINT [FK_ShoppingCartItem_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'INDEX', @level2name = N'IX_ShoppingCartItem_ShoppingCartID_ProductID';


GO

CREATE NONCLUSTERED INDEX [IX_ShoppingCartItem_ShoppingCartID_ProductID]
    ON [Sales].[ShoppingCartItem]([ShoppingCartID] ASC, [ProductID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shopping cart identification number.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'COLUMN', @level2name = N'ShoppingCartID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Quantity] >= (1)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'CONSTRAINT', @level2name = N'CK_ShoppingCartItem_Quantity';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product quantity ordered.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'COLUMN', @level2name = N'Quantity';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for ShoppingCartItem records.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'COLUMN', @level2name = N'ShoppingCartItemID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'CONSTRAINT', @level2name = N'FK_ShoppingCartItem_Product_ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'CONSTRAINT', @level2name = N'DF_ShoppingCartItem_Quantity';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'CONSTRAINT', @level2name = N'DF_ShoppingCartItem_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Contains online customer orders until the order is submitted or cancelled.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'CONSTRAINT', @level2name = N'DF_ShoppingCartItem_DateCreated';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'CONSTRAINT', @level2name = N'PK_ShoppingCartItem_ShoppingCartItemID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product ordered. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the time the record was created.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'COLUMN', @level2name = N'DateCreated';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'ShoppingCartItem', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

