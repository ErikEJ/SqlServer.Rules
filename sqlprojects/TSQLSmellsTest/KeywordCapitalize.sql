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

CREATE TABLE [dbo].[Case]
(
	[Id] [uniqueidentifier] NOT NULL,
    [CASEID] [varchar](44) NOT NULL,
	[PROCES] [varchar](3) NULL,
    CONSTRAINT [PK_Case] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group]([Id])
)
GO

CREATE TABLE [HangFire].[Set](
	[Key] NVARCHAR(100) NOT NULL,
	[Score] FLOAT NOT NULL,
	[Value] NVARCHAR(256) NOT NULL,
    CONSTRAINT [PK_Set] PRIMARY KEY CLUSTERED ([Key] ASC, [Value] ASC)
       WITH (IGNORE_DUP_KEY = ON)
)
GO


-- SRD0067
