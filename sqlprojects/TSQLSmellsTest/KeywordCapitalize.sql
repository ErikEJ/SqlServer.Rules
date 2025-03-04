create table foo
( 
	ID VARCHAR(64) not null
	, Name VARCHAR(128) NOT NULL
	, Value VARCHAR(256) NOT NULL
);
GO

CREATE TABLE bar
( 
	ID varchar(64)
);
GO

SELECT foo FROM foo 
		WHERE foo = 'foo' and
		foo != 'foo'

-- SRD0067
