
Create Table dbo.Parent
(
ParentId integer primary key,
SomeData  char(1)
)
go
Create Table dbo.Child
(
ChildID integer primary key,
ParentId integer constraint fkParentChild references dbo.Parent(ParentID),
SomeData  char(1)
)
go
alter table dbo.Child nocheck constraint fkParentChild

-- SML006, SRD0004
