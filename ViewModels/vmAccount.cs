using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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

namespace MarionUpload.ViewModels
{
    public class vmAccount : ViewModelBase
    {
        private bool accountImportEnabled = true;
        private bool accountUploadEnabled = false;

        public ICommand CommandImportAccounts => new RelayCommand(OnImportAccounts);
        public ICommand CommandUploadAccounts => new RelayCommand(OnUploadAccounts);

        public bool AccountImportEnabled { get => accountImportEnabled; set { accountImportEnabled = value; RaisePropertyChanged(nameof(AccountImportEnabled));  } }
        public bool AccountUploadEnabled { get => accountUploadEnabled; set { accountUploadEnabled = value; RaisePropertyChanged(nameof(AccountUploadEnabled));  } }


        private void OnImportAccounts()
        {
            //CommandImportAccounts.CanExecute(false);
            SelectAccountDataFromImportTable();

            AccountImportEnabled = false;
            AccountUploadEnabled = true;
        }

        private void SelectAccountDataFromImportTable()
        {
            //NOTE:! I queried AbMarionImport:
            // distinct Description1,Description2                       and got 646 rows.
            // distinct Description1,Description2,PropertyType          and got 647 rows.
            // distinct Description1,Description2,PropertyType,SPTBcode and got 652 rows.
            // I need all these fields so I am using the last query despite some (8) strange duplicates
            MarionAccounts.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionAccount>("Select ImportID, OwnerNumber, LeaseNumber, InterestType,SPTBCode,Protest,DecimalInterest,AccountNumber,[AccountSequence]"
                    + " from AbMarionImport"); ;

                var resultList = results.ToList();

                resultList.ForEach(marionAccount => MarionAccounts.Add(marionAccount));

            }


        }


        public ObservableCollection<mMarionAccount> MarionAccounts { get; private set; } = new ObservableCollection<mMarionAccount>();

        private void OnUploadAccounts()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                foreach (var _marionAccount in MarionAccounts)
                {
                    var populatedAccount = TranslateFrom_mMarionAccountTo_mAccount(_marionAccount);
                    var primaryKey = db.Insert<mAccount>(populatedAccount);

                    var populatedCadAccount = TranslateFrom_mMarionAccountTo_mCadAccount(_marionAccount);
                    var primaryCadAccountKey = db.Insert<mCadAccount>((mCadAccount)populatedCadAccount);
                }
            }

            MessageBox.Show($"Finished uploading {MarionAccounts.Count()} accounts");

            Messenger.Default.Send<AccountsFinishedMessage>(new AccountsFinishedMessage());
        }

        private mCadAccount TranslateFrom_mMarionAccountTo_mCadAccount(mMarionAccount marionAccount)
        {
            var cadAccount = new mCadAccount();
            cadAccount.CadID = "MAR";
            char _interestType = ConvertInterestType(marionAccount);
            cadAccount.CadAcctID = marionAccount.OwnerNumber.ToString().PadLeft(7, '0') + "-" +
                                   _interestType + "-" +
                                   marionAccount.LeaseNumber.ToString().PadLeft(7, '0');
            cadAccount.Lock_YN = false;
            cadAccount.ExportCd = "";
            cadAccount.ExportDate = DateTime.Now;
            cadAccount.delflag = false;
            cadAccount.CadAcctID_pre = "";
            return cadAccount;

            // ******* USE this for the tblCadAccount CadAccountId ******
            // return CadOwnerId.PadLeft(7, '0') + "-" + m.InterestType + "-" +
            //        CadPropId.PadLeft(7, '0');
        }

        private mAccount TranslateFrom_mMarionAccountTo_mAccount(mMarionAccount _marionAccount)
        {
            var account = new mAccount();
            account.PctProp = _marionAccount.DecimalInterest;
            account.Protest_YN = _marionAccount.Protest == "P";
            account.PTDcode = _marionAccount.SPTBCode;
            account.PctType = ConvertInterestType(_marionAccount);
            account.PropID = vmProperty.PropertyIdMap[_marionAccount.LeaseNumber];
            account.NameID = vmOwner.NameIdMap[_marionAccount.OwnerNumber];

            return account;
        }

        private static char ConvertInterestType(mMarionAccount _marionAccount)
        {
            char _intType;
            switch (_marionAccount.InterestType)
            {
                case 1:
                    _intType = 'R';
                    break;
                case 2:
                    _intType = 'O';
                    break;
                case 3:
                    _intType = 'W';
                    break;
                default:
                    _intType = 'U';
                    break;
            }
            return _intType;
        }
    }
}
