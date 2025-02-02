CREATE TABLE [Production].[ProductReview] (
    [ProductReviewID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductID]       INT             NOT NULL,
    [ReviewerName]    [dbo].[Name]    NOT NULL,
    [ReviewDate]      DATETIME        CONSTRAINT [DF_ProductReview_ReviewDate] DEFAULT (getdate()) NOT NULL,
    [EmailAddress]    NVARCHAR (50)   NOT NULL,
    [Rating]          INT             NOT NULL,
    [Comments]        NVARCHAR (3850) NULL,
    [ModifiedDate]    DATETIME        CONSTRAINT [DF_ProductReview_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProductReview_ProductReviewID] PRIMARY KEY CLUSTERED ([ProductReviewID] ASC),
    CONSTRAINT [CK_ProductReview_Rating] CHECK ([Rating]>=(1) AND [Rating]<=(5)),
    CONSTRAINT [FK_ProductReview_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'INDEX', @level2name = N'IX_ProductReview_ProductID_Name';


GO

CREATE NONCLUSTERED INDEX [IX_ProductReview_ProductID_Name]
    ON [Production].[ProductReview]([ProductID] ASC, [ReviewerName] ASC)
    INCLUDE([Comments]);


GO

CREATE FULLTEXT INDEX ON [Production].[ProductReview]
    ([Comments] LANGUAGE 1033)
    KEY INDEX [PK_ProductReview_ProductReviewID]
    ON [AW2008FullTextCatalog];


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'CONSTRAINT', @level2name = N'PK_ProductReview_ProductReviewID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductReview_ReviewDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Rating] BETWEEN (1) AND (5)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'CONSTRAINT', @level2name = N'CK_ProductReview_Rating';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Customer reviews of products they have purchased.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for ProductReview records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'ProductReviewID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product identification number. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'CONSTRAINT', @level2name = N'DF_ProductReview_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the reviewer.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'ReviewerName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date review was submitted.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'ReviewDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Reviewer''s e-mail address.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'EmailAddress';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product rating given by the reviewer. Scale is 1 to 5 with 5 as the highest rating.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'Rating';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'CONSTRAINT', @level2name = N'FK_ProductReview_Product_ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Reviewer''s comments', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'ProductReview', @level2type = N'COLUMN', @level2name = N'Comments';


GO

