using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MarionUpload.Messages;

namespace MarionUpload.ViewModels
{
    public class vmMain : INotifyPropertyChanged
    {
        private bool _ownersEnabled = true;
        private bool _isStarted;
        private bool _propertiesEnabled;
        private bool _accountsEnabled;
        private bool _unitsEnabled;
        private int _currentStep;
        private bool _leasesEnabled;
        private const int OWNER_TAB_INDEX = 0;
        private const int PROPERTY_TAB_INDEX = 1;
        private const int ACCOUNT_TAB_INDEX = 2;
        private const int UNIT_TAB_INDEX = 3;
        private const int LEASE_TAB_INDEX = 4;
        private const int FINISHED_TAB_INDEX = 5;

        public bool IsStarted { get => _isStarted; set { _isStarted = value; Raise(nameof(IsStarted)); } }
        public ICommand CommandStartWizard => new RelayCommand(OnStartWizard);

        public bool OwnersEnabled { get => _ownersEnabled; set  { _ownersEnabled = value; Raise(nameof(OwnersEnabled)); } }
        public bool PropertiesEnabled { get => _propertiesEnabled; set { _propertiesEnabled = value; Raise(nameof(PropertiesEnabled)); } }
        public bool AccountsEnabled { get => _accountsEnabled; set { _accountsEnabled = value; Raise(nameof(AccountsEnabled)); } }
        public bool UnitsEnabled { get => _unitsEnabled; set { _unitsEnabled = value; Raise(nameof(UnitsEnabled)); } }
        public bool LeasesEnabled { get => _leasesEnabled; set { _leasesEnabled = value; Raise(nameof(LeasesEnabled)); } }

        public int CurrentStep { get => _currentStep; set { _currentStep = value; Raise(nameof(CurrentStep));  } }

        public event PropertyChangedEventHandler PropertyChanged;

        public vmMain()
        {
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
            IsStarted = true;
        }


    }
}
