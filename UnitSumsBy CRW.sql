TRANSFORM Sum(Round([valacctcur]*[unitpct],0)) AS CalculatedUnitValue

SELECT tblProperty.PtdClassSub

FROM tblName INNER JOIN ((tblProperty LEFT JOIN tblUnitProperty ON tblProperty.PropId = tblUnitProperty.PropID) 
INNER JOIN tblAccount ON tblProperty.PropId = tblAccount.PropID) ON tblName.NameID = tblAccount.NameID

WHERE (((tblProperty.ControlCad)="ken") 
AND ((tblAccount.ValAcctCur)>=0) 
AND ((tblAccount.Stat_YN)=True) 
AND ((tblAccount.Supp_YN)=False))

GROUP BY tblProperty.PtdClassSub

PIVOT tblUnitProperty.UnitID;