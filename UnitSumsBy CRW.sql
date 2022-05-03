--MS ACCESS VERSION (FROM CRW)

--TRANSFORM Sum(Round([valacctcur]*[unitpct],0)) AS CalculatedUnitValue

--SELECT tblProperty.PtdClassSub

--FROM tblName INNER JOIN ((tblProperty LEFT JOIN tblUnitProperty ON tblProperty.PropId = tblUnitProperty.PropID) 
--INNER JOIN tblAccount ON tblProperty.PropId = tblAccount.PropID) ON tblName.NameID = tblAccount.NameID

--WHERE (((tblProperty.ControlCad)="ken") 
--AND ((tblAccount.ValAcctCur)>=0) 
--AND ((tblAccount.Stat_YN)=True) 
--AND ((tblAccount.Supp_YN)=False))

--GROUP BY tblProperty.PtdClassSub

--PIVOT tblUnitProperty.UnitID;



--SQL SERVER (TSQL) VERSION: (converted by MPW - I hope it is correct)

SELECT * FROM
(
    SELECT tblUnitProperty.UnitID as UnitID ,tblProperty.PtdClassSub as PtdClassSub,Round(tblAccount.ValAcctCur*tblUnitProperty.UnitPct,0) as UnitValue

    FROM tblName INNER JOIN ((tblProperty LEFT JOIN tblUnitProperty ON tblProperty.PropId = tblUnitProperty.PropID) 
    INNER JOIN tblAccount ON tblProperty.PropId = tblAccount.PropID) ON tblName.NameID = tblAccount.NameID

    WHERE (((tblProperty.ControlCad)='mar') 
    AND ((tblAccount.ValAcctCur)>=0) 
    AND ((tblAccount.Stat_YN)=1) 
    AND ((tblAccount.Supp_YN)=0))
) 
as SourceQuery

PIVOT
(
    Sum(UnitValue) 
    FOR UnitID
    IN (GMAR,HMAR,RMAR,SJEF,CJEF)
) 
as PivotTable


