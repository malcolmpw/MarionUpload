/****** Script for SelectTopNRows command from SSMS  ******/
SELECT distinct 
	   n.NameID
      ,a.Cad
	  ,c.CadOwnerID
      ,[OwnerID]
      ,[NameH]
      ,[NameF]
      ,[NameM]
      ,[NameLP]
      ,[NameL1]
      ,[NameL2]
      ,[NameLS]
      ,[NameT]
      ,[NameC]
      ,[NameCP]
      ,[NameSel_YN]
      ,[Name2]
      ,[NameSort]
      ,[NameSortFirst]
      ,[NameSortCad]
      ,[NameNick]
  FROM [WagData2015].[dbo].[tblName] n 
  inner join tblAccount a 
  on n.NameID=a.NameID 
  inner join tblCadOwners c
  on a.NameID=c.NameID 
  where a.cad='MAR' and n.NameSel_YN=1
  order by n.NameC