  select n.NameC,n.Mail1,n.MailCi,a.NameSort,n.Oper_YN,n.MailCi
  from tblName n
  join AbMarionOperatorsFromCRW a on n.NameSortCad=a.NameSort 
  --where n.Oper_YN=1
  order by n.NameSortCad