USE wagapp2_2021_Marion

--Drop Table if exists AbMarionOperators

Create Table AbMarionOperators
(
OperatorName VarChar(50),
CompanyNameSub VarChar(50),
CompanyName VarChar(50),
CompanyID int,
OperatorFlag bit,
Active bit
)

Insert Into AbMarionOperators
SELECT distinct a.[OperatorName] as OperatorName
				, substring(n.NameC, 1, 10) as CompanyNameSub
                , n.NameC as CompanyName 
				, n.NameID as CompanyID 
				, n.Oper_YN as OperatorFlag
				, n.Stat_YN as Active				
                    FROM[wagapp2_2021_Marion].[dbo].[AbMarionImport] a 
                    Left join tblName n on substring(a.OperatorName, 1, 10) 
                    like substring(n.NameC, 1, 10) 
                    where substring(a.SPTBCode, 1, 2) = 'G1' 
                    order by a.OperatorName 