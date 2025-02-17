CREATE TABLE [Production].[ProductPhoto] (
    [ProductPhotoID]         INT             IDENTITY (1, 1) NOT NULL,
    [ThumbNailPhoto]         VARBINARY (MAX) NULL,
    [ThumbnailPhotoFileName] NVARCHAR (50)   NULL,
    [LargePhoto]             VARBINARY (MAX) NULL,
    [LargePhotoFileName]     NVARCHAR (50)   NULL,
    [ModifiedDate]           DATETIME        CONSTRAINT [DF_ProductPhoto_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProductPhoto_ProductPhotoID] PRIMARY KEY CLUSTERED ([ProductPhotoID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Small image file name.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'COLUMN', @level2name = N'ThumbnailPhotoFileName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Large image of the product.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'COLUMN', @level2name = N'LargePhoto';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Large image file name.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'COLUMN', @level2name = N'LargePhotoFileName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'CONSTRAINT', @level2name = N'PK_ProductPhoto_ProductPhotoID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product images.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for ProductPhoto records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'COLUMN', @level2name = N'ProductPhotoID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Small image of the product.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'COLUMN', @level2name = N'ThumbNailPhoto';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductPhoto', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductPhoto_ModifiedDate';


GO

