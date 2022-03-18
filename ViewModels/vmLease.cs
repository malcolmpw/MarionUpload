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

namespace MarionUpload.ViewModels
{
    public class vmLease : ViewModelBase
    {
        private const string UpdateByDefault = "MPW";
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool _leaseImportEnabled = true;
        private bool _leaseUploadEnabled = false;

        public ICommand CommandImportLeases => new RelayCommand(OnImportLeases);
        public ICommand CommandUploadLeases => new RelayCommand(OnUploadLeases);

        public ObservableCollection<mMarionLease> MarionLeases { get; set; }

        public vmLease()
        {
            MarionLeases = new ObservableCollection<mMarionLease>();
        }

        private void OnImportLeases()
        {
            SelectLeaseDataFromMarionImportTable();
        }

        private void SelectLeaseDataFromMarionImportTable()
        {
            MarionLeases.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionLease>("Select RRC, Description1, Description2, LeaseNumber, LeaseName, OperatorName, SPTBCode " +
                                                     "from AbMarionImport where SPTBCode = 'G1' order by LeaseName");

                var distinctResults = results.Distinct(new LeaseComparer()).ToList();

                distinctResults.ForEach(marionLease => MarionLeases.Add(marionLease));
            }

            LeaseImportEnabled = false;
            LeaseUploadEnabled = true;
        }

        private void OnUploadLeases()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                foreach (var marionLease in MarionLeases)
                {
                    var populatedLease = TranslateFrom_mMarionLeaseTo_mLease(marionLease);
                    var primaryLeaseKey = db.Insert<mLease>(populatedLease);

                    var populatedCadLease = TranslateFrom_mMarionLeaseTo_mCadLease(marionLease);
                    populatedCadLease.LeaseId = primaryLeaseKey;
                    db.Insert<mCadLease>(populatedCadLease);

                    var populatedTract = TranslateFrom_mMarionLeaseTo_mTract(marionLease);
                    populatedTract.LeaseId = primaryLeaseKey;
                    populatedTract.PropId = vmProperty.PropertyIdMap[marionLease.LeaseNumber];
                    db.Insert<mTract>(populatedTract);
                }

                LeaseUploadEnabled = false;
                MessageBox.Show($"Finished uploading {MarionLeases.Count()} leases");

                Messenger.Default.Send<LeaseFinishedMessage>(new LeaseFinishedMessage());
            }

        }

        private mCadLease TranslateFrom_mMarionLeaseTo_mCadLease(mMarionLease marionLease)
        {
            var cadLease = new mCadLease();
            cadLease.CadId = "MAR";
            cadLease.CadLeaseId = marionLease.LeaseNumber;
           
            return cadLease;
        }

        private mTract TranslateFrom_mMarionLeaseTo_mTract(mMarionLease marionLease)
        {
            var tract = new mTract();
            tract.TractId = "00";
            tract.CadId = "MAR";
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
            lease.LeaseNameWag = marionLease.LeaseName;
                                 
            lease.Stat_YN = true;
            lease.StatBy = UpdateByDefault;
            lease.StatDate = DateTime.Now;
            lease.StatReason = "Import";
                        
            lease.UpdateBy = UpdateByDefault;
            lease.UpdateDate = DateTime.Now;

            return lease;

        }

       

        public bool LeaseImportEnabled { get => _leaseImportEnabled; set { _leaseImportEnabled = value; RaisePropertyChanged(nameof(LeaseImportEnabled)); } }
        public bool LeaseUploadEnabled { get => _leaseUploadEnabled; set { _leaseUploadEnabled = value; RaisePropertyChanged(nameof(LeaseUploadEnabled)); } }

  }

}
