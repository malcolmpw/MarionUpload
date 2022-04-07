using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MarionUpload.Comparers;
using MarionUpload.Helpers;
using MarionUpload.Messages;
using MarionUpload.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;


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

        public ICommand CommandImportUnitProperty => new RelayCommand(OnImportUnitProperty);
        public ICommand CommandUploadUnitProperty => new RelayCommand(OnUploadUnitProperty);

        public bool UnitImportEnabled { get => _unitImportEnabled; set { _unitImportEnabled = value; RaisePropertyChanged(nameof(UnitImportEnabled)); } }
        public bool UnitUploadEnabled { get => _unitUploadEnabled; set { _unitUploadEnabled = value; RaisePropertyChanged(nameof(UnitUploadEnabled)); } }



        private void OnImportUnitProperty()
        {

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {

                var mineralResults = db.Query<mMarionMineralProperty>(
                "Select distinct LeaseNumber, PropertyType, SPTBCode, Description1, Description2, LeaseName, RRC, OperatorName," +
                "Jurisdiction1, Jurisdiction2, Jurisdiction3, Jurisdiction4, Jurisdiction5, Jurisdiction6, " +
                "Jurisdiction7, Jurisdiction8, Jurisdiction9, Jurisdiction10, Jurisdiction11, Jurisdiction12 " +
                "from AbMarionImport where SPTBCode = 'G1 ' or SPTBCode = 'XV ' ");

                var distinctMineralResults = mineralResults.Distinct(new MineralPropertyComparer()).ToList();

                distinctMineralResults.ForEach(mineralProperty => MarionMineralProperties.Add(mineralProperty));

                var personalResults = db.Query<mMarionPersonalProperty>(
                "Select distinct OwnerNumber, PropertyType, SPTBCode, Description1, Description2, LeaseName, RRC, OperatorName," +
                "Jurisdiction1, Jurisdiction2, Jurisdiction3, Jurisdiction4, Jurisdiction5, Jurisdiction6, " +
                "Jurisdiction7, Jurisdiction8, Jurisdiction9, Jurisdiction10, Jurisdiction11, Jurisdiction12 " +
                "from AbMarionImport where SPTBCode <> 'G1 ' and SPTBCode <> 'XV ' ");

                var distinctPersonalResults = mineralResults.Distinct(new MineralPropertyComparer()).ToList();

                distinctPersonalResults.ForEach(personalProperty => MarionMineralProperties.Add(personalProperty));

                UnitImportEnabled = false;
                UnitUploadEnabled = true;
            }
        }

        public Dictionary<string, mCadUnit> CadUnitIDMap { get; private set; }
        public ObservableCollection<mMarionMineralProperty> MarionMineralProperties { get; private set; } = new ObservableCollection<mMarionMineralProperty>();
        public ObservableCollection<mMarionPersonalProperty> MarionPersonalProperties { get; private set; } = new ObservableCollection<mMarionPersonalProperty>();

        private void OnUploadUnitProperty()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {

                var unitLookup = db.Query<mCadUnit>("Select [CadID], [UnitID], [CadUnitIDText], [CadAppraised], active " +
                                                    "From tlkpCadUnit where CadID = 'MAR' and CadAppraised = 1");

                CadUnitIDMap = unitLookup.ToDictionary(key => key.CadUnitIDText.Trim(), val => val);

                foreach (var mineralProperty in MarionMineralProperties)
                {
                    var jurisdictions = new List<int>
                    {
                        mineralProperty.Jurisdiction1, mineralProperty.Jurisdiction2, mineralProperty.Jurisdiction3,
                        mineralProperty.Jurisdiction4, mineralProperty.Jurisdiction5, mineralProperty.Jurisdiction6,
                        mineralProperty.Jurisdiction7, mineralProperty.Jurisdiction8, mineralProperty.Jurisdiction9,
                        mineralProperty.Jurisdiction10, mineralProperty.Jurisdiction11, mineralProperty.Jurisdiction12
                    };

                    foreach (var jurisdiction in jurisdictions)
                    {
                        if (jurisdiction == 0) continue;
                        if (!CadUnitIDMap.ContainsKey(jurisdiction.ToString()))
                        {
                            Log.Error($"Jurisdiction #{jurisdiction} does not exist in tlkpCadUnit ");
                            //   MessageBox.Show($"Jurisdiction #{jurisdiction} does not exist in tlkpCadUnit");
                            continue;
                        }
                        var unitProperty = TranslateMineralImportPropertyToUnitProperty(mineralProperty, jurisdiction);
                        db.Insert<mUnitProperty>(unitProperty);
                    }
                }

                foreach (var personalProperty in MarionPersonalProperties)
                {
                    var jurisdictions = new List<int>
                    {
                        personalProperty.Jurisdiction1, personalProperty.Jurisdiction2, personalProperty.Jurisdiction3,
                        personalProperty.Jurisdiction4, personalProperty.Jurisdiction5, personalProperty.Jurisdiction6,
                        personalProperty.Jurisdiction7, personalProperty.Jurisdiction8, personalProperty.Jurisdiction9,
                        personalProperty.Jurisdiction10, personalProperty.Jurisdiction11, personalProperty.Jurisdiction12
                    };

                    foreach (var jurisdiction in jurisdictions)
                    {
                        if (jurisdiction == 0) continue;
                        if (!CadUnitIDMap.ContainsKey(jurisdiction.ToString()))
                        {
                            Log.Error($"Jurisdiction #{jurisdiction} does not exist in tlkpCadUnit ");
                            //   MessageBox.Show($"Jurisdiction #{jurisdiction} does not exist in tlkpCadUnit");
                            continue;
                        }
                        var unitProperty = TranslatePersonalImportPropertyToUnitProperty(personalProperty, jurisdiction);
                        db.Insert<mUnitProperty>(unitProperty);
                    }
                }

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                MessageBox.Show($"Finished uploading {MarionMineralProperties.Count()} unit properties");
                Messenger.Default.Send<UnitsFinishedMessage>(new UnitsFinishedMessage());

            }
        }

        private mUnitProperty TranslateMineralImportPropertyToUnitProperty(mMarionMineralProperty property, int jurisdiction)
        {
            mUnitProperty unitProperty = new mUnitProperty();
            unitProperty.PropID = (int)vmProperty.MineralPropertyIdMap[property.LeaseNumber];
            unitProperty.UnitID = CadUnitIDMap[jurisdiction.ToString()].UnitID;
            unitProperty.UnitPct = 1;
            return unitProperty;
        }

        private mUnitProperty TranslatePersonalImportPropertyToUnitProperty(mMarionPersonalProperty property, int jurisdiction)
        {
            mUnitProperty unitProperty = new mUnitProperty();
            unitProperty.PropID = (int)vmProperty.PersonalPropertyIdMap[property.OwnerNumber];
            unitProperty.UnitID = CadUnitIDMap[jurisdiction.ToString()].UnitID;
            unitProperty.UnitPct = 1;
            return unitProperty;
        }
    }
}
