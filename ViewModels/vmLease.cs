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
    public class vmLease : ViewModelBase
    {
        private const string UpdateByDefault = "MPW";
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool _leaseImportEnabled = true;
        private bool _leaseUploadEnabled = false;
        
        public ObservableCollection<mMarionLease> GreenFoxTracts { get; set; }
        public ObservableCollection<mMarionLease> KildareTracts { get; set; }
        public ObservableCollection<mMarionLease> OneTractLeases { get; set; }
        
        public ObservableCollection<mWellOperatorID> WellOperatorRrcData { get; private set; }
        public ObservableCollection<mWellOperatorData> WellOperatorData { get; private set; }

        public ICommand CommandImportLeases => new RelayCommand(OnImportLeases);
        public ICommand CommandUploadLeases => new RelayCommand(OnUploadLeases);

        public static ObservableCollection<mMarionLease> LeaseDataFromAbMarionImport { get; set; }
                       
        public ObservableCollection<mMarionOperator> OperatorsFromAbMarionImport { get; private set; }       
        public ObservableCollection<string> OperatorNamesFromAbMarionImport { get; private set; }

        //public IDictionary<string, mMarionOperator> AbMarionOperatorNameIdMap { get; private set; }
        //public static IDictionary<string, string> OperatorRrcDataMap { get; private set; } = new Dictionary<string, string>();
        //public static IDictionary<string, mWellOperatorData> OperatorDataMap { get; private set; } = new Dictionary<string, mWellOperatorData>();


        public vmLease()
        {           
            OperatorsFromAbMarionImport = new ObservableCollection<mMarionOperator>();
            OperatorNamesFromAbMarionImport = new ObservableCollection<string>();
            
            LeaseDataFromAbMarionImport = new ObservableCollection<mMarionLease>();
            //WellOperatorRrcData = new ObservableCollection<mWellOperatorID>();
              WellOperatorRrcData = new ObservableCollection<mWellOperatorID>();
        }

        private void OnImportLeases()
        {            
            GetLeaseDataFromAbMarionImportTable();
            GetOperatorIdAndRrcNumbersFromWell();
        }

        private void GetOperatorIdAndRrcNumbersFromWell()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                string sqlString = $"Select distinct w.RrcLease,w.RrcOpr from tblWell w where CadID='MAR'";
                var wells = db.Query(sqlString).ToList(); ;
                foreach (var well in wells)
                {
                    var operData = new mWellOperatorID();
                    operData.RrcLease = int.Parse(well.RrcLease).ToString();
                    operData.RrcOpr = well.RrcOpr;
                    WellOperatorRrcData.Add(operData);
                    //if (!OperatorRrcDataMap.ContainsKey(operData.RrcLease))
                    //    OperatorRrcDataMap.Add(operData.RrcLease, operData.RrcOpr);
                }
            }
        }

        private void GetOperatorDataFromWell()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                string sqlString = $"Select distinct w.RrcLease,w.RrcOpr,w.LpdLeaseName, " +
                    $"cast(w.LpdStatusDate as datetime), " +
                    $"cast(w.OprDODateRecvd as datetime), " +
                    $"cast (w.OprDODatePosted as datetime) " +
                    "from tblWell w where CadID = 'MAR'";
                var wells = db.Query(sqlString).ToList(); ;
                foreach (var well in wells)
                {
                    var operData = new mWellOperatorData();

                    operData.RrcOpr = well.RrcOpr;
                    operData.RrcLease = well.RrcOpr;
                    //operData.LpdLeaseName = well.LpdLeaseName;
                    //operData.LpdStatusDate = well.LpdStatusDate;
                    //operData.OprDODateRecvd = well.OprDODateRecvd;
                    //operData.OprDODatePosted = well.OprDODatePosted;
                    //operData.LpdStatus = well.LpdStatus;

                    operData.RrcLease = int.Parse(well.RrcLease).ToString();
                    operData.RrcOpr = well.RrcOpr;
                    WellOperatorData.Add(operData);
                    //if (!OperatorDataMap.ContainsKey(operData.RrcLease))
                    //    OperatorDataMap.Add(operData.RrcLease, operData);
                }
            }
        }


       

        private void GetLeaseDataFromAbMarionImportTable()
        {
            LeaseDataFromAbMarionImport.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionLease>("Select RRC, Description1, Description2, LeaseNumber, " +
                                                     "LeaseName, OperatorName, SPTBCode, Acres " +
                                                     "from AbMarionImport where SPTBCode = 'G1 ' or SPTBCode = 'XV ' order by LeaseName");

                var distinctResults = results.Distinct(new LeaseComparer()).OrderBy(x => x.RRC).ToList();

                var multiTracts = distinctResults.GroupBy(x => x.RRC.Trim())
                    .Where(y => y.Count() > 1)
                    .Select(y => new { Element = y.Key, Counter = y.Count() })
                    .ToList();

                var greenFoxTracts = distinctResults.Where(x => x.RRC.Trim() == "RRC #   5264").ToList();
                var kildareTracts = distinctResults.Where(x => x.RRC.Trim() == "RRC #   4738").ToList();
                var oneTractLeases = distinctResults
                    .Where(x => x.RRC.Trim() != "RRC #   4738" && x.RRC.Trim() != "RRC #   5264").ToList();
                GreenFoxTracts = new ObservableCollection<mMarionLease>(greenFoxTracts);
                KildareTracts = new ObservableCollection<mMarionLease>(kildareTracts);
                OneTractLeases = new ObservableCollection<mMarionLease>(oneTractLeases);

                //distinctResults.ForEach(marionLease => MarionMineralAccounts.Add(marionLease));
                foreach (mMarionLease lease in distinctResults)
                {
                    LeaseDataFromAbMarionImport.Add(lease);
                }
            }

            LeaseDataFromAbMarionImport = new ObservableCollection<mMarionLease>(LeaseDataFromAbMarionImport.OrderBy(x => x.RRC).ThenBy(x => x.LeaseNumber));

            LeaseImportEnabled = false;
            LeaseUploadEnabled = true;
        }

        private void OnUploadLeases()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                foreach (var marionLease in OneTractLeases)
                {
                    string rrcOperId, formattedRRC; long primaryLeaseKey;
                    InsertLease(db, marionLease, out rrcOperId, out formattedRRC, out primaryLeaseKey);

                    InsertCadLease(db, marionLease, primaryLeaseKey);

                    bool isOneTractLease = true;
                    var tractId = 0;
                    InsertTract(db, marionLease, primaryLeaseKey, tractId, isOneTractLease);
                }

                var kildareTractId = 0;
                bool kildareLeaseAlreadyInserted = false;
                long oldKildarePrimaryLeaseKey = 0;
                foreach (var marionLease in KildareTracts)
                {
                    string rrcOperId, formattedRRC; long primaryLeaseKey = 0;
                    if (!kildareLeaseAlreadyInserted)
                    {
                        InsertLease(db, marionLease, out rrcOperId, out formattedRRC, out primaryLeaseKey);
                        oldKildarePrimaryLeaseKey = primaryLeaseKey;

                        InsertCadLease(db, marionLease, primaryLeaseKey);
                    }
                    else
                    {
                        primaryLeaseKey = oldKildarePrimaryLeaseKey;
                    }
                    kildareLeaseAlreadyInserted = true;

                    bool isOneTractLease = false;
                    //++kildareTractId;                    
                    kildareTractId = int.Parse(marionLease.LeaseName.Trim().Substring(marionLease.LeaseName.IndexOf('#') + 1));
                    InsertTract(db, marionLease, primaryLeaseKey, kildareTractId, isOneTractLease);
                }

                var greenFoxTractId = 0;
                bool greenFoxLeaseAlreadyInserted = false;
                long oldGreenFoxPrimaryLeaseKey = 0;
                foreach (var marionLease in GreenFoxTracts)
                {
                    string rrcOperId, formattedRRC; long primaryLeaseKey = 0;
                    if (!greenFoxLeaseAlreadyInserted)
                    {
                        InsertLease(db, marionLease, out rrcOperId, out formattedRRC, out primaryLeaseKey);
                        oldGreenFoxPrimaryLeaseKey = primaryLeaseKey;

                        InsertCadLease(db, marionLease, primaryLeaseKey);
                    }
                    else
                    {
                        primaryLeaseKey = oldGreenFoxPrimaryLeaseKey;
                    }
                    greenFoxLeaseAlreadyInserted = true;

                    bool isOneTractLease = false;
                    //++greenFoxTractId;
                    greenFoxTractId = int.Parse(marionLease.LeaseName.Trim().Substring(marionLease.LeaseName.IndexOf('#') + 1));
                    InsertTract(db, marionLease, primaryLeaseKey, greenFoxTractId, isOneTractLease);
                }

                //UpdateTblNameOperatorFlags(db);       //this is superceded by CRW list of operators

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                LeaseUploadEnabled = false;
                MessageBox.Show($"Finished uploading {LeaseDataFromAbMarionImport.Count()} leases");

                Messenger.Default.Send<LeaseFinishedMessage>(new LeaseFinishedMessage());
            }

        }

        private static void UpdateTblNameOperatorFlags(IDbConnection db)
        {
            // For all operators in AbMarionOperators change their Oper_YN in tblName to true.
            string sqlString = "update dbo.tblName " +
                               "set tblName.Oper_YN = o.OperatorFlag " +
                               "from dbo.AbMarionOperators o " +
                               "inner join dbo.tblName n " +
                               "on o.CompanyID = n.NameID " +
                               "where o.Active = 1";
            var affectedRows = db.Execute(sqlString);
        }

        private void InsertTract(IDbConnection db, mMarionLease marionLease, long primaryLeaseKey, int tractId, bool isOneTractLease)
        {
            var populatedTract = TranslateFrom_mMarionLeaseTo_mTract(tractId);
            populatedTract.LeaseID = primaryLeaseKey;
            populatedTract.PropID = vmProperty.MineralPropertyIdMap[marionLease.LeaseNumber];

            var tractAcres = LeaseDataFromAbMarionImport.Where(t => t.LeaseNumber == marionLease.LeaseNumber).FirstOrDefault().Acres; //tract acres
            var leaseAcres = (from m in LeaseDataFromAbMarionImport where m.RRC == marionLease.RRC select m.Acres).Sum();//sum of tract acres in lease
            populatedTract.LeasePct = Math.Round(leaseAcres != 0 ? tractAcres / leaseAcres : 0.0, 3);
            if (isOneTractLease) populatedTract.LeasePct = 1.0;
            //Note!! LeasePct needs to be inserted into tblAccount.Legal ??????? how? parse legal and re-build?

            db.Insert<mTract>(populatedTract);
        }

        private static void UpdateRrcOperId(IDbConnection db, string rrcOperId, string formattedRRC, long primaryLeaseKey)
        {
            // after getting the primaryLeaseKey update tblWell with the matching 
            if (!string.IsNullOrEmpty(rrcOperId))
            {
                string sql = $"UPDATE tblWell SET LeaseID = @LeaseID WHERE RrcLease = @rrc;";
                int rowsAffected = db.Execute(sql, new { rrc = formattedRRC, LeaseID = primaryLeaseKey });
            }
        }

        private void InsertLease(IDbConnection db, mMarionLease marionLease, out string rrcOperId, out string formattedRRC, out long primaryLeaseKey)
        {
            var populatedLease = TranslateFrom_mMarionLeaseTo_mLease(marionLease);
            populatedLease.LeaseNameWag = marionLease.LeaseName;
            rrcOperId = "";
            formattedRRC = RRCHelper.FormatRRC(populatedLease.RrcLease, out rrcOperId);
            if (!string.IsNullOrWhiteSpace(rrcOperId))
            {
                populatedLease.LeaseOprID = int.Parse(rrcOperId);
                var crwRrcOperId = int.Parse(rrcOperId).ToString();
                if (vmAgentAndOperator.CrwOperRrcIDToNameIdMap.ContainsKey(crwRrcOperId))
                    populatedLease.LeaseOprID = (int)vmAgentAndOperator.CrwOperRrcIDToNameIdMap[crwRrcOperId];
            }

            var lpdLeaseName = db.ExecuteScalar($"SELECT TOP 1 LpdLeaseName FROM tblWell where RrcLease = '{formattedRRC}'") as string;
            populatedLease.LeaseNameOpr = lpdLeaseName;

            primaryLeaseKey = db.Insert<mLease>(populatedLease);

            if (!string.IsNullOrEmpty(rrcOperId))
            {
                string sql = $"UPDATE tblWell SET LeaseID = @LeaseID WHERE RrcLease = @rrc;";
                int rowsAffected = db.Execute(sql, new { rrc = formattedRRC, LeaseID = primaryLeaseKey });
            }
        }

        private void InsertCadLease(IDbConnection db, mMarionLease marionLease, long primaryLeaseKey)
        {
            var populatedCadLease = TranslateFrom_mMarionLeaseTo_mCadLease(marionLease);
            populatedCadLease.LeaseId = primaryLeaseKey;

            db.Insert<mCadLease>(populatedCadLease);
        }

        private mCadLease TranslateFrom_mMarionLeaseTo_mCadLease(mMarionLease marionLease)
        {
            var cadLease = new mCadLease();
            cadLease.CadId = "MAR";

            cadLease.CadLeaseId = marionLease.LeaseNumber.ToString().PadLeft(7, '0');

            return cadLease;
        }

        private mTract TranslateFrom_mMarionLeaseTo_mTract(int tractId)
        {
            var tract = new mTract();

            tract.TractID = tractId.ToString().Trim().PadLeft(3, '0');
            tract.CadID = "MAR";
            tract.delflag = false;

            tract.StatDate = DateTime.Now;
            tract.StatBy = UpdateByDefault;
            tract.StatReason = "Import";
            tract.Stat_YN = true;

            tract.UpdateBy = UpdateByDefault;
            tract.UpdateDate = DateTime.Now;

            return tract;
        }

        private mLease TranslateFrom_mMarionLeaseTo_mLease(mMarionLease marionLease)
        {
            var lease = new mLease();

            var parser1 = new RrcParser();
            var marionRrc = parser1.GetRRCnumberFromImportRRCstring(marionLease.RRC);
            string rrcOperId = "";
            lease.RrcLease = RRCHelper.FormatRRC(marionRrc, out rrcOperId);

            lease.Stat_YN = true;
            lease.StatBy = UpdateByDefault;
            lease.StatDate = DateTime.Now;
            lease.StatReason = "Import";

            lease.UpdateBy = UpdateByDefault;
            lease.UpdateDate = DateTime.Now;

            return lease;

        }

        //private void GetOperatorNamesFromAbMarionImport()
        //{
        //    using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
        //    {
        //        string operatorNamesQueryString = 
        //            "use wagapp2_2021_Marion Select distinct OperatorName from AbMarionImport where sptbCode='G1 ' order by OperatorName ";
        //        OperatorNamesFromAbMarionImport = db.Query<string>(operatorNamesQueryString).ToList();
        //    }
        //}

        public bool LeaseImportEnabled { get => _leaseImportEnabled; set { _leaseImportEnabled = value; RaisePropertyChanged(nameof(LeaseImportEnabled)); } }
        public bool LeaseUploadEnabled { get => _leaseUploadEnabled; set { _leaseUploadEnabled = value; RaisePropertyChanged(nameof(LeaseUploadEnabled)); } }


    }

}
