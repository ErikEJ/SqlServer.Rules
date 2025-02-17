CREATE TABLE [Sales].[SpecialOffer] (
    [SpecialOfferID] INT              IDENTITY (1, 1) NOT NULL,
    [Description]    NVARCHAR (255)   NOT NULL,
    [DiscountPct]    SMALLMONEY       CONSTRAINT [DF_SpecialOffer_DiscountPct] DEFAULT ((0.00)) NOT NULL,
    [Type]           NVARCHAR (50)    NOT NULL,
    [Category]       NVARCHAR (50)    NOT NULL,
    [StartDate]      DATETIME         NOT NULL,
    [EndDate]        DATETIME         NOT NULL,
    [MinQty]         INT              CONSTRAINT [DF_SpecialOffer_MinQty] DEFAULT ((0)) NOT NULL,
    [MaxQty]         INT              NULL,
    [rowguid]        UNIQUEIDENTIFIER CONSTRAINT [DF_SpecialOffer_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]   DATETIME         CONSTRAINT [DF_SpecialOffer_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SpecialOffer_SpecialOfferID] PRIMARY KEY CLUSTERED ([SpecialOfferID] ASC),
    CONSTRAINT [CK_SpecialOffer_DiscountPct] CHECK ([DiscountPct]>=(0.00)),
    CONSTRAINT [CK_SpecialOffer_EndDate] CHECK ([EndDate]>=[StartDate]),
    CONSTRAINT [CK_SpecialOffer_MaxQty] CHECK ([MaxQty]>=(0)),
    CONSTRAINT [CK_SpecialOffer_MinQty] CHECK ([MinQty]>=(0))
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'INDEX', @level2name = N'AK_SpecialOffer_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_SpecialOffer_rowguid]
    ON [Sales].[SpecialOffer]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'DF_SpecialOffer_DiscountPct';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'PK_SpecialOffer_SpecialOfferID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [MinQty] >= (0)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'CK_SpecialOffer_MinQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [DiscountPct] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'CK_SpecialOffer_DiscountPct';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'DF_SpecialOffer_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum discount percent allowed.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'MinQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Discount type category.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'Type';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Discount end date.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'EndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Maximum discount percent allowed.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'MaxQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Discount precentage.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'DiscountPct';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Group the discount applies to such as Reseller or Customer.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'Category';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Discount start date.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'StartDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'DF_SpecialOffer_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [EndDate] >= [StartDate]', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'CK_SpecialOffer_EndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Sale discounts lookup table.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for SpecialOffer records.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'SpecialOfferID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Discount description.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'Description';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'DF_SpecialOffer_MinQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [MaxQty] >= (0)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SpecialOffer', @level2type = N'CONSTRAINT', @level2name = N'CK_SpecialOffer_MaxQty';


GO

