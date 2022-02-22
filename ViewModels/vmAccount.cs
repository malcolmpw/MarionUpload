using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dapper;
using MarionUpload.Models;
using MarionDistributeImport.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper.Contrib.Extensions;

namespace MarionUpload.ViewModels
{
    public class vmAccount
    {
        const string ConnectionString = @"Data Source=WAGSQLSRV01\DEV;Initial Catalog=wagapp2_2021_Marion;Integrated Security=True;";
        public ICommand CommandImportAccounts => new RelayCommand(OnImportAccounts);
        public ICommand CommandUploadAccounts => new RelayCommand(OnUploadAccounts);

        private void OnImportAccounts()
        {
            SelectAccountDataFromImportTable();
        }

        private void SelectAccountDataFromImportTable()
        {
            //NOTE:! I queried AbMarionImport:
            // distinct Description1,Description2                       and got 646 rows.
            // distinct Description1,Description2,PropertyType          and got 647 rows.
            // distinct Description1,Description2,PropertyType,SPTBcode and got 652 rows.
            // I need all these fields so I am using the last query despite some (8) strange duplicates
            MarionAccounts.Clear();
            using (IDbConnection db = new SqlConnection(ConnectionString))
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
            return;  // skip insert for now until property and account tables , and tblCadOwner are ready to test

            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                foreach (var _marionAccount in MarionAccounts)
                {
                    var populatedAccount = TranslateFrom_mMarionAccountTo_mAccount(_marionAccount);
                    var primaryKey = db.Insert<mAccount>(populatedAccount);

                    var populatedCadAccount = TranslateFrom_mMarionAccountTo_mCadAccount(_marionAccount);
                    var primaryCadAccountKey = db.Insert<mCadAccount>((mCadAccount)populatedCadAccount);
                }
            }
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
