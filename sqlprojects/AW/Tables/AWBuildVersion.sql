CREATE TABLE [dbo].[AWBuildVersion] (
    [SystemInformationID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Database Version]    NVARCHAR (25) NOT NULL,
    [VersionDate]         DATETIME      NOT NULL,
    [ModifiedDate]        DATETIME      CONSTRAINT [DF_AWBuildVersion_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_AWBuildVersion_SystemInformationID] PRIMARY KEY CLUSTERED ([SystemInformationID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion', @level2type = N'COLUMN', @level2name = N'VersionDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion', @level2type = N'CONSTRAINT', @level2name = N'DF_AWBuildVersion_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Version number of the database in 9.yy.mm.dd.00 format.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion', @level2type = N'COLUMN', @level2name = N'Database Version';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for AWBuildVersion records.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion', @level2type = N'COLUMN', @level2name = N'SystemInformationID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Current version number of the AdventureWorks2008R2 sample database. ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AWBuildVersion', @level2type = N'CONSTRAINT', @level2name = N'PK_AWBuildVersion_SystemInformationID';


GO

