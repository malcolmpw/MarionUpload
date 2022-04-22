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
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MarionUpload.Comparers;
using System.Collections.Generic;

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

                var distinctResults = results.Distinct(new LeaseComparer()).ToList();

                distinctResults.ForEach(marionLease => MarionMineralAccounts.Add(marionLease));
            }

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
                foreach (var marionLease in marionLeases)
                {
                    marionRRC = marionLease.RRC;
                    marionLeaseNumber = marionLease.LeaseNumber;

                    var populatedLease = TranslateFrom_mMarionLeaseTo_mLease(marionLease, MarionOperators);

                    if (marionRRC != oldMarionRRC) populatedLease.LeaseNameWag=marionLease.LeaseName;

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

                    // after getting the primaryLeaseKey update tblWell with the matching 
                    if (!string.IsNullOrEmpty(rrcOperId))
                    {
                        string sql = $"UPDATE tblWell SET LeaseID = @LeaseID WHERE RrcLease = @rrc;";
                        int rowsAffected = db.Execute(sql, new { rrc = formattedRRC, LeaseID = primaryLeaseKey });
                    }

                    var populatedCadLease = TranslateFrom_mMarionLeaseTo_mCadLease(marionLease);
                    populatedCadLease.LeaseId = (long)primaryLeaseKey;
                    db.Insert<mCadLease>(populatedCadLease);

                    //var marionTracts = MarionMineralAccounts.Where(a => a.RRC == marionLease.RRC).ToList();
                    var tractId = 0;
                    if (marionLeaseNumber != oldMarionLeaseNumber && marionRRC == oldMarionRRC) ++tractId;
                    if (marionLeaseNumber != oldMarionLeaseNumber && marionRRC != oldMarionRRC) tractId = 1;
                    if (marionLeaseNumber != oldMarionLeaseNumber)
                    {
                        var populatedTract = TranslateFrom_mMarionLeaseTo_mTract(tractId);
                        populatedTract.LeaseID = (long)primaryLeaseKey;
                        populatedTract.PropID = vmProperty.MineralPropertyIdMap[marionLease.LeaseNumber];

                        var tractAcres = MarionMineralAccounts.Where(t => t.LeaseNumber == marionLease.LeaseNumber).FirstOrDefault().Acres; //tract acres
                        var leaseAcres = (from m in MarionMineralAccounts where m.RRC == marionLease.RRC select m.Acres).Sum();//sum of tract acres in lease
                        populatedTract.LeasePct = Math.Round(leaseAcres != 0 ? tractAcres / leaseAcres : 0.0, 3);
                        //Note!! LeasePct needs to be inserted into tblAccount.Legal ??????? how? parse legal and re-build?

                        db.Insert<mTract>(populatedTract);
                    }
                    oldMarionRRC = marionRRC;
                    oldMarionLeaseNumber = marionLeaseNumber;
                }

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                LeaseUploadEnabled = false;
                MessageBox.Show($"Finished uploading {MarionMineralAccounts.Count()} leases");

                Messenger.Default.Send<LeaseFinishedMessage>(new LeaseFinishedMessage());
            }

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

            cadLease.CadLeaseId = marionLease.LeaseNumber.ToString();

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
            lease.LeaseNameWag = marionLease.LeaseName.Trim();

            var parsers = new RrcParser();
            var marionRrc = parsers.GetRRCnumberFromImportRRCstring(marionLease.RRC);
            lease.RrcLease = marionRrc;
            lease.LeaseNameWag = marionLease.LeaseName;

            //lease.LeaseOprID = int.Parse(OperatorRrcDataMap[lease.RrcLease]);
            //long rrcLeaseInt = 0;
            //if (vmAgentAndOperator.CrwOperRrcIDToNameIdMap.ContainsKey(lease.RrcLease))
            //    rrcLeaseInt = vmAgentAndOperator.CrwOperRrcIDToNameIdMap[lease.RrcLease];
            //lease.LeaseOprID = (int)rrcLeaseInt;

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
