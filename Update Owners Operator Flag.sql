update wagapp2_2021_Marion.dbo.tblName 
set tblName.Oper_YN = o.CompanyID 
from wagapp2_2021_Marion.dbo.AbMarionOperators o
inner join wagapp2_2021_Marion.dbo.tblName n
on o.CompanyID = n.NameID
where o.Active=1