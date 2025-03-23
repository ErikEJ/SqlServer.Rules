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
		foo != 'foo';
;
GO

-- SRD0067
