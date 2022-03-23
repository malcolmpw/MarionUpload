using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MarionUpload.Comparers;
using MarionUpload.Helpers;
using MarionUpload.Messages;
using MarionUpload.Models;
using MarionUpLoad.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

        public ObservableCollection<mMarionProperty> MarionProperties { get; set; }
        //public static ObservableCollection<mMarionOwner> MarionOwners { get; set; }

        public ICommand CommandImportProperties => new RelayCommand(OnImportProperties);
        public ICommand CommandUploadProperties => new RelayCommand(OnUploadProperties);

        public IDictionary<string, long> PropIdMap { get; private set; } = new Dictionary<string, long>();
        public Dictionary<int, string> JurisdictionMap { get; private set; }
        public Dictionary<string, string> PtdPropMap { get; private set; }

        public bool PropertyImportEnabled { get => _propertyImportEnabled; set { _propertyImportEnabled = value; RaisePropertyChanged(nameof(PropertyImportEnabled)); } }
        public bool PropertyUploadEnabled { get => _propertyUploadEnabled; set { _propertyUploadEnabled = value; RaisePropertyChanged(nameof(PropertyUploadEnabled)); } }

        public vmProperty()
        {
            MarionProperties = new ObservableCollection<mMarionProperty>();
        }

        private void OnImportProperties()
        {
            SelectPropertyDataFromMarionImportTable();
        }

        private void SelectPropertyDataFromMarionImportTable()
        {
            //NOTE:! I queried AbMarionImport:
            // distinct Description1,Description2                       and got 646 rows.
            // distinct Description1,Description2,PropertyType          and got 647 rows.
            // distinct Description1,Description2,PropertyType,SPTBcode and got 652 rows.
            // I need all these fields so I am using the last query despite some (8) strange duplicates

            MarionProperties.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionProperty>(
                    "Select distinct LeaseNumber, PropertyType, SPTBCode, " +
                    "Description1, Description2," +
                    "LeaseName,RRC,OperatorName," +
                    "Jurisdiction1, Jurisdiction2, Jurisdiction3, " +
                    "Jurisdiction4, Jurisdiction5, Jurisdiction6, " +
                    "Jurisdiction7, Jurisdiction8, Jurisdiction9, " +
                    "Jurisdiction10, Jurisdiction11, Jurisdiction12 " +
                    "Juris2MarketValue " +
                    "from AbMarionImport");

                JurisdictionMap = db.Query<mJurisdiction>("Select Code,Name from abMariontlkpJurisdiction")
                    .ToDictionary(jurisdiction => jurisdiction.Code, val => val.Name);
                PtdPropMap = db.Query<mPtdProp>("Select PropClassSub, PropClassDesc from tlkpPtdPropClassSub").ToDictionary(key => key.PropClassSub, val => val.PropClassDesc);

                var resultList = results.Distinct(new PropertyComparer()).ToList();

                resultList.ForEach(marionProperty => MarionProperties.Add(marionProperty));

                PropertyImportEnabled = false;
                PropertyUploadEnabled = true;
            }
        }

        public static IDictionary<int, long> PropertyIdMap { get; private set; } = new Dictionary<int, long>();
        public static IDictionary<int, string> PropertyLegalMap { get; private set; } = new Dictionary<int, string>();
        public IDictionary<int, long> CadPropertyIdMap { get; private set; } = new Dictionary<int, long>();

        private void OnUploadProperties()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                foreach (mMarionProperty _marionProperty in MarionProperties)
                {
                    var populatedProperty = TranslateFrom_mMarionPropertyTo_mProperty(_marionProperty);
                    var primaryPropertyKey = db.Insert<mProperty>(populatedProperty);
                    PropertyIdMap.Add(_marionProperty.LeaseNumber, primaryPropertyKey);
                    PropertyLegalMap.Add((int)primaryPropertyKey, populatedProperty.Legal);
                    System.Diagnostics.Debug.WriteLine($"Primary Key: {primaryPropertyKey}");

                    var populatedCadProperty = TranslateFrom_mMarionPropertyTo_mCadProperty(_marionProperty, primaryPropertyKey);
                    var primaryCadPropertyKey = db.Insert<mCadProperty>(populatedCadProperty);
                    CadPropertyIdMap.Add(_marionProperty.LeaseNumber, primaryPropertyKey);

                    //Add a first segment to each property where SPTBCode <> 'G1'
                    if (_marionProperty.SPTBCode != "G1")
                    {
                        var populatedSegment = TranslateFrom_mPropertyTo_mSegment(populatedProperty, _marionProperty);
                        var primarySegmentKey = db.Insert<mSegment>(populatedSegment);
                    }
                }

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                MessageBox.Show($"Finished uploading {MarionProperties.Count()} properties");
                Messenger.Default.Send<PropertiesFinishedMessage>(new PropertiesFinishedMessage());
            }
        }

        private mSegment TranslateFrom_mPropertyTo_mSegment(mProperty property, mMarionProperty marionProperty)
        {
            var oppSegment = new mSegment();

            oppSegment.PropID = property.PropId;
            oppSegment.PrsnlID = 1;
            oppSegment.PrsnlDesc = "1st Segt: ";
            //oppSegment.PrsnlDesc += "(added: " + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ") ";
            oppSegment.PrsnlDesc += property.Legal;


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

            oppSegment.PrsnlValCur = marionProperty.Juris2MarketValue;
            oppSegment.PrsnlValFlat = marionProperty.Juris2MarketValue;

            oppSegment.delflag = false;

            return oppSegment;
        }

        private mCadProperty TranslateFrom_mMarionPropertyTo_mCadProperty(mMarionProperty marionProperty, long primaryPropertyKey)
        {
            var cadProperty = new mCadProperty();
            cadProperty.CadID = "MAR";
            cadProperty.CadPropid = marionProperty.LeaseNumber.ToString();
            cadProperty.CadPct = 1.0;
            cadProperty.PropID = (int)primaryPropertyKey;
            cadProperty.delflag = false;
            return cadProperty;
        }

        private mProperty TranslateFrom_mMarionPropertyTo_mProperty(mMarionProperty importedMarionProperty)
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

            if (importedMarionProperty.SPTBCode.Trim().Substring(0, 2) == "G1")
            {
                property.PropType = "M";

                string rrc = importedMarionProperty.RRC.Trim();
                string pat = @"(\d+)";
                Regex re = new Regex(pat);
                var match = re.Match(rrc);
                var rrcNumber = "";
                if (match.Success)
                {
                    rrcNumber = (match.Groups[0].Value).Trim();
                }

                property.Legal = importedMarionProperty.LeaseName.Trim() +
                    " (" + rrcNumber +
                    "); Opr: " + importedMarionProperty.OperatorName.Trim();

            }
            else
            {
                property.PropType = "P";
                //property.Legal = FetchPTDDescription(sptbCode) + "," +
                //                 FetchISDJurisdictionName(importedMarionProperty);  //importedMarionProperty.Description2;
                property.Legal = (importedMarionProperty.Description1).Trim() + "-" +
                                    (importedMarionProperty.Description2).Trim();
                property.Location = (importedMarionProperty.Description2).Trim();
            }

            //property.SegmentValue = importedMarionProperty.Juris2MarketValue;

            property.ControlCad = "MAR";

            property.UpdateWhy = _updateWhy;
            property.CreateBy = _updateBy;
            property.UpdateBy = _updateBy;
            property.UpdateDate = _updateDate;
            property.CreateWhy = _updateWhy;
            property.CreateDate = _updateDate;

            return property;
        }

        private string FetchPTDDescription(string sptbCode)
        {
            if (PtdPropMap.ContainsKey(sptbCode))
            {
                return PtdPropMap[sptbCode];
            }

            return "--";
        }

        private string FetchISDJurisdictionName(mMarionProperty importedMarionProperty)
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
