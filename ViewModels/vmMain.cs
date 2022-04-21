using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MarionUpload.Helpers;
using MarionUpload.Messages;
using MarionUpload.Models;
using MarionUpLoad.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace MarionUpload.ViewModels
{
    public class vmMain : INotifyPropertyChanged
    {
        private bool _isStarted;
        private int _currentStep;

        private bool _agentsEnabled = true;
        private bool _ownersEnabled = true;
        private bool _propertiesEnabled;
        private bool _accountsEnabled;
        private bool _unitsEnabled;
        private bool _leasesEnabled;

        private const int AGENT_TAB_INDEX = 0;
        private const int OWNER_TAB_INDEX = 1;
        private const int PROPERTY_TAB_INDEX = 2;
        private const int ACCOUNT_TAB_INDEX = 3;
        private const int UNIT_TAB_INDEX = 4;
        private const int LEASE_TAB_INDEX = 5;
        private const int FINISHED_TAB_INDEX = 6;

        public bool IsStarted { get => _isStarted; set { _isStarted = value; Raise(nameof(IsStarted)); } }
        public int CurrentStep { get => _currentStep; set { _currentStep = value; Raise(nameof(CurrentStep)); } }

        public ICommand CommandStartImportWizard => new RelayCommand(OnStartImportWizard);
        public ICommand CommandStartExportWizard => new RelayCommand(OnStartExportWizard);

        public ObservableCollection<mMarionImport> MarionExportRows { get; set; }
        public ObservableCollection<mAccount> tblAccountRows { get; set; }
        public ObservableCollection<mCadAccount> tblCadAccountRows { get; set; }
        public ObservableCollection<mOwner> tblNameRows { get; set; }
        public ObservableCollection<mCadOwner> tblCadOwnerRows { get; set; }
        public ObservableCollection<mProperty> tblPropertyRows { get; set; }
        public ObservableCollection<mCadProperty> tblCadPropertyRows { get; set; }
        public ObservableCollection<mTract> tblTractRows { get; set; }
        public ObservableCollection<mLease> tblLeaseRows { get; set; }
        public ObservableCollection<mCadLease> tblCadLeaseRows { get; set; }

        private void OnStartExportWizard()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                //create an empty collection of type AbMarionImport table that will later be saved as the export file
                MarionExportRows = new ObservableCollection<mMarionImport>();

                tblAccountRows = new ObservableCollection<mAccount>();
                tblCadAccountRows = new ObservableCollection<mCadAccount>();

                tblNameRows = new ObservableCollection<mOwner>();
                tblCadOwnerRows = new ObservableCollection<mCadOwner>();

                tblPropertyRows = new ObservableCollection<mProperty>();
                tblCadPropertyRows = new ObservableCollection<mCadProperty>();

                tblTractRows = new ObservableCollection<mTract>();
                tblLeaseRows = new ObservableCollection<mLease>();
                tblCadLeaseRows = new ObservableCollection<mCadLease>();

                //import tblAccount where Cad='MAR'
                var acctSqlString = $"select * from tblAccount a where a.Cad='MAR' ";
                var accountRows = db.Query<mAccount>(acctSqlString);
                var translatedAccountRow = new mMarionExport();
                foreach (mAccount accountRow in accountRows)
                {
                    //      translate the appropriate columns to AbMarionImport type columns. Include 
                    translatedAccountRow = TranslateAccountRowToMarionImportRow(accountRow);
                    //      add (insert) the translated columns to accountRow. Repeat this for tblCadAccount
                    //MarionExportRows.Add(translatedAccountRow);
                }

                //import tblName where Cad='MAR'
                var nameSqlString = $"select * from tblName n join tblCadOwners c on n.NameID = c.NameID where c.CadID='MAR'";
                var nameRows = db.Query<mOwner>(nameSqlString);

                foreach (mAccount accountRow in accountRows)
                {
                    //      using accountRow.NameID find the corresponding row in tblName.
                    var nameRow = (from n in tblNameRows where accountRow.NameID == n.NameID select n).FirstOrDefault();
                    //          translate the appropriate columns in tblName to AbMarionImport columns.
                    var translatedNameRow = TranslateNameRowToMarionImportRow(nameRow);
                    //          add (update) the translated columns to accountRow. Repeat this for tblCadOwners
                }

                var propertySqlString = $"select * from tblProperty p join tblCadProperty c on p.PropID = c.PropID where c.CadID='MAR'";
                var propertyRows = db.Query<mProperty>(nameSqlString);

                foreach (mAccount accountRow in accountRows)
                {
                    var propertyRow = (from n in tblNameRows where accountRow.NameID == n.NameID select n).FirstOrDefault();
                    //var translatedNameRow = TranslateNameRowToMarionImportRow(nameRow);
                }
                //      using accountRow.PropID find the corresponding row in tblProperty
                //          translate the appropriate columns in tblProperty to AbMarionImport columns.
                //          add (update) the translated columns to accountRow. Repeat this for tblCadProperty.

                var unitPropertySqlString = $"select * from tblUnitProperty u join tlkpCadUnit c on u.UnitID = c.UnitID where c.CadID='MAR'";
                var unitPropertyRows = db.Query<mUnitProperty>(unitPropertySqlString);
                //      using accountRow.PropID and UnitID to  find the corresponding row in tblUnitProperty
                //          translate the appropriate columns in tblUnitProperty to AbMarionImport columns.
                //          add (update) the translated columns to accountRow.

                var tractSqlString = $"select * from tblTract t where t.CadID='MAR' ";
                var tractRows = db.Query<mTract>(tractSqlString);
                //      using accountRow.PropID and TractPropID to find the corresponding row in tblTract
                //          translate the appropriate columns in tblTract to AbMarionImport columns.
                //          add (update) the translated columns to accountRow.           
                //
                //      using accountRow.PropID find the corresponding rows in tblTract and cycle through the tracts (usually only one)
                //          translate the appropriate columns in tblTract to AbMarionImport columns.
                //          add (update) the translated columns to accountRow.

                var leaseSqlString = $"select * from tblLease l join tblCadLease c on l.LeaseID = c.LeaseID where c.CadID='MAR'";
                var leaseRows = db.Query(leaseSqlString);
                //      using LeaseID to find the corresponding row in tblLease
                //          translate the appropriate columns in tblLease to AbMarionImport columns.
                //          add (update) the translated columns to accountRow. Repeat this for tblCadLease.           
                //

            }
            //MarionExportRows = null;

            //tblAccountRows = null;
            //tblCadAccountRows = null;

            //tblNameRows = null;
            //tblCadOwnerRows = null;

            //tblPropertyRows = null;
            //tblCadPropertyRows = null;

            //tblTractRows = null;
            //tblLeaseRows = null;
            //tblCadLeaseRows = null;
        }

        private mMarionExport TranslateNameRowToMarionImportRow(mOwner owner)
        {
            var marionExportRow = new mMarionExport();

            marionExportRow.OwnerName = owner.NameSortCad;
            marionExportRow.StreetAddress = owner.Mail1;
            marionExportRow.CityStateZip = owner.MailCi + ", " + owner.MailSt + owner.MailZip;
            //marionExportRow.InCareOf =                                    //from owner?                                   


            //marionExportRow.GeoRef =                                      //this comes from CadAccountID

            //marionExportRow.OperatorName = owner.NameSortCad;             //this comes from lease.            
            //marionExportRow.RRC =                                         //this comes from lease.            
            //marionExportRow.LeaseNumber =                                 //this comes from lease.             
            //marionExportRow.acres =                                       //this comes from lease
            //marionExportRow.AbsoluteExemptionCode =                       //this comes from lease.
            //marionExportRow.LeaseName =                                   //this comes from lease. 
            //marionExportRow.OperatorName =                                //this comes from lease.     
            //marionExportRow.Description1 =                                //this comes from property or lease?   
            //marionExportRow.Description2 =                                //this comes from property or lease?
            //marionExportRow.YearLeaseStarted =                            //in don't know how to convert right now. tblWell??

            //marionExportRow.PropertyType =                                //this comes from property     
            //marionExportRow.SPTBCode = accountRow.PTDcode;                //this comes from property

            //marionExportRow.RenderedCode =                                //this has no conversion    
            //marionExportRow.SortCode =                                    //this has no conversion    

            //marionExportRow.OwnerNumber =                                 //in don't know how to convert right now.
            //marionExportRow.AgentNumber =                                 //in don't know how to convert right now.

            //marionExportRow.PollutionControlValue =                       //in don't know how to convert right now.    
            //marionExportRow.PreviousAccountNumber =                       //in don't know how to convert right now.
            //marionExportRow.PreviousAccountSequence =                     //in don't know how to convert right now.    
            //marionExportRow.PrivacyCode =                                 //in don't know how to convert right now.
            //marionExportRow.ComplianceCode =                              //in don't know how to convert right now.
            //marionExportRow.TCEQFlag =                                    //in don't know how to convert right now.

            //marionExportRow.NewImprovementPercent =                       //in don't know how to convert right now.
            return marionExportRow;
        }

        private mMarionExport TranslateAccountRowToMarionImportRow(mAccount accountRow)
        {
            var marionExportRow = new mMarionExport();
            
            //          don't forget to reverse:
            //          account.SeqNmbr = _marionAccount.AccountNumber.ToString() + " | " + _marionAccount.AccountSequence.ToString();
            int acctNum;
            bool res = int.TryParse(accountRow.SeqNmbr, out acctNum);
            marionExportRow.AccountNumber = res ? acctNum : 0;


            var dividerIndex = accountRow.SeqNmbr.IndexOf("|");
            string acctNmbrPart = accountRow.SeqNmbr.Substring(0, dividerIndex);
            string acctSeqPart = accountRow.SeqNmbr.Substring(dividerIndex);

            int acctNumbr; int acctSeq;
            bool res1 = int.TryParse(acctNmbrPart, out acctNumbr);
            bool res2 = int.TryParse(acctSeqPart, out acctSeq);
            marionExportRow.AccountNumber = res1 ? acctNumbr : 0;
            marionExportRow.AccountSequence = res2 ? acctSeq : 0; ;

            marionExportRow.CurrentTaxYear = 2022;

            marionExportRow.Protest = "";       // at start of season there are no protests.
            marionExportRow.DecimalInterest = (decimal)accountRow.PctProp;

            switch (accountRow.PctType)
            {
                case 'R':
                    marionExportRow.InterestType = 1;
                    break;
                case 'P':
                    marionExportRow.InterestType = 2;
                    break;
            }

            marionExportRow.InterestType = accountRow.PctType;

            //marionExportRow.OperatorName = "";
            //marionExportRow.OwnerName =
            //marionExportRow.OwnerNumber =
            //marionExportRow.RRC =
            //marionExportRow.LeaseNumber =
            //marionExportRow.RenderedCode =
            //marionExportRow.PropertyType =

            //marionExportRow.YearLeaseStarted =

            //marionExportRow.SPTBCode = accountRow.PTDcode;
            //marionExportRow.AgentNumber = 
            //marionExportRow.SortCode =

            //marionExportRow.LeaseName =
            //marionExportRow.OperatorName =
            //marionExportRow.Description1 =
            //marionExportRow.Description2 =
            //marionExportRow.InCareOf =
            //marionExportRow.StreetAddress =
            //marionExportRow.CityStateZip =

            //marionExportRow.acres =
            //marionExportRow.AbsoluteExemptionCode =
            //marionExportRow.GeoRef =
            //marionExportRow.PollutionControlValue =
            //marionExportRow.PreviousAccountNumber =
            //marionExportRow.PreviousAccountSequence =
            //marionExportRow.PrivacyCode =
            //marionExportRow.ComplianceCode =
            //marionExportRow.TCEQFlag =

            //marionExportRow.NewImprovementPercent =

            return marionExportRow;
        }

        public bool AgentsEnabled { get => _agentsEnabled; set { _agentsEnabled = value; Raise(nameof(AgentsEnabled)); } }
        public bool OwnersEnabled { get => _ownersEnabled; set { _ownersEnabled = value; Raise(nameof(OwnersEnabled)); } }
        public bool PropertiesEnabled { get => _propertiesEnabled; set { _propertiesEnabled = value; Raise(nameof(PropertiesEnabled)); } }
        public bool AccountsEnabled { get => _accountsEnabled; set { _accountsEnabled = value; Raise(nameof(AccountsEnabled)); } }
        public bool UnitsEnabled { get => _unitsEnabled; set { _unitsEnabled = value; Raise(nameof(UnitsEnabled)); } }
        public bool LeasesEnabled { get => _leasesEnabled; set { _leasesEnabled = value; Raise(nameof(LeasesEnabled)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public vmMain()
        {
            Messenger.Default.Register<AgentFinishedMessage>(this, message =>
            {
                AgentsEnabled = false;
                OwnersEnabled = true;
                Navigate(OWNER_TAB_INDEX);
            });

            Messenger.Default.Register<OwnerFinishedMessage>(this, message =>
            {
                OwnersEnabled = false;
                PropertiesEnabled = true;
                Navigate(PROPERTY_TAB_INDEX);
            });

            Messenger.Default.Register<PropertiesFinishedMessage>(this, message =>
            {
                PropertiesEnabled = false;
                AccountsEnabled = true;
                Navigate(ACCOUNT_TAB_INDEX);
            });

            Messenger.Default.Register<AccountsFinishedMessage>(this, message =>
            {
                AccountsEnabled = false;
                UnitsEnabled = true;
                Navigate(UNIT_TAB_INDEX);
            });

            Messenger.Default.Register<UnitsFinishedMessage>(this, message =>
            {
                UnitsEnabled = false;
                LeasesEnabled = true;
                Navigate(LEASE_TAB_INDEX);
            });

            Messenger.Default.Register<LeaseFinishedMessage>(this, message =>
            {
                LeasesEnabled = false;
                Navigate(FINISHED_TAB_INDEX);
            });
        }

        private void Navigate(int nextIndex)
        {
            CurrentStep = nextIndex;
        }

        private void Raise([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnStartImportWizard()
        {
            // Clean up all Tables
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
            DeleteAllMarionCountyData();
            IsStarted = true;
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

        }

        private void DeleteAllMarionCountyData()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    // delete accounts
                    db.Execute("Delete from tblAccount where Cad = 'MAR'");
                    db.Execute("Delete from tblAprslAdmin where CadID = 'MAR'");
                    db.Execute("Delete n from tblName n, tblCadOwners c where n.NameID = c.NameID and c.CadID = 'MAR'");
                    db.Execute("Delete from tblCadOwners where CadID = 'MAR'");

                    db.Execute("Delete s from tblPersonal s, tblProperty p, tblCadProperty c where s.PropId = p.PropId and  p.PropId = c.PropID and c.CadID = 'MAR'");
                    db.Execute("Delete p from tblProperty p, tblCadProperty c where p.PropId = c.PropID and c.CadID = 'MAR'");
                    db.Execute("Delete from tblCadProperty where CadID = 'MAR'");

                    db.Execute("Delete t from tblLease l, tblTract t, tblCadLease c where t.LeaseID = l.LeaseID and l.LeaseID = c.LeaseID and c.CadID = 'MAR'");
                    db.Execute("Delete l from tblLease l, tblCadLease c where l.LeaseID = c.LeaseID and c.CadID = 'MAR'");
                    db.Execute("Delete from tblCadLease where CadID = 'MAR'");

                    db.Execute($"delete tblUnitProperty from tblUnitProperty u join tlkpCadUnit c on u.UnitId=c.UnitID where c.CadId='MAR'");

                    db.Execute($"delete tblName from tblName n join AbMarionOperatorsFromCRW c on n.OperRrcID=c.OperRrcID");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Uploading Owner Data -> {ex}");
                Messenger.Default.Send<OwnerFinishedMessage>(new OwnerFinishedMessage());
            }
        }
    }
}
