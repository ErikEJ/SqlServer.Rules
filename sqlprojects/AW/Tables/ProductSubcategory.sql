CREATE TABLE [Production].[ProductSubcategory] (
    [ProductSubcategoryID] INT              IDENTITY (1, 1) NOT NULL,
    [ProductCategoryID]    INT              NOT NULL,
    [Name]                 [dbo].[Name]     NOT NULL,
    [rowguid]              UNIQUEIDENTIFIER CONSTRAINT [DF_ProductSubcategory_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]         DATETIME         CONSTRAINT [DF_ProductSubcategory_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProductSubcategory_ProductSubcategoryID] PRIMARY KEY CLUSTERED ([ProductSubcategoryID] ASC),
    CONSTRAINT [FK_ProductSubcategory_ProductCategory_ProductCategoryID] FOREIGN KEY ([ProductCategoryID]) REFERENCES [Production].[ProductCategory] ([ProductCategoryID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'INDEX', @level2name = N'AK_ProductSubcategory_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductSubcategory_Name]
    ON [Production].[ProductSubcategory]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'INDEX', @level2name = N'AK_ProductSubcategory_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductSubcategory_rowguid]
    ON [Production].[ProductSubcategory]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product subcategories. See ProductCategory table.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for ProductSubcategory records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'COLUMN', @level2name = N'ProductSubcategoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product category identification number. Foreign key to ProductCategory.ProductCategoryID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'COLUMN', @level2name = N'ProductCategoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Subcategory description.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductSubcategory_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'CONSTRAINT', @level2name = N'PK_ProductSubcategory_ProductSubcategoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductSubcategory_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ProductCategory.ProductCategoryID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductSubcategory', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductSubcategory_ProductCategory_ProductCategoryID';


GO

