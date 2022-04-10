using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
using System.Globalization;

namespace MarionUpload.ViewModels
{
    public class vmAccount : ViewModelBase
    {
        private const string UpdateByDefault = "MPW";
        private bool accountImportEnabled = true;
        private bool accountUploadEnabled = false;

        //public int ProgressBarUpLoadMarionAccountsMinimumValue
        //{
        //    get => ProgressBarUpLoadMarionAccountsMinimumValue;
        //    private set
        //    {
        //        ProgressBarUpLoadMarionAccountsMinimumValue = value;
        //        RaisePropertyChanged(nameof(ProgressBarUpLoadMarionAccountsMinimumValue));
        //    }
        //}
        //public int ProgressBarUpLoadMarionAccountsMaximumValue
        //{
        //    get => ProgressBarUpLoadMarionAccountsMaximumValue;
        //    private set
        //    {
        //        ProgressBarUpLoadMarionAccountsMaximumValue = value;
        //        RaisePropertyChanged(nameof(ProgressBarUpLoadMarionAccountsMaximumValue));
        //    }
        //}
        //public int ProgressBarUpLoadMarionAccountsCurrentValue
        //{
        //    get => ProgressBarUpLoadMarionAccountsMaximumValue;
        //    private set
        //    {
        //        ProgressBarUpLoadMarionAccountsMaximumValue = value;
        //        RaisePropertyChanged(nameof(ProgressBarUpLoadMarionAccountsMaximumValue));
        //    }
        //}

        public ICommand CommandImportAccounts => new RelayCommand(OnImportAccounts);
        public ICommand CommandUploadAccounts => new RelayCommand(OnUploadAccounts);

        public bool AccountImportEnabled
        {
            get => accountImportEnabled; set
            {
                accountImportEnabled = value;
                RaisePropertyChanged(nameof(AccountImportEnabled));
            }
        }
        public bool AccountUploadEnabled
        {
            get => accountUploadEnabled; set
            {
                accountUploadEnabled = value;
                RaisePropertyChanged(nameof(AccountUploadEnabled));
            }
        }

        public List<mAccount> AccountList { get; set; }

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
                                                        " Protest, DecimalInterest, AccountNumber, Juris1MarketValue, [AccountSequence]" +
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
                //ProgressBarUpLoadMarionAccountsMinimumValue = 0;
                //ProgressBarUpLoadMarionAccountsMaximumValue = MarionAccounts.Count();
                //ProgressBarUpLoadMarionAccountsCurrentValue = 0;

                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    AccountList = new List<mAccount>();
                    foreach (var _marionAccount in MarionAccounts)
                    {
                        //++ProgressBarUpLoadMarionAccountsCurrentValue;
                        var populatedAccount = TranslateFrom_mMarionAccountTo_mAccount(_marionAccount);
                        var primaryKey = db.Insert<mAccount>(populatedAccount);
                        AccountList.Add(populatedAccount);

                        var populatedAccountPrYr = ConvertFromAccountToAccountPrYr(populatedAccount);

                        var populatedCadAccount = TranslateFrom_mMarionAccountTo_mCadAccount(_marionAccount, (long)primaryKey);
                        var primaryCadAccountKey = db.Insert<mCadAccount>((mCadAccount)populatedCadAccount);
                    }

                    // copy tblAccount rows just inserted into tlkp
                    string sqlStringForTlkpAccountPrYr1 =
                        "insert INTO tlkpAccountPrYr " +
                        "(CadParcelId,PropID,CadRealID,NameID,AcctLegal,ChgCode,SeqNmbr,PctProp,PctType" +
                        ",ValImpAppr,ValImpHsNo,ValImpHsYes,ValLandAppr,ValLandHsNo,ValLandHsYes,ValLandMkt,ValLandAg,ValPrsnlAppr,ValPrsnlHsNo" +
                        ",ValPrsnlHsYes,ValMnrlCalc,ValMnrlAppr,ValNpmAppr,ValAcctCur,ValAcctNtc,ValAcctLock,ValueLocked,NtcDate,LastProtDate" +
                        ",ValAcctCrt,valacctPrYr,AcctValPrYr,ValAcctPryr5,CrtDate,Stat_YN,Protest_YN,ProtestResolved_YN,Supp_YN,UpdateDate" +
                        ",UpdateBy,Memo,ProtStat,ProtStatDate,ProtDate,ProtCause,CrtStat,ResStat,ResStatDate,BatchProtest_YN" +
                        ",BatchWithdraw_YN,ConveyTransactionNo,Prc,crtNote,Cad,ValidationNote,delflag,PTDcode,GeoRef,corr_yn" +
                        ",AcctValPryr5,division)" +
                        "SELECT " +
                        "CadParcelId,PropID,CadRealID,NameID,AcctLegal,ChgCode,SeqNmbr,PctProp,PctType" +
                        ",ValImpAppr,ValImpHsNo,ValImpHsYes,ValLandAppr,ValLandHsNo,ValLandHsYes,ValLandMkt,ValLandAg,ValPrsnlAppr,ValPrsnlHsNo" +
                        ",ValPrsnlHsYes,ValMnrlCalc,ValMnrlAppr,ValNpmAppr,ValAcctCur,ValAcctNtc,ValAcctLock,ValueLocked,NtcDate,LastProtDate" +
                        ",ValAcctCrt,valacctPrYr,AcctValPrYr,ValAcctPryr5,CrtDate,Stat_YN,Protest_YN,ProtestResolved_YN,Supp_YN,UpdateDate" +
                        ",UpdateBy,Memo,ProtStat,ProtStatDate,ProtDate,ProtCause,CrtStat,ResStat,ResStatDate,BatchProtest_YN" +
                        ",BatchWithdraw_YN,ConveyTransactionNo,Prc,crtNote,Cad,ValidationNote,delflag,PTDcode,GeoRef,corr_yn" +
                        ",AcctValPryr5,division " +
                        "from tblAccount where tblAccount.PropID = PropID and tblAccount.NameID = NameID " +
                        "and tblAccount.Cad='MAR'";

                    var affectedRows = db.Execute(sqlStringForTlkpAccountPrYr1);

                    // update tlkpAccountPrYr with values in tblAccount
                    string sqlStringForTlkpAccountPrYr2 =
                        "UPDATE tlkpAccountPrYr " +
                        "SET tlkpAccountPrYr.ValAcctCrt = cur.ValAcctCur " +
                        "From tlkpAccountPrYr pr inner join tblAccount cur " +
                        "on pr.NameID = cur.NameID and pr.PropID = cur.PropID " +
                        "where cur.cad = 'MAR'";

                    var affectedRows2 = db.Execute(sqlStringForTlkpAccountPrYr2);

                    foreach (var acct in AccountList)
                    {
                        decimal sumOfOwnerCadValues = 0;
                        foreach (var innerAcct in AccountList)
                        {
                            if (innerAcct.NameID == acct.NameID) sumOfOwnerCadValues += innerAcct.ValAcctCur;
                        }

                        var populatedAprslAdmin = TranslateFrom_mOwnerTo_mAprslAdmin(acct, sumOfOwnerCadValues);

                        try
                        {
                            // test to see if populatedAprslAdmin already exists.
                            var adminQueryString = $"use wagapp2_2021_Marion Select * from tblAprslAdmin a where a.Year = '2022' and a.NameID = {acct.NameID} and a.CadID='MAR' ";
                            var affectedRows3 = db.Query(adminQueryString).Count();
                            if (affectedRows3 == 0)
                            {
                                var primaryAprslAdminKey = db.Insert<mAprslAdmin>(populatedAprslAdmin);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error in inserting into Admin Appraisal table {ex.Message}");
                        }
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
            acctPrYr.PctProp = (float)Math.Round(populatedAccount.PctProp, 9);
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

            //account.NameID= get this from a mapping of Marion OwnerNumber to Wag tblName.NameId
            //account.PropID= get this from a mapping of Marion LeaseNumber to Wag tblProperty.PropId for Mineral Properties
            //account.PropID= get this from a mapping of Marion OwnerNumber and LeaseNumber to Wag tblProperty.PropId for Non-Mineral Properties

            account.Stat_YN = true;
            account.UpdateBy = UpdateByDefault;
            account.UpdateDate = DateTime.Now;
            account.Cad = "MAR";

            account.PctProp = (float)(_marionAccount.SPTBCode.Trim() == "G1" || _marionAccount.SPTBCode.Trim() == "XV" ?
                _marionAccount.DecimalInterest * 10.0 : 1.0);
            account.PctProp = (float)Math.Round(account.PctProp, 9);
            account.PctType = ConvertInterestType(_marionAccount);

            account.Protest_YN = _marionAccount.Protest == "P";
            account.PTDcode = _marionAccount.SPTBCode.Trim();

            if (account.PTDcode == "G1" || account.PTDcode == "XV")
            {
                if (vmProperty.MineralPropertyIdMap.ContainsKey(_marionAccount.LeaseNumber))
                    account.PropID = vmProperty.MineralPropertyIdMap[_marionAccount.LeaseNumber];
            }
            else
            {
                Tuple<int, int> keyTuple = new Tuple<int, int>(_marionAccount.OwnerNumber, _marionAccount.LeaseNumber);
                if (vmProperty.PersonalPropertyIdMap.ContainsKey(keyTuple))
                    account.PropID = vmProperty.PersonalPropertyIdMap[keyTuple];
            }
            account.NameID = vmOwner.NameIdMap[_marionAccount.OwnerNumber];

            if (vmProperty.PropertyLegalMap.ContainsKey((int)account.PropID))
                account.AcctLegal = vmProperty.PropertyLegalMap[(int)account.PropID];
            var interestDecimalsFormatted = _marionAccount.DecimalInterest.ToString("F9", CultureInfo.InvariantCulture);
            //var interestInfo = " ( " + interestDecimalsFormatted + " - " + account.PctType.ToString() + ")";
            var interestInfo = " ( " + interestDecimalsFormatted + ")";
            if (_marionAccount.SPTBCode.Trim() == "G1" || _marionAccount.SPTBCode.Trim() == "XV") account.AcctLegal += interestInfo;

            account.ValAcctCur = _marionAccount.Juris1MarketValue;
            account.valacctPrYr = _marionAccount.Juris1MarketValue;
            account.AcctValPrYr = _marionAccount.Juris1MarketValue;

            string divString = _marionAccount.SPTBCode.Trim() == "G1" || _marionAccount.SPTBCode.Trim() == "XV" ? "M" : "U";
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
