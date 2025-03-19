create table foo
( 
	ID VARCHAR(64) not null
	, Name VARCHAR(128) NOT NULL
	, Valu VARCHAR(256) NOT NULL
);
GO

CREATE TABLE bar
( 
	ID varchar(64)
);
GO

CREATE PROCEDURE hip
AS
SELECT foo FROM foo 
		WHERE foo = 'foo' and
		foo != 'foo'
GO

CREATE TABLE [dbo].[xxx]
(
	[Id] [uniqueidentifier] NOT NULL,
    [CASEID] [varchar](44) NOT NULL,
	[PROCES] [varchar](3) NULL,
    CONSTRAINT [PK_Case] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE VIEW dbo.CaseView
AS
SELECT
     [CASEID] AS [Id]
	, CASE [STATUS]
		WHEN 0 THEN 'X'
	  END AS [Status]
FROM [dbo].[Case]

-- SRD0067
