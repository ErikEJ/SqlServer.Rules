CREATE TABLE [Production].[ProductModelProductDescriptionCulture] (
    [ProductModelID]       INT       NOT NULL,
    [ProductDescriptionID] INT       NOT NULL,
    [CultureID]            NCHAR (6) NOT NULL,
    [ModifiedDate]         DATETIME  CONSTRAINT [DF_ProductModelProductDescriptionCulture_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID] PRIMARY KEY CLUSTERED ([ProductModelID] ASC, [ProductDescriptionID] ASC, [CultureID] ASC),
    CONSTRAINT [FK_ProductModelProductDescriptionCulture_Culture_CultureID] FOREIGN KEY ([CultureID]) REFERENCES [Production].[Culture] ([CultureID]),
    CONSTRAINT [FK_ProductModelProductDescriptionCulture_ProductDescription_ProductDescriptionID] FOREIGN KEY ([ProductDescriptionID]) REFERENCES [Production].[ProductDescription] ([ProductDescriptionID]),
    CONSTRAINT [FK_ProductModelProductDescriptionCulture_ProductModel_ProductModelID] FOREIGN KEY ([ProductModelID]) REFERENCES [Production].[ProductModel] ([ProductModelID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to ProductModel.ProductModelID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'COLUMN', @level2name = N'ProductModelID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ProductDescription.ProductDescriptionID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductModelProductDescriptionCulture_ProductDescription_ProductDescriptionID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Culture.CultureID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductModelProductDescriptionCulture_Culture_CultureID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ProductModel.ProductModelID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductModelProductDescriptionCulture_ProductModel_ProductModelID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to ProductDescription.ProductDescriptionID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'COLUMN', @level2name = N'ProductDescriptionID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Culture identification number. Foreign key to Culture.CultureID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'COLUMN', @level2name = N'CultureID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductModelProductDescriptionCulture_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture', @level2type = N'CONSTRAINT', @level2name = N'PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Cross-reference table mapping product descriptions and the language the description is written in.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductModelProductDescriptionCulture';


GO

