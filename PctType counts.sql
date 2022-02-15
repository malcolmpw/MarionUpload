SELECT 
--TOP (1000) 
	   count(*) 
  FROM [WagData2015].[dbo].[tblName] n
  inner join tblAccount a
  on n.NameID=a.NameID
  inner join tblProperty p
  on a.PropID=p.PropId
  where p.ControlCad='MAR'
  --and (a.PctType<>'W' and a.PctType<>'R' and PctType<>'O' and PctType<>'U')



  --and n.NameSel_YN=0
  --order by a.PctType,n.NameSel_YN

  -- results:
  -- PctType,	NameSel_YN,	count(*)
  -- 'U'	,   1		  ,   545 all				  
  -- 'U'	,   0		  ,     0				  
  -- 'U'	,   1 or 0	  ,   545
  
  -- 'O'	,   1		  ,   691				  
  -- 'O'	,   0		  ,  1160 majority				  
  -- 'O'	,   1 or 0	  ,  1851			  

  -- 'R'	,   1		  ,  3850 				  
  -- 'R'	,   0		  , 16328 majority  				  
  -- 'R'	,   1 or 0	  , 20178  				  
  
  -- 'W'	,   1		  ,   407 majority		  
  -- 'W'	,   0		  ,    70				  
  -- 'W'	,   1 or 0	  ,   477				  

  -- Note! All records have one of these PctTypes

  -- CONCLUSION: For all PctType='U' assume NameSel_YN=1
  --             For all others you don't know, so try to check WagData2015
