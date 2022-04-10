using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MarionUpload.Comparers;
using MarionUpload.Helpers;
using MarionUpload.Interfaces;
using MarionUpload.Messages;
using MarionUpload.Models;
using MarionUpLoad.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MarionUpload.ViewModels
{
    public class vmProperty : ViewModelBase
    {
        private bool _propertyImportEnabled = true;
        private bool _propertyUploadEnabled = false;

        public ObservableCollection<mMarionMineralProperty> MarionMineralProperties { get; set; }
        public ObservableCollection<mMarionPersonalProperty> MarionPersonalProperties { get; set; }
        //public static ObservableCollection<mMarionOwner> MarionOwners { get; set; }

        //public int ProgressBarUpLoadMarionMineralPropertiesMinimumValue
        //{
        //    get => ProgressBarUpLoadMarionMineralPropertiesMinimumValue;
        //    private set
        //    {
        //        ProgressBarUpLoadMarionMineralPropertiesMinimumValue = value;
        //        RaisePropertyChanged(nameof(ProgressBarUpLoadMarionMineralPropertiesMinimumValue));
        //    }
        //}
        //public int ProgressBarUpLoadMarionMineralPropertiesMaximumValue
        //{
        //    get => ProgressBarUpLoadMarionMineralPropertiesMaximumValue;
        //    private set
        //    {
        //        ProgressBarUpLoadMarionMineralPropertiesMaximumValue = value;
        //        RaisePropertyChanged(nameof(ProgressBarUpLoadMarionMineralPropertiesMaximumValue));
        //    }
        //}
        //public int ProgressBarUpLoadMarionMineralPropertiesCurrentValue
        //{
        //    get => ProgressBarUpLoadMarionMineralPropertiesCurrentValue;
        //    private set
        //    {
        //        ProgressBarUpLoadMarionMineralPropertiesCurrentValue = value;
        //        RaisePropertyChanged(nameof(ProgressBarUpLoadMarionMineralPropertiesCurrentValue));
        //    }
        //}

        public int ProgressBarUpLoadMarionPersonalPropertiesMinimumValue
        {
            get => ProgressBarUpLoadMarionPersonalPropertiesMinimumValue;
            private set
            {
                ProgressBarUpLoadMarionPersonalPropertiesMinimumValue = value;
                RaisePropertyChanged(nameof(ProgressBarUpLoadMarionPersonalPropertiesMinimumValue));
            }
        }
        public int ProgressBarUpLoadMarionPersonalPropertiesMaximumValue
        {
            get => ProgressBarUpLoadMarionPersonalPropertiesMaximumValue;
            private set
            {
                ProgressBarUpLoadMarionPersonalPropertiesMaximumValue = value;
                RaisePropertyChanged(nameof(ProgressBarUpLoadMarionPersonalPropertiesMaximumValue));
            }
        }
        public int ProgressBarUpLoadMarionPersonalPropertiesCurrentValue
        {
            get => ProgressBarUpLoadMarionPersonalPropertiesCurrentValue;
            private set
            {
                ProgressBarUpLoadMarionPersonalPropertiesCurrentValue = value;
                RaisePropertyChanged(nameof(ProgressBarUpLoadMarionPersonalPropertiesCurrentValue));
            }
        }

        public ICommand CommandImportProperties => new RelayCommand(OnImportProperties);
        public ICommand CommandUploadProperties => new RelayCommand(OnUploadProperties);

        public IDictionary<string, long> PropIdMap { get; private set; } = new Dictionary<string, long>();
        public Dictionary<int, string> JurisdictionMap { get; private set; }
        public Dictionary<string, string> PtdPropMap { get; private set; }

        public bool PropertyImportEnabled { get => _propertyImportEnabled; set { _propertyImportEnabled = value; RaisePropertyChanged(nameof(PropertyImportEnabled)); } }
        public bool PropertyUploadEnabled { get => _propertyUploadEnabled; set { _propertyUploadEnabled = value; RaisePropertyChanged(nameof(PropertyUploadEnabled)); } }

        public vmProperty()
        {
            MarionMineralProperties = new ObservableCollection<mMarionMineralProperty>();
            MarionPersonalProperties = new ObservableCollection<mMarionPersonalProperty>();
        }

        private void OnImportProperties()
        {
            SelectJurisdictionNamesfromAbMarionTlkpJurisdiction();
            SelectPropClassDataFromTlkpPtdPropClassSub();

            SelectMineralPropertyDataFromMarionImportTable();
            SelectPersonalPropertyDataFromMarionImportTable();
        }

        private void SelectJurisdictionNamesfromAbMarionTlkpJurisdiction()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                JurisdictionMap = db.Query<mJurisdiction>("Select Code,Name from abMariontlkpJurisdiction")
                    .ToDictionary(jurisdiction => jurisdiction.Code, val => val.Name);
            }
        }

        private void SelectPropClassDataFromTlkpPtdPropClassSub()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                PtdPropMap = db.Query<mPtdProp>("Select PropClassSub, PropClassDesc from tlkpPtdPropClassSub")
                    .ToDictionary(key => key.PropClassSub, val => val.PropClassDesc);
            }
        }

        private void SelectMineralPropertyDataFromMarionImportTable()
        {          
            MarionMineralProperties.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionMineralProperty>(
                    "Select LeaseNumber, PropertyType, SPTBCode, Description1, Description2, LeaseName,RRC,OperatorName, " +
                    "Jurisdiction1, Jurisdiction2, Jurisdiction3, Jurisdiction4, Jurisdiction5, Jurisdiction6, " +
                    "Jurisdiction7, Jurisdiction8, Jurisdiction9, Jurisdiction10, Jurisdiction11, Jurisdiction12, " +
                    "Juris1MarketValue from AbMarionImport where SPTBCode = 'G1 ' or SPTBCode = 'XV ' ");
                var resultList = results.Distinct(new MineralPropertyComparer()).ToList();
                resultList.ForEach(marionProperty => MarionMineralProperties.Add(marionProperty));

                PropertyImportEnabled = false;
                PropertyUploadEnabled = true;
            }
        }

        private void SelectPersonalPropertyDataFromMarionImportTable()
        {
            MarionPersonalProperties.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionPersonalProperty>(
                "Select OwnerNumber, PropertyType, SPTBCode, Description1, Description2, LeaseName,RRC,OperatorName, " +
                "Jurisdiction1, Jurisdiction2, Jurisdiction3, Jurisdiction4, Jurisdiction5, Jurisdiction6, " +
                "Jurisdiction7, Jurisdiction8, Jurisdiction9, Jurisdiction10, Jurisdiction11, Jurisdiction12, " +
                "Juris1MarketValue from AbMarionImport where SPTBCode <> 'G1 ' AND SPTBCode <> 'XV ' ");
                //MessageBox.Show($"sptb code = --{results.FirstOrDefault().SPTBCode}--");

                var resultList = results.Distinct(new PersonalPropertyComparer()).ToList();
                resultList.ForEach(marionProperty => MarionPersonalProperties.Add(marionProperty));

                PropertyImportEnabled = false;
                PropertyUploadEnabled = true;
            }
        }

        public static IDictionary<int, long> MineralPropertyIdMap { get; private set; } = new Dictionary<int, long>();
        public static IDictionary<Tuple<int, int>, long> PersonalPropertyIdMap { get; private set; } = new Dictionary<Tuple<int, int>, long>();
        public static IDictionary<int, string> PropertyLegalMap { get; private set; } = new Dictionary<int, string>();
        public IDictionary<int, long> CadPropertyIdMap { get; private set; } = new Dictionary<int, long>();

        private void OnUploadProperties()
        {
            //ProgressBarUpLoadMarionMineralPropertiesMinimumValue = 0;
            //ProgressBarUpLoadMarionMineralPropertiesMaximumValue = MarionMineralProperties.Count();
            //ProgressBarUpLoadMarionMineralPropertiesCurrentValue = 0;

            //ProgressBarUpLoadMarionPersonalPropertiesMinimumValue = 0;
            //ProgressBarUpLoadMarionPersonalPropertiesMaximumValue = MarionPersonalProperties.Count();
            //ProgressBarUpLoadMarionPersonalPropertiesCurrentValue = 0;

            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                UploadMineralProperties(db);
                UploadPersonalProperties(db);
            }
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

            MessageBox.Show($"Finished uploading {MarionMineralProperties.Count()} mineral properties and {MarionPersonalProperties.Count()} personal properties.");
            Messenger.Default.Send<PropertiesFinishedMessage>(new PropertiesFinishedMessage());
        }

        private void UploadPersonalProperties(IDbConnection db)
        {
            foreach (mMarionPersonalProperty marionPersonalProperty in MarionPersonalProperties)
            {
                var populatedProperty = TranslateFrom_mMarionPersonalPropertyTo_mProperty(marionPersonalProperty);
                var primaryPropertyKey = db.Insert<mProperty>(populatedProperty);
                Tuple<int, int> keyTuple = new Tuple<int, int>(marionPersonalProperty.OwnerNumber, marionPersonalProperty.LeaseNumber);
                if (!PersonalPropertyIdMap.ContainsKey(keyTuple))
                    PersonalPropertyIdMap.Add(keyTuple, primaryPropertyKey);
                if (!PropertyLegalMap.ContainsKey((int)primaryPropertyKey))
                    PropertyLegalMap.Add((int)primaryPropertyKey, populatedProperty.Legal);
                System.Diagnostics.Debug.WriteLine($"Primary Key: {primaryPropertyKey}");

                var populatedCadProperty = TranslateFrom_mMarionPersonalPropertyTo_mCadProperty(marionPersonalProperty, primaryPropertyKey);
                var primaryCadPropertyKey = db.Insert<mCadProperty>(populatedCadProperty);
                if (!CadPropertyIdMap.ContainsKey(marionPersonalProperty.LeaseNumber))
                    CadPropertyIdMap.Add(marionPersonalProperty.LeaseNumber, primaryPropertyKey);

                //Add a first segment to each property where SPTBCode <> 'G1' and SPTBCode <> 'XV'
                var populatedSegment = TranslateFrom_mPersonalPropertyTo_mSegment(populatedProperty, marionPersonalProperty);
                var primarySegmentKey = db.Insert<mSegment>(populatedSegment);
            }
        }

        private void UploadMineralProperties(IDbConnection db)
        {
            foreach (mMarionMineralProperty marionMineralProperty in MarionMineralProperties)
            {
                //++ProgressBarUpLoadMarionMineralPropertiesCurrentValue;                   
                var populatedProperty = TranslateFrom_mMarionMineralPropertyTo_mProperty(marionMineralProperty);
                var primaryPropertyKey = db.Insert<mProperty>(populatedProperty);
                if (!MineralPropertyIdMap.ContainsKey(marionMineralProperty.LeaseNumber))
                    MineralPropertyIdMap.Add(marionMineralProperty.LeaseNumber, primaryPropertyKey);
                if (!PropertyLegalMap.ContainsKey((int)primaryPropertyKey))
                    PropertyLegalMap.Add((int)primaryPropertyKey, populatedProperty.Legal);
                System.Diagnostics.Debug.WriteLine($"Primary Key: {primaryPropertyKey}");

                var populatedCadProperty = TranslateFrom_mMarionMineralPropertyTo_mCadProperty(marionMineralProperty, primaryPropertyKey);
                var primaryCadPropertyKey = db.Insert<mCadProperty>(populatedCadProperty);
                if (!CadPropertyIdMap.ContainsKey(marionMineralProperty.LeaseNumber))
                    CadPropertyIdMap.Add(marionMineralProperty.LeaseNumber, primaryPropertyKey);
            }
        }

        private mSegment TranslateFrom_mPersonalPropertyTo_mSegment(mProperty property, mMarionPersonalProperty marionProperty)
        {
            var oppSegment = new mSegment();

            oppSegment.PropID = property.PropId;
            oppSegment.PrsnlID = 1;
            //commented out per CRW Mar 29,2022 email
            //oppSegment.PrsnlDesc = "1st Segt: ";
            //oppSegment.PrsnlDesc += "(added: " + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ") ";
            //oppSegment.PrsnlDesc += property.Legal;
            oppSegment.PrsnlDesc = property.Legal;

            oppSegment.PrsnlCreateDate = DateTime.Now;
            oppSegment.PrsnlCreateBy = "MPW";
            oppSegment.PrsnlCreateWhy = "conversion";

            oppSegment.PrsnlStatDate = DateTime.Now;
            oppSegment.PrsnlStatBy = "MPW";
            oppSegment.PrsnlStatWhy = "conversion";
            oppSegment.PrsnlStat_YN = true;

            oppSegment.PrsnlUnitModifier = 1.0d;
            oppSegment.PrsnlUtilPct = 1.0d;
            oppSegment.PrsnlApprMethod = "Flat value";

            oppSegment.PrsnlPtdPropClass = property.PtdClass;
            oppSegment.EqptClassID = 136;//inventory
            oppSegment.DeprSchedID = 1;// from inventory

            oppSegment.PrsnlValCur = marionProperty.Juris1MarketValue;
            oppSegment.PrsnlValFlat = marionProperty.Juris1MarketValue;
            oppSegment.PrsnlValPrYr = marionProperty.Juris1MarketValue;

            oppSegment.delflag = false;

            return oppSegment;
        }


        private mCadProperty TranslateFrom_mMarionMineralPropertyTo_mCadProperty(mMarionMineralProperty marionProperty, long primaryPropertyKey)
        {
            var cadProperty = new mCadProperty();
            cadProperty.CadID = "MAR";
            cadProperty.CadPropid = marionProperty.LeaseNumber.ToString();
            cadProperty.CadPct = 1.0;
            cadProperty.PropID = (int)primaryPropertyKey;
            cadProperty.delflag = false;
            return cadProperty;
        }

        private mProperty TranslateFrom_mMarionMineralPropertyTo_mProperty(mMarionMineralProperty importedMarionProperty)
        {
            NameSorts nsorts = new NameSorts();
            var _updateDate = DateTime.Now;
            var _updateBy = "MPW";
            var _updateWhy = "MAR Import";

            var property = new mProperty();

            property.Stat_YN = true;
            property.PtdClassSub = importedMarionProperty.SPTBCode.Trim().Substring(0, 2);
            property.PropType = "M";
            string rrcNumber = GetRRCnumberFromImportRRCstring(importedMarionProperty);
            property.Legal = importedMarionProperty.LeaseName.Trim() +
                " (" + rrcNumber +
                "); Opr: " + importedMarionProperty.OperatorName.Trim();
            property.ControlCad = "MAR";
            property.UpdateWhy = _updateWhy;
            property.CreateBy = _updateBy;
            property.UpdateBy = _updateBy;
            property.UpdateDate = _updateDate;
            property.CreateWhy = _updateWhy;
            property.CreateDate = _updateDate;

            return property;
        }

        private mCadProperty TranslateFrom_mMarionPersonalPropertyTo_mCadProperty(mMarionPersonalProperty marionProperty, long primaryPropertyKey)
        {
            var cadProperty = new mCadProperty();
            cadProperty.CadID = "MAR";
            cadProperty.CadPropid = marionProperty.LeaseNumber.ToString();
            cadProperty.CadPct = 1.0;
            cadProperty.PropID = (int)primaryPropertyKey;
            cadProperty.delflag = false;
            return cadProperty;
        }

        private mProperty TranslateFrom_mMarionPersonalPropertyTo_mProperty(mMarionPersonalProperty importedMarionProperty)
        {
            NameSorts nsorts = new NameSorts();
            var _updateDate = DateTime.Now;
            var _updateBy = "MPW";
            var _updateWhy = "MAR Import";

            var property = new mProperty();

            property.Stat_YN = true;

            property.PtdClassSub = importedMarionProperty.SPTBCode.Trim().Substring(0, 2);

            if (property.PtdClassSub == "L1" || property.PtdClassSub == "L2")
            {
                property.PtdClass = property.PtdClassSub;
            }
            else
            {
                property.PtdClass = property.PtdClassSub.Substring(0, 1);
            }

            //property.PropType = "P";
            //property.Legal = FetchPTDDescription(sptbCode) + "," +
            //                 FetchISDJurisdictionName(importedMarionProperty);  //importedMarionProperty.Description2;
            property.Legal = (importedMarionProperty.Description1).Trim() + "-" +
                                (importedMarionProperty.Description2).Trim();
            property.Location = (importedMarionProperty.Description2).Trim();

            property.ControlCad = "MAR";

            property.UpdateWhy = _updateWhy;
            property.CreateBy = _updateBy;
            property.UpdateBy = _updateBy;
            property.UpdateDate = _updateDate;
            property.CreateWhy = _updateWhy;
            property.CreateDate = _updateDate;

            return property;
        }

        private static string GetRRCnumberFromImportRRCstring(IMarionWithRRC importedMarionProperty)
        {
            string rrc = importedMarionProperty.RRC.Trim();
            string pat = @"(\d+)";
            Regex re = new Regex(pat);
            var match = re.Match(rrc);
            var rrcNumber = "";
            if (match.Success)
            {
                rrcNumber = (match.Groups[0].Value).Trim();
            }

            return rrcNumber;
        }

        private string FetchPTDDescription(string sptbCode)
        {
            if (PtdPropMap.ContainsKey(sptbCode))
            {
                return PtdPropMap[sptbCode];
            }

            return "--";
        }

        private string FetchISDJurisdictionName(mMarionMineralProperty importedMarionProperty)
        {
            var jurisdictions = new List<int>
            {
                importedMarionProperty.Jurisdiction1,
                importedMarionProperty.Jurisdiction2,
                importedMarionProperty.Jurisdiction3,
                importedMarionProperty.Jurisdiction4,
                importedMarionProperty.Jurisdiction5,
                importedMarionProperty.Jurisdiction6,
                importedMarionProperty.Jurisdiction7,
                importedMarionProperty.Jurisdiction8,
                importedMarionProperty.Jurisdiction9,
                importedMarionProperty.Jurisdiction10,
                importedMarionProperty.Jurisdiction11,
                importedMarionProperty.Jurisdiction12
            };

            var ISDJurisdiction = jurisdictions.Where(j => (j / 10) * 10 == 30).FirstOrDefault();

            if (!JurisdictionMap.ContainsKey(ISDJurisdiction)) return "No ISD";

            return JurisdictionMap[ISDJurisdiction];

        }
    }
}
