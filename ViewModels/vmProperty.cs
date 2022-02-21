using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight.Command;
using MarionUpload.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MarionUpload.ViewModels
{
    public class vmProperty
    {
        const string ConnectionString = @"Data Source=WAGSQLSRV01\DEV;Initial Catalog=wagapp2_2021_Marion;Integrated Security=True;";
        public ObservableCollection<mMarionProperty> MarionProperties { get; set; }
        //public static ObservableCollection<mMarionOwner> MarionOwners { get; set; }

        public ICommand CommandImportProperties => new RelayCommand(OnImportProperties);
        public ICommand CommandUploadProperties => new RelayCommand(OnUploadProperties);
        public IDictionary<string, long> PropIdMap { get; private set; } = new Dictionary<string, long>();
        public Dictionary<int, string> JurisdictionMap { get; private set; }
        public Dictionary<string, string> PtdPropMap { get; private set; }

        public vmProperty()
        {
            MarionProperties = new ObservableCollection<mMarionProperty>();
            //MarionOwners = new ObservableCollection<mMarionOwner>();
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
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                var results = db.Query<mMarionProperty>("Select distinct LeaseNumber, PropertyType,SPTBCode,Description1,Description2,LeaseName,RRC,OperatorName," +
                    "Jurisdiction1, Jurisdiction2, Jurisdiction3, " +
                    "Jurisdiction4, Jurisdiction5, Jurisdiction6, " +
                    "Jurisdiction7, Jurisdiction8, Jurisdiction9, " +
                    "Jurisdiction10, Jurisdiction11, Jurisdiction12" +
                    " from AbMarionImport");
                JurisdictionMap = db.Query<mJurisdiction>("Select Code,Name from abMariontlkpJurisdiction")
                    .ToDictionary(jurisdiction => jurisdiction.Code, val => val.Name);
                PtdPropMap = db.Query<mPtdProp>("Select PropClassSub, PropClassDesc from tlkpPtdPropClassSub").ToDictionary(key => key.PropClassSub, val => val.PropClassDesc);

                var resultList = results.Distinct(new LeaseNumberComparer()).ToList();


                resultList.ForEach(marionProperty => MarionProperties.Add(marionProperty));
             
            }

            
        }

        public static IDictionary<int, long> PropertyIdMap { get; private set; } = new Dictionary<int, long>();


        private void OnUploadProperties()
        {
            return;  // skip insert for now until property and account tables , and tblCadOwner are ready to test

            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                foreach (mMarionProperty m in MarionProperties)
                {
                    var populatedProperty = TranslateFrom_mMarionPropertyTo_mProperty(m);
                    var primaryKey = db.Insert<mProperty>(populatedProperty);
                    PropertyIdMap.Add(m.LeaseNumber, primaryKey);
                    System.Diagnostics.Debug.WriteLine($"Primary Key: {primaryKey}");
                }
            }
        }

        private mProperty TranslateFrom_mMarionPropertyTo_mProperty(mMarionProperty importedMarionProperty)
        {
            NameSorts nsorts = new NameSorts();
            var _updateDate = DateTime.Now;
            var _updateBy = "MPW";
            var _updateWhy = "MAR Import";

            var property = new mProperty();

            property.PtdClassSub = importedMarionProperty.SPTBCode.Trim();
            string sptbCode = importedMarionProperty.SPTBCode.Trim();
            if (sptbCode == "L1" || sptbCode == "L2")
            {
                property.PtdClass = sptbCode;
            }
            else
            {
                property.PtdClass = sptbCode.Substring(0, 1);
            }

            if (sptbCode == "G1")
            {
                string rrc = importedMarionProperty.RRC.Trim();
                string pat = @"(\d+)";
                Regex re = new Regex(pat);
                var match = re.Match(rrc);

                var rrcNumber = "";
                if (match.Success)
                {
                    rrcNumber = match.Groups[0].Value;
                }
                
                property.PropType = "M";
                property.Legal = importedMarionProperty.LeaseName.Trim() + " (" +
                    rrcNumber + ") " +
                    importedMarionProperty.OperatorName.Trim();
            }
            else
            {
                property.PropType = "P";
                property.Legal = FetchPTDDescription(sptbCode) + "," + FetchISDJurisdictionName(importedMarionProperty);  //importedMarionProperty.Description2;
            }

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
            var jurisdictions = new List<int> {importedMarionProperty.Jurisdiction1,
                importedMarionProperty.Jurisdiction2,
                importedMarionProperty.Jurisdiction3,
                importedMarionProperty.Jurisdiction4,
                importedMarionProperty.Jurisdiction5,
                importedMarionProperty.Jurisdiction6,
                importedMarionProperty.Jurisdiction7,
                importedMarionProperty.Jurisdiction8,
                importedMarionProperty.Jurisdiction9,importedMarionProperty.Jurisdiction10,
                importedMarionProperty.Jurisdiction11,importedMarionProperty.Jurisdiction12 };

            var ISDJurisdiction = jurisdictions.Where(j => (j / 10) * 10 == 30).FirstOrDefault();

            if (!JurisdictionMap.ContainsKey(ISDJurisdiction)) return "No ISD";

            return JurisdictionMap[ISDJurisdiction];

        }
    }

    public class LeaseNumberComparer : IEqualityComparer<mMarionProperty>
    {
        public bool Equals(mMarionProperty x, mMarionProperty y)
        {
            return x.LeaseNumber == y.LeaseNumber;
        }

        public int GetHashCode(mMarionProperty obj)
        {
            return obj.LeaseNumber.GetHashCode();
        }
    }
}
