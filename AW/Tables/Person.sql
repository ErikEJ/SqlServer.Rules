CREATE TABLE [Person].[Person] (
    [BusinessEntityID]      INT                                                           NOT NULL,
    [PersonType]            NCHAR (2)                                                     NOT NULL,
    [NameStyle]             [dbo].[NameStyle]                                             CONSTRAINT [DF_Person_NameStyle] DEFAULT ((0)) NOT NULL,
    [Title]                 NVARCHAR (8)                                                  NULL,
    [FirstName]             [dbo].[Name]                                                  NOT NULL,
    [MiddleName]            [dbo].[Name]                                                  NULL,
    [LastName]              [dbo].[Name]                                                  NOT NULL,
    [Suffix]                NVARCHAR (10)                                                 NULL,
    [EmailPromotion]        INT                                                           CONSTRAINT [DF_Person_EmailPromotion] DEFAULT ((0)) NOT NULL,
    [AdditionalContactInfo] XML(CONTENT [Person].[AdditionalContactInfoSchemaCollection]) NULL,
    [Demographics]          XML(CONTENT [Person].[IndividualSurveySchemaCollection])      NULL,
    [rowguid]               UNIQUEIDENTIFIER                                              CONSTRAINT [DF_Person_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]          DATETIME                                                      CONSTRAINT [DF_Person_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Person_BusinessEntityID] PRIMARY KEY CLUSTERED ([BusinessEntityID] ASC),
    CONSTRAINT [CK_Person_EmailPromotion] CHECK ([EmailPromotion]>=(0) AND [EmailPromotion]<=(2)),
    CONSTRAINT [CK_Person_PersonType] CHECK ([PersonType] IS NULL OR (upper([PersonType])='GC' OR upper([PersonType])='SP' OR upper([PersonType])='EM' OR upper([PersonType])='IN' OR upper([PersonType])='VC' OR upper([PersonType])='SC')),
    CONSTRAINT [FK_Person_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'INDEX', @level2name = N'AK_Person_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Person_rowguid]
    ON [Person].[Person]([rowguid] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_Person_LastName_FirstName_MiddleName]
    ON [Person].[Person]([LastName] ASC, [FirstName] ASC, [MiddleName] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary XML index.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'INDEX', @level2name = N'PXML_Person_AddContact';


GO

CREATE PRIMARY XML INDEX [PXML_Person_AddContact]
    ON [Person].[Person]([AdditionalContactInfo])
    WITH (PAD_INDEX = OFF);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Secondary XML index for property.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'INDEX', @level2name = N'XMLPROPERTY_Person_Demographics';


GO

CREATE XML INDEX [XMLPROPERTY_Person_Demographics]
    ON [Person].[Person]([Demographics])
    USING XML INDEX [PXML_Person_Demographics] FOR PROPERTY
    WITH (PAD_INDEX = OFF);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Secondary XML index for path.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'INDEX', @level2name = N'XMLPATH_Person_Demographics';


GO

CREATE XML INDEX [XMLPATH_Person_Demographics]
    ON [Person].[Person]([Demographics])
    USING XML INDEX [PXML_Person_Demographics] FOR PATH
    WITH (PAD_INDEX = OFF);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Secondary XML index for value.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'INDEX', @level2name = N'XMLVALUE_Person_Demographics';


GO

CREATE XML INDEX [XMLVALUE_Person_Demographics]
    ON [Person].[Person]([Demographics])
    USING XML INDEX [PXML_Person_Demographics] FOR VALUE
    WITH (PAD_INDEX = OFF);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary XML index.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'INDEX', @level2name = N'PXML_Person_Demographics';


GO

CREATE PRIMARY XML INDEX [PXML_Person_Demographics]
    ON [Person].[Person]([Demographics])
    WITH (PAD_INDEX = OFF);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'AFTER INSERT, UPDATE trigger inserting Individual only if the Customer does not exist in the Store table and setting the ModifiedDate column in the Person table to the current date.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'TRIGGER', @level2name = N'iuPerson';


GO


CREATE TRIGGER [Person].[iuPerson] ON [Person].[Person] 
AFTER INSERT, UPDATE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    IF UPDATE([BusinessEntityID]) OR UPDATE([Demographics]) 
    BEGIN
        UPDATE [Person].[Person] 
        SET [Person].[Person].[Demographics] = N'<IndividualSurvey xmlns="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"> 
            <TotalPurchaseYTD>0.00</TotalPurchaseYTD> 
            </IndividualSurvey>' 
        FROM inserted 
        WHERE [Person].[Person].[BusinessEntityID] = inserted.[BusinessEntityID] 
            AND inserted.[Demographics] IS NULL;
        
        UPDATE [Person].[Person] 
        SET [Demographics].modify(N'declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
            insert <TotalPurchaseYTD>0.00</TotalPurchaseYTD> 
            as first 
            into (/IndividualSurvey)[1]') 
        FROM inserted 
        WHERE [Person].[Person].[BusinessEntityID] = inserted.[BusinessEntityID] 
            AND inserted.[Demographics] IS NOT NULL 
            AND inserted.[Demographics].exist(N'declare default element namespace 
                "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
                /IndividualSurvey/TotalPurchaseYTD') <> 1;
    END;
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'DF_Person_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary type of person: SC = Store Contact, IN = Individual (retail) customer, SP = Sales person, EM = Employee (non-sales), VC = Vendor contact, GC = General contact', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'PersonType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Personal information such as hobbies, and income collected from online shoppers. Used for sales analysis.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'Demographics';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [EmailPromotion] >= (0) AND [EmailPromotion] <= (2)', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'CK_Person_EmailPromotion';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'NameStyle';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'PK_Person_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'A courtesy title. For example, Mr. or Ms.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'Title';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'DF_Person_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'First name of the person.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'FirstName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing BusinessEntity.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'FK_Person_BusinessEntity_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Middle name or middle initial of the person.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'MiddleName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'DF_Person_NameStyle';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Last name of the person.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'LastName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for Person records.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Contact does not wish to receive e-mail promotions, 1 = Contact does wish to receive e-mail promotions from AdventureWorks, 2 = Contact does wish to receive e-mail promotions from AdventureWorks and selected partners. ', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'EmailPromotion';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [PersonType] is one of SC, VC, IN, EM or SP.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'CK_Person_PersonType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Surname suffix. For example, Sr. or Jr.', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'Suffix';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Additional contact information about the person stored in xml format. ', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'COLUMN', @level2name = N'AdditionalContactInfo';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'Person', @level1type = N'TABLE', @level1name = N'Person', @level2type = N'CONSTRAINT', @level2name = N'DF_Person_EmailPromotion';


GO

