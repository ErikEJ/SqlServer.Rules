CREATE TABLE [Production].[Product] (
    [ProductID]             INT              IDENTITY (1, 1) NOT NULL,
    [Name]                  [dbo].[Name]     NOT NULL,
    [ProductNumber]         NVARCHAR (25)    NOT NULL,
    [MakeFlag]              [dbo].[Flag]     CONSTRAINT [DF_Product_MakeFlag] DEFAULT ((1)) NOT NULL,
    [FinishedGoodsFlag]     [dbo].[Flag]     CONSTRAINT [DF_Product_FinishedGoodsFlag] DEFAULT ((1)) NOT NULL,
    [Color]                 NVARCHAR (15)    NULL,
    [SafetyStockLevel]      SMALLINT         NOT NULL,
    [ReorderPoint]          SMALLINT         NOT NULL,
    [StandardCost]          MONEY            NOT NULL,
    [ListPrice]             MONEY            NOT NULL,
    [Size]                  NVARCHAR (5)     NULL,
    [SizeUnitMeasureCode]   NCHAR (3)        NULL,
    [WeightUnitMeasureCode] NCHAR (3)        NULL,
    [Weight]                DECIMAL (8, 2)   NULL,
    [DaysToManufacture]     INT              NOT NULL,
    [ProductLine]           NCHAR (2)        NULL,
    [Class]                 NCHAR (2)        NULL,
    [Style]                 NCHAR (2)        NULL,
    [ProductSubcategoryID]  INT              NULL,
    [ProductModelID]        INT              NULL,
    [SellStartDate]         DATETIME         NOT NULL,
    [SellEndDate]           DATETIME         NULL,
    [DiscontinuedDate]      DATETIME         NULL,
    [rowguid]               UNIQUEIDENTIFIER CONSTRAINT [DF_Product_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]          DATETIME         CONSTRAINT [DF_Product_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Product_ProductID] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [CK_Product_Class] CHECK (upper([Class])='H' OR upper([Class])='M' OR upper([Class])='L' OR [Class] IS NULL),
    CONSTRAINT [CK_Product_DaysToManufacture] CHECK ([DaysToManufacture]>=(0)),
    CONSTRAINT [CK_Product_ListPrice] CHECK ([ListPrice]>=(0.00)),
    CONSTRAINT [CK_Product_ProductLine] CHECK (upper([ProductLine])='R' OR upper([ProductLine])='M' OR upper([ProductLine])='T' OR upper([ProductLine])='S' OR [ProductLine] IS NULL),
    CONSTRAINT [CK_Product_ReorderPoint] CHECK ([ReorderPoint]>(0)),
    CONSTRAINT [CK_Product_SafetyStockLevel] CHECK ([SafetyStockLevel]>(0)),
    CONSTRAINT [CK_Product_SellEndDate] CHECK ([SellEndDate]>=[SellStartDate] OR [SellEndDate] IS NULL),
    CONSTRAINT [CK_Product_StandardCost] CHECK ([StandardCost]>=(0.00)),
    CONSTRAINT [CK_Product_Style] CHECK (upper([Style])='U' OR upper([Style])='M' OR upper([Style])='W' OR [Style] IS NULL),
    CONSTRAINT [CK_Product_Weight] CHECK ([Weight]>(0.00)),
    CONSTRAINT [FK_Product_ProductModel_ProductModelID] FOREIGN KEY ([ProductModelID]) REFERENCES [Production].[ProductModel] ([ProductModelID]),
    CONSTRAINT [FK_Product_ProductSubcategory_ProductSubcategoryID] FOREIGN KEY ([ProductSubcategoryID]) REFERENCES [Production].[ProductSubcategory] ([ProductSubcategoryID]),
    CONSTRAINT [FK_Product_UnitMeasure_SizeUnitMeasureCode] FOREIGN KEY ([SizeUnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode]),
    CONSTRAINT [FK_Product_UnitMeasure_WeightUnitMeasureCode] FOREIGN KEY ([WeightUnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'INDEX', @level2name = N'AK_Product_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Product_rowguid]
    ON [Production].[Product]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'INDEX', @level2name = N'AK_Product_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Product_Name]
    ON [Production].[Product]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'INDEX', @level2name = N'AK_Product_ProductNumber';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Product_ProductNumber]
    ON [Production].[Product]([ProductNumber] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product weight.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'Weight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'PK_Product_ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'DF_Product_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Product is purchased, 1 = Product is manufactured in-house.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'MakeFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ProductLine]=''r'' OR [ProductLine]=''m'' OR [ProductLine]=''t'' OR [ProductLine]=''s'' OR [ProductLine]=''R'' OR [ProductLine]=''M'' OR [ProductLine]=''T'' OR [ProductLine]=''S'' OR [ProductLine] IS NULL', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_ProductLine';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'R = Road, M = Mountain, T = Touring, S = Standard', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ProductLine';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for Product records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Weight] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_Weight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Standard cost of the product.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'StandardCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_SellEndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'W = Womens, M = Mens, U = Universal', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'Style';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product is a member of this product model. Foreign key to ProductModel.ProductModelID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ProductModelID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of  1', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'DF_Product_FinishedGoodsFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product size.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'Size';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the product was no longer available for sale.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'SellEndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product color.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'Color';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ListPrice] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_ListPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unit of measure for Weight column.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'WeightUnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique product identification number.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ProductNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [SafetyStockLevel] > (0)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_SafetyStockLevel';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Products sold or used in the manfacturing of sold products.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Inventory level that triggers a purchase order or work order. ', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ReorderPoint';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'DF_Product_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Style]=''u'' OR [Style]=''m'' OR [Style]=''w'' OR [Style]=''U'' OR [Style]=''M'' OR [Style]=''W'' OR [Style] IS NULL', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_Style';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Number of days required to manufacture the product.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'DaysToManufacture';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [DaysToManufacture] >= (0)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_DaysToManufacture';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'H = High, M = Medium, L = Low', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'Class';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Product is not a salable item. 1 = Product is salable.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'FinishedGoodsFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product is a member of this product subcategory. Foreign key to ProductSubCategory.ProductSubCategoryID. ', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ProductSubcategoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Selling price.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ListPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of  1', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'DF_Product_MakeFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the product was available for sale.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'SellStartDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ReorderPoint] > (0)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_ReorderPoint';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the product.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the product was discontinued.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'DiscontinuedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ProductModel.ProductModelID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'FK_Product_ProductModel_ProductModelID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [SafetyStockLevel] > (0)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_StandardCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unit of measure for Size column.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'SizeUnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum inventory quantity. ', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'COLUMN', @level2name = N'SafetyStockLevel';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ProductSubcategory.ProductSubcategoryID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'FK_Product_ProductSubcategory_ProductSubcategoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'FK_Product_UnitMeasure_WeightUnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'FK_Product_UnitMeasure_SizeUnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Class]=''h'' OR [Class]=''m'' OR [Class]=''l'' OR [Class]=''H'' OR [Class]=''M'' OR [Class]=''L'' OR [Class] IS NULL', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Product', @level2type = N'CONSTRAINT', @level2name = N'CK_Product_Class';


GO

