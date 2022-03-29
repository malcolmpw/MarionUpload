SELECT distinct a.[OperatorName], n.NameC, n.NameID, n.Oper_YN	  
  FROM [wagapp2_2021_Marion].[dbo].[AbMarionImport] a
  Left join tblName n on substring(a.OperatorName,1,10) like substring(n.NameC,1,10) 
  where substring(a.SPTBCode,1,2)='G1' 
  order by a.OperatorName