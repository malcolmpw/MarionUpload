/****** Script for SelectTopNRows command from SSMS  ******/
SELECT distinct 
	   n.NameID
      ,a.Cad
	  ,c.CadOwnerID
      ,[NameSel_YN]
      ,[NameSortCad]
  FROM [WagData2017].[dbo].[tblName] n 
  inner join tblAccount a 
  on n.NameID=a.NameID 
  inner join tblCadOwners c
  on a.NameID=c.NameID 
  where a.cad='MAR' and n.NameSel_YN=1
  --order by n.NameC