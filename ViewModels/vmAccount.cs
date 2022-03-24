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
        private const string UpdateByDefault = "MPW";
        private bool accountImportEnabled = true;
        private bool accountUploadEnabled = false;

        public ICommand CommandImportAccounts => new RelayCommand(OnImportAccounts);
        public ICommand CommandUploadAccounts => new RelayCommand(OnUploadAccounts);

        public bool AccountImportEnabled { get => accountImportEnabled; set { accountImportEnabled = value; RaisePropertyChanged(nameof(AccountImportEnabled)); } }
        public bool AccountUploadEnabled { get => accountUploadEnabled; set { accountUploadEnabled = value; RaisePropertyChanged(nameof(AccountUploadEnabled)); } }

        private void OnImportAccounts()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
            //CommandImportAccounts.CanExecute(false);
            SelectAccountDataFromImportTable();
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

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
                var results = db.Query<mMarionAccount>("Select ImportID, OwnerNumber, LeaseNumber, InterestType, SPTBCode," +
                                                        " Protest, DecimalInterest, AccountNumber, Juris2MarketValue, [AccountSequence]" +
                                                        " from AbMarionImport"); ;

                var resultList = results.ToList();

                resultList.ForEach(marionAccount => MarionAccounts.Add(marionAccount));
            }
        }


        public ObservableCollection<mMarionAccount> MarionAccounts { get; private set; } = new ObservableCollection<mMarionAccount>();

        private void OnUploadAccounts()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    int currentNameId = 0;
                    int previousNameId = 0;
                    int currentPropId = 0;
                    int previousPropId = 0;
                    mAccount previousPopulatedAccount = new mAccount();
                    mAccountPrYr populatedAccountPrYr = new mAccountPrYr();
                    decimal sumOfOwnerCadValues = 0;
                    float sumOfAccountPctPropForThisProperty = 0;

                    foreach (var _marionAccount in MarionAccounts)
                    {
                        var populatedAccount = TranslateFrom_mMarionAccountTo_mAccount(_marionAccount);
                        var primaryKey = db.Insert<mAccount>(populatedAccount);
                        currentNameId = (int)populatedAccount.NameID;


                        populatedAccountPrYr = ConvertFromAccountToAccountPrYr(populatedAccount);
                        var priorPrimaryKey = db.Insert<mAccountPrYr>(populatedAccountPrYr);

                        if (priorPrimaryKey != primaryKey)
                        {
                            throw new Exception($"tblAccount and tlkpAccountPrYr are out of sync. PK tblAccount = {primaryKey} and PK tlkpAccountPrYr = {priorPrimaryKey}");                            
                        }

                        var populatedCadAccount = TranslateFrom_mMarionAccountTo_mCadAccount(_marionAccount, (long)primaryKey);
                        var primaryCadAccountKey = db.Insert<mCadAccount>((mCadAccount)populatedCadAccount);

                        if (currentNameId == previousNameId)
                        {
                            sumOfOwnerCadValues += populatedAccount.ValAcctCur;
                        }
                        else
                        {
                            var populatedAprslAdmin = TranslateFrom_mOwnerTo_mAprslAdmin(previousPopulatedAccount, sumOfOwnerCadValues);

                            try
                            {
                                var primaryAprslAdminKey = db.Insert<mAprslAdmin>(populatedAprslAdmin);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error in inserting into Admin Appraisal table {ex.Message}");
                            }
                        }

                        if (currentPropId == previousPropId)
                        {
                            sumOfAccountPctPropForThisProperty += populatedAccount.PctProp;
                        }
                        else
                        {
                            //calculate the AccoountPctProp for each account (the sum of the PctProp for each Account in this Property - should be 1.0)
                        }

                        previousNameId = currentNameId;
                        previousPopulatedAccount = populatedAccount;
                    }
                }

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                MessageBox.Show($"Finished uploading {MarionAccounts.Count()} accounts");

                Messenger.Default.Send<AccountsFinishedMessage>(new AccountsFinishedMessage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            }
        }

        private void insertTlkpAccountPrYr(mAccountPrYr marionAccount)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    string sqlQuery = "Insert Into tlkpAccountPrYr " +
                                      "(AcctID, AcctLegal, PctType, PctProp, Protest_YN, PTDcode, NameID, PropID, Cad, Stat_YN, " +
                                      "UpdateBy, UpdateDate, ValAcctCur, ValAcctCrt, valacctPrYr, AcctValPrYr, division) " +
                                      "Values" +
                                      "(@AcctID, @AcctLegal, @PctType, @PctProp, @Protest_YN, @PTDcode, @NameID, @PropID, @Cad, @Stat_YN, " +
                                      "@UpdateBy, @UpdateDate, @ValAcctCur, @ValAcctCrt, @valacctPrYr, @AcctValPrYr, @division)";

                    int rowsAffected = db.Execute(sqlQuery, marionAccount);
                }
            }
            catch
            {
                MessageBox.Show($"Insert Failed on account with NameID: {marionAccount.NameID}");
            }
        }

        private mAccountPrYr ConvertFromAccountToAccountPrYr(mAccount populatedAccount)
        {
            var acctPrYr = new mAccountPrYr();
            acctPrYr.AcctLegal = populatedAccount.AcctLegal;
            acctPrYr.PctType = populatedAccount.PctType;
            acctPrYr.PctProp = populatedAccount.PctProp;
            acctPrYr.Protest_YN = populatedAccount.Protest_YN;
            acctPrYr.PTDcode = populatedAccount.PTDcode;
            acctPrYr.NameID = populatedAccount.NameID;
            acctPrYr.PropID = populatedAccount.PropID;
            acctPrYr.Cad = populatedAccount.Cad;
            acctPrYr.Stat_YN = populatedAccount.Stat_YN;
            acctPrYr.UpdateBy = populatedAccount.UpdateBy;
            acctPrYr.UpdateDate = populatedAccount.UpdateDate;
            acctPrYr.ValAcctCur = populatedAccount.ValAcctCur;
            acctPrYr.ValAcctCrt = populatedAccount.ValAcctCrt;
            acctPrYr.valacctPrYr = populatedAccount.valacctPrYr;
            acctPrYr.AcctValPrYr = populatedAccount.AcctValPrYr;
            acctPrYr.division = populatedAccount.division;

            return acctPrYr;
        }

        private mCadAccount TranslateFrom_mMarionAccountTo_mCadAccount(mMarionAccount marionAccount, long primaryAccountKey)
        {
            var cadAccount = new mCadAccount();
            cadAccount.AcctID = (int)primaryAccountKey;
            cadAccount.CadID = "MAR";
            cadAccount.CadAcctID = BuildCadAccountId(marionAccount);
            cadAccount.Lock_YN = false;
            cadAccount.ExportCd = "";
            cadAccount.ExportDate = DateTime.Now;
            cadAccount.delflag = false;
            cadAccount.CadAcctID_pre = "";
            return cadAccount;
        }

        private string BuildCadAccountId(mMarionAccount marionAccount)
        {
            var cadAccountId = marionAccount.OwnerNumber.ToString().PadLeft(7, '0') + "-" +
                               //ConvertInterestType(marionAccount) + "-" +
                               marionAccount.InterestType.ToString().Trim() + "-" +
                               marionAccount.LeaseNumber.ToString().PadLeft(7, '0');
            return cadAccountId;
        }

        private mAccount TranslateFrom_mMarionAccountTo_mAccount(mMarionAccount _marionAccount)
        {
            var account = new mAccount();

            account.Stat_YN = true;
            account.UpdateBy = UpdateByDefault;
            account.UpdateDate = DateTime.Now;
            account.Cad = "MAR";

            account.PctProp = _marionAccount.DecimalInterest;
            account.PctType = ConvertInterestType(_marionAccount);

            account.Protest_YN = _marionAccount.Protest == "P";
            account.PTDcode = (_marionAccount.SPTBCode).Trim();

            account.PropID = vmProperty.PropertyIdMap[_marionAccount.LeaseNumber];
            account.NameID = vmOwner.NameIdMap[_marionAccount.OwnerNumber];

            account.AcctLegal = vmProperty.PropertyLegalMap[(int)account.PropID];
            var interestInfo = " (" + _marionAccount.DecimalInterest.ToString() + " - " + _marionAccount.InterestType.ToString() + ")";
            if (_marionAccount.SPTBCode.Trim() == "G1") account.AcctLegal += interestInfo;

            account.ValAcctCur = _marionAccount.Juris2MarketValue;
            account.ValAcctCrt = _marionAccount.Juris2MarketValue;
            //account.AcctValPrYr = _marionAccount.Juris2MarketValue;
            //account.valacctPrYr = _marionAccount.Juris2MarketValue;

            string divString = _marionAccount.SPTBCode.Trim() == "G1" ? "M" : "U";
            account.division = char.Parse(divString.Substring(0, 1));

            return account;
        }


        private mAprslAdmin TranslateFrom_mOwnerTo_mAprslAdmin(mAccount populatedAccount, decimal sumOfOwnerCadValues)
        {
            var aprslAdmin = new mAprslAdmin();

            aprslAdmin.Year = "2022";
            aprslAdmin.NameID = (int)populatedAccount.NameID;
            aprslAdmin.CadID = "MAR";

            aprslAdmin.ApprValCad = sumOfOwnerCadValues;
            aprslAdmin.UpdateBy = "MPW";
            aprslAdmin.UpdateDate = DateTime.Now;

            return aprslAdmin;
        }

        private static char ConvertInterestType(mMarionAccount _marionAccount)
        {
            char _intType;
            switch (_marionAccount.InterestType)
            {
                case 0:
                    _intType = 'U';
                    break;
                case 1:
                    _intType = 'R';
                    break;
                case 2:
                    _intType = 'O';
                    break;
                case 3:// Not used in Marion Import File
                    _intType = 'W';
                    break;
                case 4:
                    _intType = 'W';
                    break;
                case 5:// Not used in Marion Import File
                    _intType = 'W';
                    break;
                default:
                    _intType = 'U';// Should not occur in Marion Import File i.e. _marionAccount.InterestType should never be blank
                    break;
            }
            return _intType;
        }
    }
}
