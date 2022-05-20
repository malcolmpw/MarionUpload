select CadID, LpdID, LpdLeaseName,RrcDist, RrcLease
from tblWell
where RrcLease='15286'

select CadID, LpdID, LpdLeaseName,RrcDist, RrcLease
from tblWell
where RrcLease='14680'

select CadID, LpdID, LpdLeaseName,RrcDist, RrcLease
from tblWell
where RrcLease='15586'

select CadID, LpdID, LpdLeaseName,RrcDist, RrcLease
from tblWell
where RrcLease='15728'

SELECT * FROM (
SELECT CadID, LpdID, RrcDist, RrcLease,
Row_Number() OVER(PARTITION BY RrcLease ORDER By CadID) as RrcLeaseCount
FROM tblWell
) AS w WHERE w.RrcLeaseCount > 1