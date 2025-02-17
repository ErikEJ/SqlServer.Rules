
Create table dbo.UnamedPK
(
    ID1 integer check (ID1>0),
	ID2 integer,
	unique (ID2,ID1)
	
    
)
go
