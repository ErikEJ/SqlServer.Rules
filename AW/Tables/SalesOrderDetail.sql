CREATE TABLE [Sales].[SalesOrderDetail] (
    [SalesOrderID]          INT              NOT NULL,
    [SalesOrderDetailID]    INT              IDENTITY (1, 1) NOT NULL,
    [CarrierTrackingNumber] NVARCHAR (25)    NULL,
    [OrderQty]              SMALLINT         NOT NULL,
    [ProductID]             INT              NOT NULL,
    [SpecialOfferID]        INT              NOT NULL,
    [UnitPrice]             MONEY            NOT NULL,
    [UnitPriceDiscount]     MONEY            CONSTRAINT [DF_SalesOrderDetail_UnitPriceDiscount] DEFAULT ((0.0)) NOT NULL,
    [LineTotal]             AS               (isnull(([UnitPrice]*((1.0)-[UnitPriceDiscount]))*[OrderQty],(0.0))),
    [rowguid]               UNIQUEIDENTIFIER CONSTRAINT [DF_SalesOrderDetail_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]          DATETIME         CONSTRAINT [DF_SalesOrderDetail_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] PRIMARY KEY CLUSTERED ([SalesOrderID] ASC, [SalesOrderDetailID] ASC),
    CONSTRAINT [CK_SalesOrderDetail_OrderQty] CHECK ([OrderQty]>(0)),
    CONSTRAINT [CK_SalesOrderDetail_UnitPrice] CHECK ([UnitPrice]>=(0.00)),
    CONSTRAINT [CK_SalesOrderDetail_UnitPriceDiscount] CHECK ([UnitPriceDiscount]>=(0.00)),
    CONSTRAINT [FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID]) REFERENCES [Sales].[SalesOrderHeader] ([SalesOrderID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID] FOREIGN KEY ([SpecialOfferID], [ProductID]) REFERENCES [Sales].[SpecialOfferProduct] ([SpecialOfferID], [ProductID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'INDEX', @level2name = N'AK_SalesOrderDetail_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesOrderDetail_rowguid]
    ON [Sales].[SalesOrderDetail]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'INDEX', @level2name = N'IX_SalesOrderDetail_ProductID';


GO

CREATE NONCLUSTERED INDEX [IX_SalesOrderDetail_ProductID]
    ON [Sales].[SalesOrderDetail]([ProductID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'AFTER INSERT, DELETE, UPDATE trigger that inserts a row in the TransactionHistory table, updates ModifiedDate in SalesOrderDetail and updates the SalesOrderHeader.SubTotal column.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'TRIGGER', @level2name = N'iduSalesOrderDetail';


GO


CREATE TRIGGER [Sales].[iduSalesOrderDetail] ON [Sales].[SalesOrderDetail] 
AFTER INSERT, DELETE, UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- If inserting or updating these columns
        IF UPDATE([ProductID]) OR UPDATE([OrderQty]) OR UPDATE([UnitPrice]) OR UPDATE([UnitPriceDiscount]) 
        -- Insert record into TransactionHistory
        BEGIN
            INSERT INTO [Production].[TransactionHistory]
                ([ProductID]
                ,[ReferenceOrderID]
                ,[ReferenceOrderLineID]
                ,[TransactionType]
                ,[TransactionDate]
                ,[Quantity]
                ,[ActualCost])
            SELECT 
                inserted.[ProductID]
                ,inserted.[SalesOrderID]
                ,inserted.[SalesOrderDetailID]
                ,'S'
                ,GETDATE()
                ,inserted.[OrderQty]
                ,inserted.[UnitPrice]
            FROM inserted 
                INNER JOIN [Sales].[SalesOrderHeader] 
                ON inserted.[SalesOrderID] = [Sales].[SalesOrderHeader].[SalesOrderID];

            UPDATE [Person].[Person] 
            SET [Demographics].modify('declare default element namespace 
                "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
                replace value of (/IndividualSurvey/TotalPurchaseYTD)[1] 
                with data(/IndividualSurvey/TotalPurchaseYTD)[1] + sql:column ("inserted.LineTotal")') 
            FROM inserted 
                INNER JOIN [Sales].[SalesOrderHeader] AS SOH
                ON inserted.[SalesOrderID] = SOH.[SalesOrderID] 
                INNER JOIN [Sales].[Customer] AS C
                ON SOH.[CustomerID] = C.[CustomerID]
            WHERE C.[PersonID] = [Person].[Person].[BusinessEntityID];
        END;

        -- Update SubTotal in SalesOrderHeader record. Note that this causes the 
        -- SalesOrderHeader trigger to fire which will update the RevisionNumber.
        UPDATE [Sales].[SalesOrderHeader]
        SET [Sales].[SalesOrderHeader].[SubTotal] = 
            (SELECT SUM([Sales].[SalesOrderDetail].[LineTotal])
                FROM [Sales].[SalesOrderDetail]
                WHERE [Sales].[SalesOrderHeader].[SalesOrderID] = [Sales].[SalesOrderDetail].[SalesOrderID])
        WHERE [Sales].[SalesOrderHeader].[SalesOrderID] IN (SELECT inserted.[SalesOrderID] FROM inserted);

        UPDATE [Person].[Person] 
        SET [Demographics].modify('declare default element namespace 
            "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
            replace value of (/IndividualSurvey/TotalPurchaseYTD)[1] 
            with data(/IndividualSurvey/TotalPurchaseYTD)[1] - sql:column("deleted.LineTotal")') 
        FROM deleted 
            INNER JOIN [Sales].[SalesOrderHeader] 
            ON deleted.[SalesOrderID] = [Sales].[SalesOrderHeader].[SalesOrderID] 
            INNER JOIN [Sales].[Customer]
            ON [Sales].[Customer].[CustomerID] = [Sales].[SalesOrderHeader].[CustomerID]
        WHERE [Sales].[Customer].[PersonID] = [Person].[Person].[BusinessEntityID];
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Quantity ordered per product.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'OrderQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Selling price of a single product.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'UnitPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [UnitPrice] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderDetail_UnitPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'SalesOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [UnitPriceDiscount] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderDetail_UnitPriceDiscount';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderDetail_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product sold to customer. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing SpecialOfferProduct.SpecialOfferIDProductID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Discount amount.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'UnitPriceDiscount';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. One incremental unique number per product sold.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'SalesOrderDetailID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing SalesOrderHeader.PurchaseOrderID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'LineTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderDetail_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Promotional code. Foreign key to SpecialOffer.SpecialOfferID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'SpecialOfferID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipment tracking number supplied by the shipper.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'COLUMN', @level2name = N'CarrierTrackingNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Individual products associated with a specific sales order. See SalesOrderHeader.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderDetail_UnitPriceDiscount';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [OrderQty] > (0)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderDetail', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderDetail_OrderQty';


GO

