﻿using Dapper;
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
using System.Text.RegularExpressions;

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

        public ICommand CommandImportLeases => new RelayCommand(OnImportLeases);
        public ICommand CommandUploadLeases => new RelayCommand(OnUploadLeases);

        public static IDictionary<string, long> OperatorNameIdMap { get; private set; }
        public List<string> OperatorNamesFromMarionImport { get; private set; }
        public vmLease()
        {
            MarionMineralAccounts = new ObservableCollection<mMarionLease>();
            MarionOperators = new ObservableCollection<mMarionOperator>();
        }

        private void OnImportLeases()
        {
            SelectOperatorDataFromMarionImportTableAndWagOwners();
            SelectMineralDataFromMarionImportTable();
        }

        private static void SelectOperatorDataFromMarionImportTableAndWagOwners()
        {
            MarionOperators.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var operatorResults = db.Query<mMarionOperator>(
                    "SELECT * from AbMarionOperators Where Active=1 order by OperatorName ");
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
                var results = db.Query<mMarionLease>("Select RRC, Description1, Description2, LeaseNumber, LeaseName, OperatorName, SPTBCode, Acres " +
                                                     "from AbMarionImport where SPTBCode = 'G1 ' or SPTBCode = 'XV ' order by LeaseName");

                var distinctResults = results.Distinct(new LeaseComparer()).ToList();
                //distinctResults = distinctResults.Where(x => x.SPTBCode == "G1" || x.SPTBCode == "XV").ToList();

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
                GetOperatorNamesFromMarionImport();
                foreach (var marionLease in marionLeases)
                {
                    var populatedLease = TranslateFrom_mMarionLeaseTo_mLease(marionLease,MarionOperators);
                    if (OperatorNamesFromMarionImport.Contains(marionLease.OperatorName)) populatedLease.LeaseOprID= 0;
                    var primaryLeaseKey = db.Insert<mLease>(populatedLease);

                    var populatedCadLease = TranslateFrom_mMarionLeaseTo_mCadLease(marionLease);
                    populatedCadLease.LeaseId = (long)primaryLeaseKey;
                    db.Insert<mCadLease>(populatedCadLease);

                    var marionTracts = MarionMineralAccounts.Where(a => a.RRC == marionLease.RRC).ToList();
                    var tractId = 0; var currentLeaseNumber = 0;
                    foreach (var marionTract in marionTracts)
                    {
                        if (currentLeaseNumber != marionTract.LeaseNumber)
                        {
                            currentLeaseNumber = marionTract.LeaseNumber;
                            tractId = 1;
                        }
                        else
                        {
                            tractId++;
                            if (tractId > 2) MessageBox.Show($"The current multi-tract lease(RRC) = {marionTract.RRC} and tractid = {marionTract.LeaseNumber} / {tractId}. ");

                        }

                        var populatedTract = TranslateFrom_mMarionLeaseTo_mTract(marionTract, tractId);
                        populatedTract.LeaseID = (long)primaryLeaseKey;
                        populatedTract.PropID = vmProperty.PropertyIdMap[marionTract.LeaseNumber];

                        var tractAcres = MarionMineralAccounts.Where(t => t.LeaseNumber == marionTract.LeaseNumber).FirstOrDefault().Acres; //tract acres
                        var leaseAcres = (from m in MarionMineralAccounts where m.RRC == marionTract.RRC select m.Acres).Sum();//sum of tract acres in lease
                        populatedTract.LeasePct = leaseAcres != 0 ? tractAcres / leaseAcres : 0.0;
                        db.Insert<mTract>(populatedTract);
                    }
                }

                // For all operators in AbMarionOperators change their Oper_YN in tblName to true.
                string sqlString = "update wagapp2_2021_Marion.dbo.tblName " +
                                   "set tblName.Oper_YN = o.OperatorFlag " +
                                   "from wagapp2_2021_Marion.dbo.AbMarionOperators o " +
                                   "inner join wagapp2_2021_Marion.dbo.tblName n " +
                                   "on o.CompanyID = n.NameID " +
                                   "where o.Active = 1";
                var affectedRows = db.Execute(sqlString);

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                LeaseUploadEnabled = false;
                MessageBox.Show($"Finished uploading {MarionMineralAccounts.Count()} leases");

                Messenger.Default.Send<LeaseFinishedMessage>(new LeaseFinishedMessage());
            }

        }

        private void InsertTracts(IDbConnection db, mMarionLease thisMarionLease, mLease thisLease, long thisLeaseId)
        {
            //   var marionTracts = MarionMineralAccounts.Where(l => l.SPTBCode.Trim() == "G1").Select(m => new { m.RRC, m.LeaseNumber }).Distinct().ToList();
            var marionMineralRows = MarionMineralAccounts.Where(a => a.SPTBCode.Trim() == "G1").GroupBy(m => new { m.RRC, m.LeaseNumber })
                .Select(group => group.FirstOrDefault())
                .ToList();

            var tractId = 0;
            var currentRRC = "";
            foreach (var marionMineralRow in marionMineralRows)
            {
                if (marionMineralRow.RRC != currentRRC)
                {
                    currentRRC = marionMineralRow.RRC;
                    tractId = 1;
                }
                else
                {
                    tractId++;
                    if (tractId > 2) MessageBox.Show($"The current multi-tract lease(RRC) = {marionMineralRow.RRC} and tractid = {marionMineralRow.LeaseNumber} / {tractId}. ");
                }

                var populatedTract = TranslateFrom_mMarionLeaseTo_mTract(marionMineralRow, tractId);
                populatedTract.LeaseID = (long)thisLeaseId;
                populatedTract.PropID = vmProperty.PropertyIdMap[thisMarionLease.LeaseNumber];

                var tractAcres = MarionMineralAccounts.Where(t => t.LeaseNumber == thisMarionLease.LeaseNumber).FirstOrDefault().Acres; //tract acres
                var leaseAcres = (from m in MarionMineralAccounts where m.RRC == thisMarionLease.RRC select m.Acres).Sum();//sum of tract acres in lease
                populatedTract.LeasePct = leaseAcres != 0 ? tractAcres / leaseAcres : 0.0;
                db.Insert<mTract>(populatedTract);
            }
        }

        private mCadLease TranslateFrom_mMarionLeaseTo_mCadLease(mMarionLease marionLease)
        {
            var cadLease = new mCadLease();
            cadLease.CadId = "MAR";
            cadLease.CadLeaseId = GetRRCnumberFromImportRRCstring(marionLease);

            return cadLease;
        }

        private mTract TranslateFrom_mMarionLeaseTo_mTract(mMarionLease marionLease, int tractId)
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
            lease.LeaseNameWag = GetRRCnumberFromImportRRCstring(marionLease);
            //var id = MarionOperators.Where(x => x.CompanyName == marionLease.OperatorName).FirstOrDefault().CompanyID;
            lease.LeaseOprID = 0;
            //(int)OperatorNameIdMap[marionLease.OperatorName];
            lease.LeaseOprID = 0;

            lease.Stat_YN = true;
            lease.StatBy = UpdateByDefault;
            lease.StatDate = DateTime.Now;
            lease.StatReason = "Import";

            lease.UpdateBy = UpdateByDefault;
            lease.UpdateDate = DateTime.Now;

            return lease;

        }

        private static string GetRRCnumberFromImportRRCstring(mMarionLease marionLease)
        {
            string rrc = marionLease.RRC.Trim();
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

        private void GetOperatorNamesFromMarionImport()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2017))
            {
                string operatorNamesQueryString = "Select distinct OperatorName from AbMarionImport where sptbCode='G1 ' order by OperatorName ";
                OperatorNamesFromMarionImport = db.Query<string>(operatorNamesQueryString).ToList();
            }
        }

        public bool LeaseImportEnabled { get => _leaseImportEnabled; set { _leaseImportEnabled = value; RaisePropertyChanged(nameof(LeaseImportEnabled)); } }
        public bool LeaseUploadEnabled { get => _leaseUploadEnabled; set { _leaseUploadEnabled = value; RaisePropertyChanged(nameof(LeaseUploadEnabled)); } }

    }

}
