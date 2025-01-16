
Create table dbo.EqualsNull
(
    ID1 integer not null
    
)
go

Create Procedure dbo.EqualsNullTest
AS
BEGIN
SET NOCOUNT ON
	Select ID1 from dbo.EqualsNull where ID1 =NULL
END

--SML046
