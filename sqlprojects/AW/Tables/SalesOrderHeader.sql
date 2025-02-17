CREATE TABLE [Sales].[SalesOrderHeader] (
    [SalesOrderID]           INT                   IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [RevisionNumber]         TINYINT               CONSTRAINT [DF_SalesOrderHeader_RevisionNumber] DEFAULT ((0)) NOT NULL,
    [OrderDate]              DATETIME              CONSTRAINT [DF_SalesOrderHeader_OrderDate] DEFAULT (getdate()) NOT NULL,
    [DueDate]                DATETIME              NOT NULL,
    [ShipDate]               DATETIME              NULL,
    [Status]                 TINYINT               CONSTRAINT [DF_SalesOrderHeader_Status] DEFAULT ((1)) NOT NULL,
    [OnlineOrderFlag]        [dbo].[Flag]          CONSTRAINT [DF_SalesOrderHeader_OnlineOrderFlag] DEFAULT ((1)) NOT NULL,
    [SalesOrderNumber]       AS                    (isnull(N'SO'+CONVERT([nvarchar](23),[SalesOrderID],0),N'*** ERROR ***')),
    [PurchaseOrderNumber]    [dbo].[OrderNumber]   NULL,
    [AccountNumber]          [dbo].[AccountNumber] NULL,
    [CustomerID]             INT                   NOT NULL,
    [SalesPersonID]          INT                   NULL,
    [TerritoryID]            INT                   NULL,
    [BillToAddressID]        INT                   NOT NULL,
    [ShipToAddressID]        INT                   NOT NULL,
    [ShipMethodID]           INT                   NOT NULL,
    [CreditCardID]           INT                   NULL,
    [CreditCardApprovalCode] VARCHAR (15)          NULL,
    [CurrencyRateID]         INT                   NULL,
    [SubTotal]               MONEY                 CONSTRAINT [DF_SalesOrderHeader_SubTotal] DEFAULT ((0.00)) NOT NULL,
    [TaxAmt]                 MONEY                 CONSTRAINT [DF_SalesOrderHeader_TaxAmt] DEFAULT ((0.00)) NOT NULL,
    [Freight]                MONEY                 CONSTRAINT [DF_SalesOrderHeader_Freight] DEFAULT ((0.00)) NOT NULL,
    [TotalDue]               AS                    (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
    [Comment]                NVARCHAR (128)        NULL,
    [rowguid]                UNIQUEIDENTIFIER      CONSTRAINT [DF_SalesOrderHeader_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]           DATETIME              CONSTRAINT [DF_SalesOrderHeader_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SalesOrderHeader_SalesOrderID] PRIMARY KEY CLUSTERED ([SalesOrderID] ASC),
    CONSTRAINT [CK_SalesOrderHeader_DueDate] CHECK ([DueDate]>=[OrderDate]),
    CONSTRAINT [CK_SalesOrderHeader_Freight] CHECK ([Freight]>=(0.00)),
    CONSTRAINT [CK_SalesOrderHeader_ShipDate] CHECK ([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL),
    CONSTRAINT [CK_SalesOrderHeader_Status] CHECK ([Status]>=(0) AND [Status]<=(8)),
    CONSTRAINT [CK_SalesOrderHeader_SubTotal] CHECK ([SubTotal]>=(0.00)),
    CONSTRAINT [CK_SalesOrderHeader_TaxAmt] CHECK ([TaxAmt]>=(0.00)),
    CONSTRAINT [FK_SalesOrderHeader_Address_BillToAddressID] FOREIGN KEY ([BillToAddressID]) REFERENCES [Person].[Address] ([AddressID]),
    CONSTRAINT [FK_SalesOrderHeader_Address_ShipToAddressID] FOREIGN KEY ([ShipToAddressID]) REFERENCES [Person].[Address] ([AddressID]),
    CONSTRAINT [FK_SalesOrderHeader_CreditCard_CreditCardID] FOREIGN KEY ([CreditCardID]) REFERENCES [Sales].[CreditCard] ([CreditCardID]),
    CONSTRAINT [FK_SalesOrderHeader_CurrencyRate_CurrencyRateID] FOREIGN KEY ([CurrencyRateID]) REFERENCES [Sales].[CurrencyRate] ([CurrencyRateID]),
    CONSTRAINT [FK_SalesOrderHeader_Customer_CustomerID] FOREIGN KEY ([CustomerID]) REFERENCES [Sales].[Customer] ([CustomerID]),
    CONSTRAINT [FK_SalesOrderHeader_SalesPerson_SalesPersonID] FOREIGN KEY ([SalesPersonID]) REFERENCES [Sales].[SalesPerson] ([BusinessEntityID]),
    CONSTRAINT [FK_SalesOrderHeader_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID]) REFERENCES [Sales].[SalesTerritory] ([TerritoryID]),
    CONSTRAINT [FK_SalesOrderHeader_ShipMethod_ShipMethodID] FOREIGN KEY ([ShipMethodID]) REFERENCES [Purchasing].[ShipMethod] ([ShipMethodID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'INDEX', @level2name = N'AK_SalesOrderHeader_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesOrderHeader_rowguid]
    ON [Sales].[SalesOrderHeader]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'INDEX', @level2name = N'IX_SalesOrderHeader_SalesPersonID';


GO

CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_SalesPersonID]
    ON [Sales].[SalesOrderHeader]([SalesPersonID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'INDEX', @level2name = N'IX_SalesOrderHeader_CustomerID';


GO

CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_CustomerID]
    ON [Sales].[SalesOrderHeader]([CustomerID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'INDEX', @level2name = N'AK_SalesOrderHeader_SalesOrderNumber';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesOrderHeader_SalesOrderNumber]
    ON [Sales].[SalesOrderHeader]([SalesOrderNumber] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'AFTER UPDATE trigger that updates the RevisionNumber and ModifiedDate columns in the SalesOrderHeader table.Updates the SalesYTD column in the SalesPerson and SalesTerritory tables.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'TRIGGER', @level2name = N'uSalesOrderHeader';


GO


CREATE TRIGGER [Sales].[uSalesOrderHeader] ON [Sales].[SalesOrderHeader] 
AFTER UPDATE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- Update RevisionNumber for modification of any field EXCEPT the Status.
        IF NOT UPDATE([Status])
        BEGIN
            UPDATE [Sales].[SalesOrderHeader]
            SET [Sales].[SalesOrderHeader].[RevisionNumber] = 
                [Sales].[SalesOrderHeader].[RevisionNumber] + 1
            WHERE [Sales].[SalesOrderHeader].[SalesOrderID] IN 
                (SELECT inserted.[SalesOrderID] FROM inserted);
        END;

        -- Update the SalesPerson SalesYTD when SubTotal is updated
        IF UPDATE([SubTotal])
        BEGIN
            DECLARE @StartDate datetime,
                    @EndDate datetime

            SET @StartDate = [dbo].[ufnGetAccountingStartDate]();
            SET @EndDate = [dbo].[ufnGetAccountingEndDate]();

            UPDATE [Sales].[SalesPerson]
            SET [Sales].[SalesPerson].[SalesYTD] = 
                (SELECT SUM([Sales].[SalesOrderHeader].[SubTotal])
                FROM [Sales].[SalesOrderHeader] 
                WHERE [Sales].[SalesPerson].[BusinessEntityID] = [Sales].[SalesOrderHeader].[SalesPersonID]
                    AND ([Sales].[SalesOrderHeader].[Status] = 5) -- Shipped
                    AND [Sales].[SalesOrderHeader].[OrderDate] BETWEEN @StartDate AND @EndDate)
            WHERE [Sales].[SalesPerson].[BusinessEntityID] 
                IN (SELECT DISTINCT inserted.[SalesPersonID] FROM inserted 
                    WHERE inserted.[OrderDate] BETWEEN @StartDate AND @EndDate);

            -- Update the SalesTerritory SalesYTD when SubTotal is updated
            UPDATE [Sales].[SalesTerritory]
            SET [Sales].[SalesTerritory].[SalesYTD] = 
                (SELECT SUM([Sales].[SalesOrderHeader].[SubTotal])
                FROM [Sales].[SalesOrderHeader] 
                WHERE [Sales].[SalesTerritory].[TerritoryID] = [Sales].[SalesOrderHeader].[TerritoryID]
                    AND ([Sales].[SalesOrderHeader].[Status] = 5) -- Shipped
                    AND [Sales].[SalesOrderHeader].[OrderDate] BETWEEN @StartDate AND @EndDate)
            WHERE [Sales].[SalesTerritory].[TerritoryID] 
                IN (SELECT DISTINCT inserted.[TerritoryID] FROM inserted 
                    WHERE inserted.[OrderDate] BETWEEN @StartDate AND @EndDate);
        END;
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

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Dates the sales order was created.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'OrderDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Financial accounting number reference.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'AccountNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Status] BETWEEN (0) AND (8)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderHeader_Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_RevisionNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_SubTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique sales order identification number.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'SalesOrderNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [SubTotal] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderHeader_SubTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Currency exchange rate used. Foreign key to CurrencyRate.CurrencyRateID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'CurrencyRateID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Credit card identification number. Foreign key to CreditCard.CreditCardID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'CreditCardID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'SalesOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing SalesPerson.SalesPersonID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_SalesPerson_SalesPersonID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Address.AddressID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_Address_BillToAddressID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Total due from customer. Computed as Subtotal + TaxAmt + Freight.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'TotalDue';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing SalesTerritory.TerritoryID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_SalesTerritory_TerritoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ShipMethod.ShipMethodID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_ShipMethod_ShipMethodID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [DueDate] >= [OrderDate]', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderHeader_DueDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Tax amount.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'TaxAmt';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Customer shipping address. Foreign key to Address.AddressID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'ShipToAddressID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Territory in which the sale was made. Foreign key to SalesTerritory.SalesTerritoryID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'TerritoryID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing CreditCard.CreditCardID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_CreditCard_CreditCardID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Address.AddressID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_Address_ShipToAddressID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Customer.CustomerID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_Customer_CustomerID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing CurrencyRate.CurrencyRateID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeader_CurrencyRate_CurrencyRateID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [TaxAmt] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderHeader_TaxAmt';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Customer purchase order number reference. ', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'PurchaseOrderNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_OrderDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Customer identification number. Foreign key to Customer.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'CustomerID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_Freight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the order is due to the customer.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'DueDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Freight] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderHeader_Freight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Incremental number to track changes to the sales order over time.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'RevisionNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Order placed by sales person. 1 = Order placed online by customer.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'OnlineOrderFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping method. Foreign key to ShipMethod.ShipMethodID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'ShipMethodID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Approval code provided by the credit card company.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'CreditCardApprovalCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_TaxAmt';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'PK_SalesOrderHeader_SalesOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Sales representative comments.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'Comment';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_SalesOrderHeader_ShipDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Sales person who created the sales order. Foreign key to SalesPerson.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'SalesPersonID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1 (TRUE)', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeader_OnlineOrderFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Customer billing address. Foreign key to Address.AddressID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'BillToAddressID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'SubTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'General sales order information.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping cost.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'Freight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the order was shipped to the customer.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeader', @level2type = N'COLUMN', @level2name = N'ShipDate';


GO

