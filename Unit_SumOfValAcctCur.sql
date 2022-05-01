select u.UnitID,sum(a.valAcctCur) as sumOfValAcctCur
from tblUnitProperty u 
join tblAccount a on u.propID=a.PropID 
where a.Cad='MAR' 
group by u.UnitID