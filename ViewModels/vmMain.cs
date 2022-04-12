using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MarionUpload.Helpers;
using MarionUpload.Messages;
using MarionUpload.Models;

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
        public ICommand CommandStartWizard => new RelayCommand(OnStartWizard);

        public bool AgentsEnabled { get => _agentsEnabled; set { _agentsEnabled = value; Raise(nameof(AgentsEnabled)); } }
        public bool OwnersEnabled { get => _ownersEnabled; set  { _ownersEnabled = value; Raise(nameof(OwnersEnabled)); } }
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

        private void OnStartWizard()
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
