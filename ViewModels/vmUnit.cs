using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Input;
using Dapper;
using GalaSoft.MvvmLight.Command;
using MarionUpload.Helpers;
using MarionUpload.Models;
using System.Collections.ObjectModel;
using Dapper.Contrib.Extensions;
using System.Windows;
using MarionUpload.Messages;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using log4net;
using MarionUpload.Comparers;


//--To populate tblUnitProperty

//-- 0.  Loop through the import file with just LeaseNumber and Jurisdictions
//-- 1.  Get the LeaseNumber and all the Jurisdictions fRom the Current row
//-- 2.  Get the property id from the map in vmProperty using the LeaseNumber
//-- 3.  Use the tlkpCadUnit table to get the UnitID from each jurisdiction against the CadUnitIDText
//-- 4.  Insert 1 jurisdiction entry at a time so e.g.   435355 (= property id) ,  SAVG if Jurisdiction is 81 into tblUnitProperty

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
//FROM[wagapp2_2021_Marion].[dbo].[tlkpCadUnit] where CADID = 'MAR' 




namespace MarionUpload.ViewModels
{
    public class vmUnit : ViewModelBase
    {

        private static readonly ILog Log =
                    LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _unitImportEnabled = true;
        private bool _unitUploadEnabled = false;

        public ICommand CommandUploadUnitProperty => new RelayCommand(OnUploadUnitProperty);
        public ICommand CommandImportUnitProperty => new RelayCommand(OnImportUnitProperty);

        public bool UnitImportEnabled { get => _unitImportEnabled; set { _unitImportEnabled = value; RaisePropertyChanged(nameof(UnitImportEnabled)); } } 
        public bool UnitUploadEnabled { get => _unitUploadEnabled; set {_unitUploadEnabled = value; RaisePropertyChanged(nameof(UnitUploadEnabled)); } }



        private void OnImportUnitProperty()
        {

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {

                var results = db.Query<mMarionProperty>("Select distinct LeaseNumber, PropertyType,SPTBCode,Description1,Description2,LeaseName,RRC,OperatorName," +

                "Jurisdiction1, Jurisdiction2, Jurisdiction3, " +

                "Jurisdiction4, Jurisdiction5, Jurisdiction6, " +

                "Jurisdiction7, Jurisdiction8, Jurisdiction9, " +

                "Jurisdiction10, Jurisdiction11, Jurisdiction12" +

                " from AbMarionImport");

                var distinctResults = results.Distinct(new PropertyComparer()).ToList();

                distinctResults.ForEach(property => MarionProperties.Add(property));

                UnitImportEnabled = false;
                UnitUploadEnabled = true;
            }
        }

        public Dictionary<string, mCadUnit> CadUnitIDMap { get; private set; }
        public ObservableCollection<mMarionProperty> MarionProperties { get; private set; } = new ObservableCollection<mMarionProperty>();

        private void OnUploadUnitProperty()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {

                var unitLookup = db.Query<mCadUnit>("Select [CadID], [UnitID], [CadUnitIDText], [CadAppraised], active " +
                                                    "From tlkpCadUnit where CadID = 'MAR' and CadAppraised = 1");

                CadUnitIDMap = unitLookup.ToDictionary(key => key.CadUnitIDText.Trim(), val => val);

                foreach (var property in MarionProperties)
                {
                    var jurisdictions = new List<int>
                    {
                        property.Jurisdiction1, property.Jurisdiction2, property.Jurisdiction3,
                        property.Jurisdiction4, property.Jurisdiction5, property.Jurisdiction6,
                        property.Jurisdiction7, property.Jurisdiction8, property.Jurisdiction9,
                        property.Jurisdiction10, property.Jurisdiction11, property.Jurisdiction12
                    };

                    foreach (var jurisdiction in jurisdictions)
                    {
                        if (jurisdiction == 0) continue;
                        if (!CadUnitIDMap.ContainsKey(jurisdiction.ToString()))
                        {
                            Log.Error($"Jurisdiction #{jurisdiction} does not exist in tlkpCadUnit");
                         //   MessageBox.Show($"Jurisdiction #{jurisdiction} does not exist in tlkpCadUnit");
                            continue;
                        }
                        var unitProperty = TranslateImportPropertyToUnitProperty(property, jurisdiction);
                        db.Insert<mUnitProperty>(unitProperty);
                    }

                }

                MessageBox.Show($"Finished uploading {MarionProperties.Count()} unit properties");
                Messenger.Default.Send<UnitsFinishedMessage>(new UnitsFinishedMessage());

            }
        }

        private mUnitProperty TranslateImportPropertyToUnitProperty(mMarionProperty property, int jurisdiction)
        {
            mUnitProperty unitProperty = new mUnitProperty();
            unitProperty.PropID = (int)vmProperty.PropertyIdMap[property.LeaseNumber];
            unitProperty.UnitID = CadUnitIDMap[jurisdiction.ToString()].UnitID;
            unitProperty.UnitPct = 1;
            return unitProperty;
        }
    }
}
