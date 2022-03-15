SELECT a.AcctID
      ,a.PropID
      ,a.NameID
      ,[AcctLegal]
      ,[PctProp]
      ,[PctType]
      ,[PTDcode]
      ,[GeoRef]    
	  ,c.CadAcctID
	  ,k.CadOwnerID
	  ,p.CadPropId
  FROM [WagData2017].[dbo].[tblAccount] a
  inner join tblCadAccount c on a.AcctID=c.AcctID
  inner join tblCadProperty p on a.PropID=p.PropID
  inner join tblName n on n.nameid=a.NameID
  inner join tblCadOwners k on k.NameID=n.NameID 
  where k.CadOnerID=704839
  --where n.nameid=138136
  --where cad='MAR'
  --and a.pcttype <> 'R' 
  --and a.pcttype<>'O' 
  --and a.pcttype<>'W' or a.pcttype='U'
   
  --NOTE!:
 -- I should check if any CadOwnerID and CadPropID in 2017 also exist in AbMarionImport......DONE!  example Norbord

 -- YES it is the same CadOwnerID 704839 in 2021 and in 2017 