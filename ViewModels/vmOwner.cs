using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MarionUpload.Helpers;
using MarionUpload.Messages;
using MarionUpload.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MarionUpload.ViewModels
{
    public class vmOwner : ViewModelBase
    {

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string MarionCounty2017QueryString = "SELECT distinct n.NameSortCad, n.NameSel_YN, " +
                                                   "n.NameH, n.NameF, n.NameM, n.NameL1, n.NameL2, n.NameLS, n.NameC, n.NameCP, n.Name2 " +
                                                   "FROM[WagData2017].[dbo].[tblName] n " +
                                                   "inner join[WagData2017].[dbo].[tblAccount] a " +
                                                   "on n.NameID = a.NameID " +
                                                   "inner join[WagData2017].[dbo].tblProperty p " +
                                                   "on a.PropID = p.PropID " +
                                                   "where p.ControlCad = 'MAR' " +
                                                   "order by n.NameSortCad ";

        public ObservableCollection<mMarionOwner> MarionOwners { get; set; }
        public ObservableCollection<string> MarionOwnerNames { get; set; }
        public ObservableCollection<mOwner> InsertedOwners { get; set; }
        public NameSorts NameSorts { get; set; }

        public ObservableCollection<mOwner> MarionOwners2017 { get; set; }
        public Dictionary<string, mOwner> CadOwner2017NameSortMap { get; set; }

        public static ObservableCollection<string> MarionOperatorNames { get; set; }
        public HashSet<string> HashSetOfOperatorNames { get; set; }

        public static ObservableCollection<mMarionOperatorRrc> MarionOperatorRRCs { get; set; }
        public Dictionary<string, string> MarionOperatorRrcMap { get; set; }

        public static ObservableCollection<mWellOperatorID> WellOperatorIDs { get; set; }
        public Dictionary<string, string> WellOperatorIdMap { get; set; }

        public static ObservableCollection<mMarionWellOperatorID> MarionWellOperatorIDs { get; set; }
        public Dictionary<string, string> MarionWellOperatorIdMap { get; set; }

        public ICommand CommandImportOwners => new RelayCommand(OnImportOwners);
        public ICommand CommandUploadOwners => new RelayCommand(OnUploadOwners);

        public bool OwnerImportEnabled { get => ownerImportEnabled; set { ownerImportEnabled = value; RaisePropertyChanged(nameof(OwnerImportEnabled)); } }
        public bool OwnerUploadEnabled { get => ownerUploadEnabled; set { ownerUploadEnabled = value; RaisePropertyChanged(nameof(ownerUploadEnabled)); } }
        private bool ownerImportEnabled = true;
        private bool ownerUploadEnabled = false;

        public List<mOwner> OwnersToInsert { get; set; }
        public static IDictionary<int, long> NameIdMap { get; private set; } = new Dictionary<int, long>();
        public IDictionary<int, long> MarionOwnerNumberToNameIdMap { get; private set; } = new Dictionary<int, long>();
        //public static IDictionary<string, long> NameSortCadMap { get; private set; } = new Dictionary<string, long>();

        private DateTime _updateDate;
        private string _updateBy;

        RrcParser rrcParser = new RrcParser();

        public vmOwner()
        {
            MarionOwners = new ObservableCollection<mMarionOwner>();
            MarionOwners2017 = new ObservableCollection<mOwner>();
            CadOwner2017NameSortMap = new Dictionary<string, mOwner>();
            MarionOperatorRrcMap = new Dictionary<string, string>();
            WellOperatorIdMap = new Dictionary<string, string>();
            MarionWellOperatorIdMap = new Dictionary<string, string>();
            MarionWellOperatorIDs = new ObservableCollection<mMarionWellOperatorID>();

            NameSorts = new NameSorts();
            HashSetOfOperatorNames = new HashSet<string>();
            OwnerImportEnabled = true;
            OwnerUploadEnabled = false;
        }

        private void OnImportOwners()
        {
            GetMarionOperatorRRCsFromImport();
            GetMarionOperatorIdsFromTblWell();
            GetMarionOperatorNamesFromImport();
            SelectOwnerDataFromWagData2017();
            SelectOwnerDataFromMarionImportTable();

            OwnerImportEnabled = false;
            OwnerUploadEnabled = true;
        }

        private void GetMarionOperatorRRCsFromImport()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string marionOperatorRrcSqlString = "select distinct RRC,OperatorName from wagapp2_2021_Marion.dbo.AbMarionImport where SPTBCode='G1 ' ";
                var wellMarionOperatorRRCs = db.Query<mMarionOperatorRrc>(marionOperatorRrcSqlString).ToList();
                MarionOperatorRRCs = new ObservableCollection<mMarionOperatorRrc>(wellMarionOperatorRRCs);
                foreach (mMarionOperatorRrc marionOperatorRrc in MarionOperatorRRCs)
                {
                    if (!MarionWellOperatorIdMap.ContainsKey(marionOperatorRrc.OperatorName))
                        MarionOperatorRrcMap.Add(marionOperatorRrc.RRC, marionOperatorRrc.OperatorName);
                }
            }
        }

        private void GetMarionOperatorIdsFromTblWell()
        {

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string wellOperatorSqlString = "select distinct RrcLease,RrcOpr from tblWell where CadID = 'MAR' ";
                var wellOperatorIDs = db.Query<mWellOperatorID>(wellOperatorSqlString).ToList();
                WellOperatorIDs = new ObservableCollection<mWellOperatorID>(wellOperatorIDs);
                foreach (mWellOperatorID wellOperatorId in WellOperatorIDs)
                {
                    if (!WellOperatorIdMap.ContainsKey(wellOperatorId.RrcLease))
                        WellOperatorIdMap.Add(wellOperatorId.RrcLease, wellOperatorId.RrcOpr);
                }
            }
        }

        private void GetMarionOperatorNamesFromImport()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string operatorNamesQueryString = "use wagapp2_2021_Marion Select distinct OperatorName from AbMarionImport where sptbCode='G1 ' order by OperatorName ";
                var marionOperatorNames = db.Query<string>(operatorNamesQueryString).ToList();
                MarionOperatorNames = new ObservableCollection<string>(marionOperatorNames);
                foreach (string operatorName in marionOperatorNames)
                {
                    HashSetOfOperatorNames.Add(operatorName.Trim());
                }
            }
        }

        private void SelectOwnerDataFromWagData2017()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string queryString = MarionCounty2017QueryString;
                var results2017 = db.Query<mOwner>(queryString);
                var distinctResults2017 = results2017.Distinct(new OwnerNumberComparer2017()).ToList();
                Create2017NameSelectDictionary(distinctResults2017);

                // Loop through each marion owner and fill in the NameSelect info.
                foreach (mMarionOwner marionOwner in MarionOwners)
                {
                    var ownerNameTrimmed = marionOwner.OwnerName.Trim().ToUpper();
                    if (CadOwner2017NameSortMap.ContainsKey(ownerNameTrimmed))
                    {
                        FillMarionOwnerWithNameSelectFlag(marionOwner);
                    }
                }

            }
        }

        private string FillMarionOwnerWithNameSelectFlag(mMarionOwner marionOwner)
        {
            string ownerNameTrimmed;
            {
                ownerNameTrimmed = marionOwner.OwnerName.Trim().ToUpper();
                marionOwner.NameSortCad = ownerNameTrimmed;
                marionOwner.NameSel_YN = CadOwner2017NameSortMap[ownerNameTrimmed].NameSel_YN;
            }

            return ownerNameTrimmed;
        }

        private void Create2017NameSelectDictionary(List<mOwner> distinctResults2017)
        {
            foreach (mOwner dr2017 in distinctResults2017)
            {
                MarionOwners2017.Add(dr2017);
                string CadOwner2017NameSortModified = dr2017.NameSortCad.Trim().ToUpper();
                if (!CadOwner2017NameSortMap.ContainsKey(CadOwner2017NameSortModified))
                {
                    if (!CadOwner2017NameSortMap.ContainsKey(CadOwner2017NameSortModified))
                        CadOwner2017NameSortMap.Add(CadOwner2017NameSortModified, dr2017);
                }
            }
        }

        private void OnUploadOwners()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            try
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    foreach (mMarionOwner _marionOwner in MarionOwners)
                    {
                        var populatedOwner = TranslateFrom_mMarionOwnerTo_mOwner(_marionOwner);
                        var primaryOwnerKey = db.Insert<mOwner>(populatedOwner);
                        if (!NameIdMap.ContainsKey(_marionOwner.OwnerNumber))
                            NameIdMap.Add(_marionOwner.OwnerNumber, primaryOwnerKey);

                        //if (!NameSortCadMap.ContainsKey(populatedOwner.NameSortCad.Trim().ToUpper()))
                        //{
                        //    NameSortCadMap.Add(populatedOwner.NameSortCad.Trim().ToUpper(), primaryOwnerKey);
                        //}

                        var populatedCadOwner = TranslateFrom_mMarionOwnerTo_mCadOwner(_marionOwner, primaryOwnerKey);
                        var primaryCadOwnerKey = db.Insert<mCadOwner>(populatedCadOwner);
                        if(!MarionOwnerNumberToNameIdMap.ContainsKey(_marionOwner.OwnerNumber))
                        MarionOwnerNumberToNameIdMap.Add(_marionOwner.OwnerNumber, primaryOwnerKey);

                    }
                }


                OwnerUploadEnabled = false;
                MessageBox.Show($"Finished uploading {MarionOwners.Count()} owners");

                Messenger.Default.Send<OwnerFinishedMessage>(new OwnerFinishedMessage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Uploading Owner Data -> {ex}");
                Messenger.Default.Send<OwnerFinishedMessage>(new OwnerFinishedMessage());
            }

            OwnerImportEnabled = false;
            OwnerUploadEnabled = false;

            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        }

        private mCadOwner TranslateFrom_mMarionOwnerTo_mCadOwner(mMarionOwner marionOwner, long primaryKey)
        {
            var cadOwner = new mCadOwner();
            cadOwner.CadID = "MAR";
            cadOwner.CadOwnerID = marionOwner.OwnerNumber.ToString();
            cadOwner.NameID = (int)primaryKey;
            cadOwner.delflag = false;
            return cadOwner;
        }

        void SelectOwnerDataFromMarionImportTable()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionOwner>("Select ImportID, OwnerNumber, OwnerName, InCareOf, StreetAddress, CityStateZip, AgentNumber From AbMarionImport");
                var distinctResults = results.Distinct(new OwnerNumberComparer()).ToList();
                distinctResults.ForEach(owner => MarionOwners.Add(owner));
            }
        }

        private mOwner TranslateFrom_mMarionOwnerTo_mOwner(mMarionOwner importedMarionOwner)
        {
            NameSorts nsorts = new NameSorts();
            _updateDate = DateTime.Now;
            _updateBy = "MPW";

            var owner = new mOwner();

            owner.CadID = "MAR";
            owner.NameSortCad = importedMarionOwner.OwnerName.Trim();
            //var matchingOwner = CadOwner2017NameSortMap[owner.NameSortCad];
            owner.Stat_YN = true;

          
            ////MarionOperatorRRCs=select from m in MarionOperatorRRCs(XamlGeneratedNamespace=>)

            //List<mMarionOperatorRrc> idsWithOnlyFirstRrc = new List<mMarionOperatorRrc>();

            //var newId = new mMarionOperatorRrc();
            //foreach (mMarionOperatorRrc oprc in MarionOperatorRRCs)
            //{
            //    newId = new mMarionOperatorRrc();
            //    newId.OperatorName = oprc.OperatorName;
            //    newId.RRC = MarionOperatorRRCs.Where(x => x.OperatorName == newId.OperatorName).Select(x => x.RRC).FirstOrDefault();
            //}
            //idsWithOnlyFirstRrc.Add(newId);
            //MarionOperatorRRCs = new ObservableCollection<mMarionOperatorRrc>(idsWithOnlyFirstRrc);

            //RrcParser rrcParser = new RrcParser();
            //foreach (mMarionWellOperatorID id in MarionWellOperatorIDs)
            //{
            //    var thisId = (from m in MarionOperatorRRCs
            //                  join w in WellOperatorIDs
            //                  on int.Parse(rrcParser.GetRRCnumberFromImportRRCstring(m.RRC))
            //                  equals int.Parse(rrcParser.GetRRCnumberFromImportRRCstring(w.RrcLease))
            //                  select new mMarionOperatorRrc
            //                  {
            //                      OperatorName = m.OperatorName,
            //                      RRC = MarionOperatorRRCs.Where(x => x.OperatorName == m.OperatorName).Select(x => x.RRC).FirstOrDefault()

            //                  }).FirstOrDefault();

            //    if (!MarionWellOperatorIdMap.ContainsKey(id.OperatorName.Trim()))
            //        MarionWellOperatorIdMap.Add(thisId.OperatorName.Trim(), thisId.RRC);
            //}

            //if (HashSetOfOperatorNames.Contains(importedMarionOwner.OwnerName.Trim()))
            //{
            //    owner.Oper_YN = true;
            //    owner.OperRrcID = MarionWellOperatorIdMap[importedMarionOwner.OwnerName.Trim()];
            //}
            //else
            //{
            //    owner.Oper_YN = false;
            //}

            mOwner matchingOwner;
            bool hasValue = CadOwner2017NameSortMap.TryGetValue(owner.NameSortCad, out matchingOwner);
            if (hasValue)
            {
                owner.NameSortFirst = owner.NameSortCad;
                owner.NameH = matchingOwner.NameH; // search the NameSortCad for titles, use SELECT distinct[NameH] FROM[WagData2017].[dbo].[tblName]                
                owner.NameF = matchingOwner.NameF; // split and parse
                owner.NameM = matchingOwner.NameM; // split and parse
                owner.NameL1 = matchingOwner.NameL1; // split and parse
                owner.NameL2 = matchingOwner.NameL2; // split and parse
                owner.NameLS = matchingOwner.NameLS; // search the NameSortCad for titles, use SELECT distinct[NameLS FROM[WagData2017].[dbo].[tblName]
                owner.NameC = matchingOwner.NameC;
                owner.NameCP = matchingOwner.NameCP;
                owner.Name2 = matchingOwner.Name2; // search the NameSortCad for titles, use SELECT distinct[Name2] FROM[WagData2017].[dbo].[tblName]
                                                   // these may be taken from WagData2017 for the old list of marion owners in tblName.               
                owner.NameSel_YN = matchingOwner.NameSel_YN;

            }
            else
            {
                owner.NameC = (importedMarionOwner.OwnerName).Trim();
                owner.NameSel_YN = true;
            }

            if (importedMarionOwner.AgentNumber == 0)
            {
                owner.AgentID = 66864;
                owner.Ntc2Agent_YN = false;
                owner.Stmnt2Agent_YN = false;
            }
            else
            {
                if (vmAgent.MarionAgentNumberToNameIdMap.ContainsKey(importedMarionOwner.AgentNumber))
                {

                    owner.AgentID = (int)vmAgent.MarionAgentNumberToNameIdMap[importedMarionOwner.AgentNumber];
                    owner.Ntc2Agent_YN = true;
                    owner.Stmnt2Agent_YN = true;
                }
                else
                {
                    owner.AgentID = 66864;
                    owner.Ntc2Agent_YN = false;
                    owner.Stmnt2Agent_YN = false;
                }
            }


            // !!!! ASK CW ABOUT IN CARE OF
            // Problem: importedMarionOwner address info is for the Agent and not for the owner if importedMarionOwner.InCareOf is not blank
            owner.Mail1 = importedMarionOwner.StreetAddress.Trim();
            var cityStateZip = importedMarionOwner.CityStateZip.Trim();
            int cityLength;
            int zipStartIndex;
            if (cityStateZip.Length > 0)
            {
                if (cityStateZip.Trim() != "LONDON" && cityStateZip.Trim() != "ABT0G-2")
                {
                    var hyphen = cityStateZip.Substring(cityStateZip.Length - 5, 1);
                    var hasZip4 = hyphen == "-";
                    cityLength = hasZip4 ? cityStateZip.Length - 12 : cityStateZip.Length - 7;
                    owner.MailCi = cityStateZip.Substring(0, cityLength).Trim();
                    zipStartIndex = cityLength;
                    owner.MailSt = cityStateZip.Substring(zipStartIndex, 2);
                    owner.MailZ = cityStateZip.Substring(zipStartIndex + 2, 5);
                    owner.MailZ4 = hasZip4 ? cityStateZip.Substring(zipStartIndex + 8, 4) : "";
                }
                else
                {
                    owner.MailCi = cityStateZip.Trim();
                    owner.MailSt = "";
                    owner.MailZ = "";
                    owner.MailZ4 = "";
                }
            }
            // Example:
            // Land O Lakes TX12345
            // 00000000001111111111
            // 01234567890123456789

            //Not Used:
            //Regex re = new Regex(@"\-\d{4}$");
            //var matchResult = re.Match(cityStateZip);
            //if (matchResult.Success)
            //{
            //    owner.MailZ4 = cityStateZip.Substring(26, 4);
            //}                       

            owner.UpdateDate = _updateDate;
            owner.UpdateBy = _updateBy;

            owner = NameSorts.RebuildNameSort(owner);

            return owner;
        }

        //public int SelectAgentNameIdFromMarionAgentImportTable(string marionAgentId)
        //{
        //    using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
        //    {
        //        var result = db.Query<mMarionAgent>($"Select NameId From AbMarionAgents where AgentId = {marionAgentId} ");

        //        if (result.Count() == 0) // could not find this agent in the Marion Agent Table
        //        {
        //            Log.Error($"Could not find agent {marionAgentId} in the Marion Agent Table");
        //            // MessageBox.Show($"Could not find agent {marionAgentId} in the Marion Agent Table");
        //            return 66864;
        //        }

        //        var AgentNameId = result.FirstOrDefault().NameId;
        //        return AgentNameId;
        //    }
        //}
    }

    public class OwnerNumberComparer : IEqualityComparer<mMarionOwner>
    {
        public bool Equals(mMarionOwner x, mMarionOwner y)
        {
            return x.OwnerNumber == y.OwnerNumber;
        }

        public int GetHashCode(mMarionOwner obj)
        {
            return obj.OwnerNumber.GetHashCode();
        }
    }

    public class OwnerNumberComparer2017 : IEqualityComparer<mOwner>
    {
        public bool Equals(mOwner x, mOwner y)
        {
            return x.NameSortCad == y.NameSortCad;
        }

        public int GetHashCode(mOwner obj)
        {
            return obj.NameSortCad.GetHashCode();
        }
    }

    // THIS IS NOT USED!  IT IS COPIED FROM WAGAPP2 FOR REFERENCE
    public class NameSorts
    {
        public mOwner RebuildNameSort(mOwner Owner)
        {
            string sWorking = "";

            if (Owner.NameSel_YN)
            {
                if (Owner.NameC != "" && Owner.NameC != null) sWorking = Owner.NameC.Trim();
                if (Owner.NameCP != "" && Owner.NameCP != null)
                {
                    sWorking = GetTruncatedSubstring(Owner.NameC.Trim() + ", " + Owner.NameCP.Trim(), 45);
                }

                Owner.Addr1 = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSortCad = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSort = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSortFirst = GetTruncatedSubstring(sWorking, 40);
            }
            else
            {
                Owner.NameSel_YN = false;

                // Build NameSortFirst
                sWorking = "";

                if (Owner.NameF != "" && Owner.NameF != null) sWorking = Owner.NameF.Trim();
                if (Owner.NameM != "" && Owner.NameM != null) sWorking = sWorking + " " + Owner.NameM.Trim();
                if (Owner.NameL1 != "" && Owner.NameL1 != null) sWorking = sWorking + " " + Owner.NameL1.Trim();
                if (Owner.NameLS != "" && Owner.NameLS != null) sWorking = sWorking + " " + Owner.NameLS.Trim();
                if (Owner.NameL2 != "" && Owner.NameL2 != null) sWorking = sWorking + " " + Owner.NameL2.Trim();
                if (Owner.NameT != "" && Owner.NameT != null) sWorking = sWorking + " " + Owner.NameT.Trim();
                if (Owner.NameH != "" && Owner.NameH != null) sWorking = sWorking + ", " + Owner.NameH.Trim();


                Owner.Addr1 = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSortFirst = GetTruncatedSubstring(sWorking, 40);


                // Build NameSort
                sWorking = "";

                if (Owner.NameL1 != "" && Owner.NameL1 != null) sWorking = Owner.NameL1.Trim();
                if (Owner.NameF != "" && Owner.NameF != null) sWorking = sWorking + ", " + Owner.NameF.Trim();
                if (Owner.NameM != "" && Owner.NameM != null) sWorking = sWorking + " " + Owner.NameM.Trim();
                if (Owner.NameLS != "" && Owner.NameLS != null) sWorking = sWorking + ", " + Owner.NameLS.Trim();
                if (Owner.NameL2 != "" && Owner.NameL2 != null) sWorking = sWorking + " " + Owner.NameL2.Trim();
                if (Owner.NameH != "" && Owner.NameH != null) sWorking = sWorking + " " + Owner.NameH.Trim();
                if (Owner.NameT != "" && Owner.NameT != null) sWorking = sWorking + ", " + Owner.NameT.Trim();

                Owner.NameSort = GetTruncatedSubstring(sWorking, 40);

                // Build NameSortCad
                sWorking = "";

                if (Owner.NameL1 != "" && Owner.NameL1 != null) sWorking = Owner.NameL1.Trim();
                if (Owner.NameF != "" && Owner.NameF != null) sWorking = sWorking + " " + Owner.NameF.Trim();
                if (Owner.NameM != "" && Owner.NameM != null) sWorking = sWorking + " " + Owner.NameM.Trim();
                if (Owner.NameLS != "" && Owner.NameLS != null) sWorking = sWorking + " " + Owner.NameLS.Trim();
                if (Owner.NameL2 != "" && Owner.NameL2 != null) sWorking = sWorking + " " + Owner.NameL2.Trim();

                Owner.NameSortCad = GetTruncatedSubstring(sWorking, 40);

            }
            return Owner;

            //SaveOwnerPrimaryData(Owner); // save any new changes before rebuilding namesort
            //Messenger.Default.Send(new OwnerUpdateNameSelectMessage() { Owner = Owner });

        }
        string GetTruncatedSubstring(string workingString, int trimLength)
        {
            if (workingString.Length > trimLength)
            {
                return workingString.Substring(0, trimLength);
            }
            return workingString;
        }
    }
}
