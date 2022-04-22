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

        public ObservableCollection<mMarionLease> MarionMineralAccounts { get; set; }
        public static ObservableCollection<mMarionOperator> MarionOperators { get; set; }
        public ObservableCollection<mWellOperatorID> WellOperatorRrcData { get; private set; }
        public ObservableCollection<mWellOperatorData> WellOperatorData { get; private set; }

        public ICommand CommandImportLeases => new RelayCommand(OnImportLeases);
        public ICommand CommandUploadLeases => new RelayCommand(OnUploadLeases);

        public static IDictionary<string, long> OperatorNameIdMap { get; private set; }
        //public static IDictionary<string, string> OperatorRrcDataMap { get; private set; } = new Dictionary<string, string>();
        public static IDictionary<string, mWellOperatorData> OperatorDataMap { get; private set; } =
            new Dictionary<string, mWellOperatorData>();
        public List<string> OperatorNamesFromMarionImport { get; private set; }
        public List<string> OperatorNamesFromCrwImport { get; private set; }



        public vmLease()
        {
            MarionMineralAccounts = new ObservableCollection<mMarionLease>();
            MarionOperators = new ObservableCollection<mMarionOperator>();
            WellOperatorRrcData = new ObservableCollection<mWellOperatorID>();
        }

        private void OnImportLeases()
        {
            //SelectOperatorDataFromMarionImportTableAndWagOwners();
            SelectMineralDataFromMarionImportTable();
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
                    if (!OperatorDataMap.ContainsKey(operData.RrcLease))
                        OperatorDataMap.Add(operData.RrcLease, operData);
                }
            }
        }


        private static void SelectOperatorDataFromMarionImportTableAndWagOwners()
        {
            MarionOperators.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var operatorResults = db.Query<mMarionOperator>
                    ("SELECT * from AbMarionOperators Where Active=1 order by OperatorName ");
                var operatorDistinctResults = operatorResults.Distinct(new OperatorComparer()).ToList();
                operatorDistinctResults.ForEach(marionOperator => MarionOperators.Add(marionOperator));

                //foreach (mMarionOperator oper in MarionOperators)
                //{
                //    if (!OperatorNameIdMap.ContainsKey(oper.CompanyName))
                //    {
                //        OperatorNameIdMap = MarionOperators.ToDictionary(op => op.CompanyName, val => (long)val.CompanyID);
                //    }
                //}
            }
        }

        private void SelectMineralDataFromMarionImportTable()
        {
            MarionMineralAccounts.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionLease>("use wagapp2_2021_Marion Select RRC, Description1, Description2, LeaseNumber, LeaseName, OperatorName, SPTBCode, Acres " +
                                                     "from AbMarionImport where SPTBCode = 'G1 ' or SPTBCode = 'XV ' order by LeaseName");

                var distinctResults = results.Distinct(new LeaseComparer()).OrderBy(x => x.RRC).ToList();

                var multiTracts = distinctResults.GroupBy(x => x.RRC.Trim())
                    .Where(y=>y.Count() > 1)
                    .Select(y => new { Element = y.Key, Counter = y.Count() })
                    .ToList();

                var greenFoxTracts = distinctResults.Where(x => x.RRC.Trim() == "RRC #   5264").ToList();
                var kildareTracts = distinctResults.Where(x => x.RRC.Trim() == "RRC #   4738").ToList();
                var oneTractLeases = distinctResults.Where(x => x.RRC.Trim() != "RRC #   4738" && x.RRC.Trim() != "RRC #   5264").ToList();

                //distinctResults.ForEach(marionLease => MarionMineralAccounts.Add(marionLease));
                foreach (mMarionLease lease in distinctResults)
                {
                    MarionMineralAccounts.Add(lease);
                }
            }

            MarionMineralAccounts = new ObservableCollection<mMarionLease>(MarionMineralAccounts.OrderBy(x => x.RRC).ThenBy(x => x.LeaseNumber));

            LeaseImportEnabled = false;
            LeaseUploadEnabled = true;
        }

        private void OnUploadLeases()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var marionLeases = MarionMineralAccounts.GroupBy(m => m.RRC).Select(g => g.FirstOrDefault()).ToList();

                //GetOperatorNamesFromCrwImport();
                //GetOperatorNamesFromMarionImport();

                var marionRRC = "";
                var oldMarionRRC = "";
                var marionLeaseNumber = 0;
                var oldMarionLeaseNumber = 0;
                var oldMarionLeaseName = "";
                int tractId = 0;
                int oldTractId = 0;
                long oldPrimaryLeaseKey = 0;
                string currentLeaseNameWag = "";

                foreach (var marionLease in marionLeases)
                {

                    marionRRC = marionLease.RRC;
                    marionLeaseNumber = marionLease.LeaseNumber;

                    if (marionRRC != oldMarionRRC)
                    {
                        currentLeaseNameWag = marionLease.LeaseName;
                        tractId = 1;
                        oldPrimaryLeaseKey = 0;
                    }
                    else
                    {
                        currentLeaseNameWag = oldMarionLeaseName;
                        tractId = oldTractId + 1;
                    }

                    var populatedLease = TranslateFrom_mMarionLeaseTo_mLease(marionLease, MarionOperators);
                    populatedLease.LeaseNameWag = currentLeaseNameWag;
                    string rrcOperId = "";
                    var formattedRRC = RRCHelper.FormatRRC(populatedLease.RrcLease, out rrcOperId);

                    if (!string.IsNullOrWhiteSpace(rrcOperId))
                    {
                        populatedLease.LeaseOprID = int.Parse(rrcOperId);
                        var crwRrcOperId = int.Parse(rrcOperId).ToString();
                        if (vmAgentAndOperator.CrwOperRrcIDToNameIdMap.ContainsKey(crwRrcOperId))
                            populatedLease.LeaseOprID = (int)vmAgentAndOperator.CrwOperRrcIDToNameIdMap[crwRrcOperId];
                    }

                    var lpdLeaseName = db.ExecuteScalar($"SELECT TOP 1 LpdLeaseName FROM tblWell where RrcLease = '{formattedRRC}'") as string;
                    populatedLease.LeaseNameOpr = lpdLeaseName;


                    var primaryLeaseKey = db.Insert<mLease>(populatedLease);
                    oldPrimaryLeaseKey = (int)primaryLeaseKey;


                    // after getting the primaryLeaseKey update tblWell with the matching 
                    if (!string.IsNullOrEmpty(rrcOperId))
                    {
                        string sql = $"UPDATE tblWell SET LeaseID = @LeaseID WHERE RrcLease = @rrc;";
                        int rowsAffected = db.Execute(sql, new { rrc = formattedRRC, LeaseID = primaryLeaseKey });
                    }

                    InsertCadLease(db, marionLeaseNumber, oldMarionLeaseNumber, marionLease, primaryLeaseKey);


                    if (marionLeaseNumber != oldMarionLeaseNumber)
                    {
                        var populatedTract = TranslateFrom_mMarionLeaseTo_mTract(tractId);
                        populatedTract.LeaseID = oldPrimaryLeaseKey;
                        populatedTract.PropID = vmProperty.MineralPropertyIdMap[marionLease.LeaseNumber];

                        var tractAcres = MarionMineralAccounts.Where(t => t.LeaseNumber == marionLease.LeaseNumber).FirstOrDefault().Acres; //tract acres
                        var leaseAcres = (from m in MarionMineralAccounts where m.RRC == marionLease.RRC select m.Acres).Sum();//sum of tract acres in lease
                        populatedTract.LeasePct = Math.Round(leaseAcres != 0 ? tractAcres / leaseAcres : 0.0, 3);
                        //Note!! LeasePct needs to be inserted into tblAccount.Legal ??????? how? parse legal and re-build?

                        db.Insert<mTract>(populatedTract);
                    }
                    oldMarionRRC = marionRRC;
                    oldMarionLeaseNumber = marionLeaseNumber;
                    oldMarionLeaseName = populatedLease.LeaseNameWag;
                    oldTractId = tractId;
                }

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                LeaseUploadEnabled = false;
                MessageBox.Show($"Finished uploading {MarionMineralAccounts.Count()} leases");

                Messenger.Default.Send<LeaseFinishedMessage>(new LeaseFinishedMessage());
            }

        }

        private void InsertCadLease(IDbConnection db, int marionLeaseNumber, int oldMarionLeaseNumber, mMarionLease marionLease, long primaryLeaseKey)
        {
            var populatedCadLease = TranslateFrom_mMarionLeaseTo_mCadLease(marionLease);
            populatedCadLease.LeaseId = primaryLeaseKey;

            if (marionLeaseNumber != oldMarionLeaseNumber) db.Insert<mCadLease>(populatedCadLease);
        }

        private void GetOperatorNamesFromCrwImport()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string operatorNamesQueryString = "use wagapp2_2021_Marion Select * from AbMarionOperatorsFromCRW ";
                OperatorNamesFromCrwImport = db.Query<string>(operatorNamesQueryString).ToList();
            }
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

        private mLease TranslateFrom_mMarionLeaseTo_mLease(mMarionLease marionLease, ObservableCollection<mMarionOperator> marionOperators)
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

        private void GetOperatorNamesFromMarionImport()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string operatorNamesQueryString = "use wagapp2_2021_Marion Select distinct OperatorName from AbMarionImport where sptbCode='G1 ' order by OperatorName ";
                OperatorNamesFromMarionImport = db.Query<string>(operatorNamesQueryString).ToList();
            }
        }

        public bool LeaseImportEnabled { get => _leaseImportEnabled; set { _leaseImportEnabled = value; RaisePropertyChanged(nameof(LeaseImportEnabled)); } }
        public bool LeaseUploadEnabled { get => _leaseUploadEnabled; set { _leaseUploadEnabled = value; RaisePropertyChanged(nameof(LeaseUploadEnabled)); } }


    }

}
