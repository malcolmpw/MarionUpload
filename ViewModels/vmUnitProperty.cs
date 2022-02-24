using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;


//--To populate tblUnitProperty

//-- 0.  Loop through the import file with just LeaseNumber and Jurisdictions
//-- 1.  Get the LeaseNumber and all the Jurisdictions fRom the Current row
//-- 2.  Get the property id from the map in vmProperty using the LeaseNumber
//--3.Use the tlkpCadUnit table to get the UnitID from each jurisdiction against the CadUnitIDText
//-- 4.  Insert 1 jurisdiction entry at a time so e.g.   435355 (= property id) ,  SAVG if Jurisdiction is 81


///****** Script for SelectTopNRows command from SSMS  ******/
//SELECT TOP(1000) [CadID]
//      ,[UnitID]
//      ,[CadUnitIDNmbr]
//      ,[CadUnitIDText]
//      ,[CadAppraised]
//      ,[GlobalUnit]
//      ,[FirstUnit]
//      ,[UnitName]
//      ,[TaxRateEffRate]
//      ,[active]
//FROM[wagapp2_2021_Marion].[dbo].[tlkpCadUnit] where CADID = 'MAR' and GLOBAlUnit = 0




namespace MarionUpload.ViewModels
{
    class vmUnitProperty
    {
    }
}
