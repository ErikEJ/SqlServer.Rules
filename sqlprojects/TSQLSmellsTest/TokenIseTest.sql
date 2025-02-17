CREATE PROCEDURE [dbo].[TokenIseTest]
	
AS
Set nocount on
	Select IDCOL from [$(TestServer)].dbo.Mytable


	SELECT IDCOL
	 from [ProdServer].[ProdDb].[Sales].[Products]

	SELECT IDCOL
	 from [IntServer].[IntDb].[Sales].[Products]


	 

RETURN 0
