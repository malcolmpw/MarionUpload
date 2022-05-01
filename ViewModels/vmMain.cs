﻿using Dapper;
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
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using System.IO;
using System.Globalization;
using CsvHelper;

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

        public ObservableCollection<mMarionExport> MarionExportRows { get; set; }
        //public ObservableCollection<mAccount> tblAccountRows { get; set; }
        //public ObservableCollection<mCadAccount> tblCadAccountRows { get; set; }
        //public ObservableCollection<mOwner> tblNameRows { get; set; }
        //public ObservableCollection<mCadOwner> tblCadOwnerRows { get; set; }
        //public ObservableCollection<mProperty> tblPropertyRows { get; set; }
        //public ObservableCollection<mCadProperty> tblCadPropertyRows { get; set; }
        //public ObservableCollection<mTract> tblTractRows { get; set; }
        //public ObservableCollection<mLease> tblLeaseRows { get; set; }
        //public ObservableCollection<mCadLease> tblCadLeaseRows { get; set; }

        //public string ConversionRules;
        //public 

        private void OnStartExportWizard()
        {
            var writer = new StreamWriter(@"C:\Users\malcolm.wardlaw\Desktop\Marion Download\MARION CAD FINAL MINERAL DATA\MarionExportFromDatabase.csv");
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                //create an empty collection of type AbMarionImport table that will later be saved as the export file
                MarionExportRows = new ObservableCollection<mMarionExport>();
                //public static IDictionary<int, long> AccountToMap { get; private set; } = new Dictionary<int, long>();


        //tblAccountRows = new ObservableCollection<mAccount>();
        //tblCadAccountRows = new ObservableCollection<mCadAccount>();

        //tblNameRows = new ObservableCollection<mOwner>();
        //tblCadOwnerRows = new ObservableCollection<mCadOwner>();

        //tblPropertyRows = new ObservableCollection<mProperty>();
        //tblCadPropertyRows = new ObservableCollection<mCadProperty>();

        //tblTractRows = new ObservableCollection<mTract>();
        //tblLeaseRows = new ObservableCollection<mLease>();
        //tblCadLeaseRows = new ObservableCollection<mCadLease>();

        //import tblAccount where Cad='MAR'
        var acctSqlString = $"select * from tblAccount a where a.Cad='MAR' ";
                var accountRows = db.Query<mAccount>(acctSqlString);

                foreach (mAccount accountRow in accountRows)
                {
                    var marionExportRow = new mMarionExport();
                    marionExportRow = TranslateAccountRowToMarionExportRow(accountRow, marionExportRow);

                    var cadAcctSqlString = $"select top 1 * from tblCadAccount a where a.AcctID={accountRow.AcctID} ";
                    var cadAccountRow = db.Query<mCadAccount>(cadAcctSqlString).FirstOrDefault();
                    marionExportRow = TranslateCadAccountRowToMarionExportRow(cadAccountRow, marionExportRow);

                    //import from tblName
                    var ownerSqlString = $"select top 1 * from tblName where NameID = {accountRow.NameID}";
                    //var ownerSqlString = $"select * from tblName n join tblCadOwners c on n.NameID = c.NameID where c.CadID='MAR'";
                    var ownerRow = db.Query<mOwner>(ownerSqlString).FirstOrDefault();
                    marionExportRow = TranslateOwnerRowToMarionExportRow(ownerRow, marionExportRow);

                    //import from tblCadOwners
                    var cadOwnerSqlString = $"select top 1 * from tblCadOwners a where a.NameID={accountRow.NameID} ";
                    //var cadOwnerSqlString = $"select top 1 * from tblCadOwners a where a.Acct={ownerRow.NameID} and a.CadID='MAR' ";
                    var cadOwnerRow = db.Query<mCadOwner>(cadOwnerSqlString).FirstOrDefault();
                    marionExportRow = TranslateCadOwnerRowToMarionExportRow(cadOwnerRow, marionExportRow);

                    //import tblProperty
                    var propertySqlString = $"select top 1 * from tblProperty p where p.PropID={accountRow.PropID}";
                    var propertyRow = db.Query<mProperty>(propertySqlString).FirstOrDefault();
                    marionExportRow = TranslatePropertyRowToMarionExportRow(propertyRow, marionExportRow);

                    var cadPropertySqlString = $"select top 1 * from tblCadProperty p where p.PropID={accountRow.PropID} ";
                    var cadPropertyRow = db.Query<mCadProperty>(cadPropertySqlString).FirstOrDefault();
                    marionExportRow = TranslateCadPropertyRowToMarionExportRow(cadPropertyRow, marionExportRow);

                    // THE IMPORT HAS AN ERROR: MARION UNITS APPEAR NOT CONVERTED THROUGH TLKPCADUNIT TO WAG UNITIDs
                    //import tblUnitProperty where Cad='MAR'
                    //var unitPropertySqlString = $"select * from tblUnitProperty where PropID={accountRow.PropID}";
                    //var unitPropertyRows = db.Query<mUnitProperty>(unitPropertySqlString);
                    //
                    //foreach (mUnitProperty unitPropertyRow in unitPropertyRows)
                    //{
                    //    marionExportRow  = TranslateUnitPropertyRowToMarionExportRow(unitPropertyRow);                       
                    //    //what about tlkpCadUnit??
                    //}


                    if (propertyRow.PtdClassSub.Trim() == "G1" || propertyRow.PtdClassSub.Trim() == "XV")
                    {

                        //import from tblTract
                        var tractSqlString = $"select top 1 t.LeasePct,t.LeaseID from tblTract t where t.PropID = {accountRow.PropID}";
                        var tractRow = db.Query<mTract>(tractSqlString).FirstOrDefault();
                        marionExportRow = TranslateTractRowToMarionExportRow(tractRow, marionExportRow);

                        if (tractRow != null)
                        {
                            //import tblLease where Cad='MAR'
                            //l.LeaseNameWag,l.LeaseOprID
                            var leaseSqlString = $"select top 1 * from tblLease l where l.LeaseID = {tractRow.LeaseID}";
                            var leaseRow = db.Query<mLease>(leaseSqlString).FirstOrDefault();
                            marionExportRow = TranslateLeaseRowToMarionImportRow(leaseRow, marionExportRow);

                            //import from tblCadLease
                            var cadLeaseSqlString = $"select top 1 * from tblCadLease c where c.LeaseID={leaseRow.LeaseID}";
                            var cadLeaseRow = db.Query<mCadLease>(cadLeaseSqlString).FirstOrDefault();
                            marionExportRow = TranslateCadLeaseRowToMarionImportRow(cadLeaseRow, marionExportRow);

                            //import from tblWell
                            var wellSqlString = $"select top 1 ProdDateFirst from tblWell w where w.LeaseID={leaseRow.LeaseID}";
                            var ProdDateFirst = db.Query<DateTime>(wellSqlString).FirstOrDefault();
                            marionExportRow.YearLeaseStarted = ProdDateFirst.Year;

                            var wellRrc = cadLeaseRow.CadLeaseId;
                            marionExportRow.RRC = "RRC #  " + int.Parse(wellRrc).ToString();
                        }
                    }
                    MarionExportRows.Add(marionExportRow);
                }
            }

            csvWriter.WriteRecords(MarionExportRows);
            csvWriter.Dispose();
            writer.Dispose();

            MessageBox.Show($"MarionExportCompleted");
        }

        private mMarionExport TranslateWellRowToMarionImportRow(DateTime prodDateFirst, mMarionExport marionExportRow)
        {


            return marionExportRow;
        }

        private mMarionExport TranslateTractRowToMarionExportRow(mTract tractRow, mMarionExport marionExportRow)
        {
            if (tractRow != null)
                marionExportRow.acres = (float)tractRow.LeasePct;
            //store this in Memo(not in model because it is nvarchar type)
            return marionExportRow;
        }

        private mMarionExport TranslateCadLeaseRowToMarionImportRow(mCadLease cadLeaseRow, mMarionExport marionExportRow)
        {
            //marionExportRow.LeaseNumber = int.Parse(cadLeaseRow.CadLeaseId); this was already done with tblProperty
            return marionExportRow;
        }

        private mMarionExport TranslateLeaseRowToMarionImportRow(mLease leaseRow, mMarionExport marionExportRow)
        {
            marionExportRow.LeaseName = leaseRow.LeaseNameWag.Trim();

            var operatorKey = leaseRow.LeaseOprID;
            var operNameSortCad = "";
            if (operatorKey != 0)
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    operNameSortCad = db.ExecuteScalar($"SELECT TOP 1 NameSortCad FROM tblName where NameID = '{operatorKey}'") as string;
                }
                marionExportRow.OperatorName = operNameSortCad;
            }
            else
            {
                marionExportRow.AgentNumber = 0;
            }

            return marionExportRow;
        }

        private mMarionExport TranslateUnitPropertyRowToMarionImportRow(mUnitProperty unitPropertyRow, mMarionExport marionExportRow)
        {
            marionExportRow.Jurisdiction1 = unitPropertyRow.UnitID;
            return marionExportRow;
        }

        private mMarionExport TranslateCadPropertyRowToMarionExportRow(mCadProperty cadPropertyRow, mMarionExport marionExportRow)
        {
            marionExportRow.LeaseNumber = int.Parse(cadPropertyRow.CadPropID);
            return marionExportRow;
        }

        private mMarionExport TranslatePropertyRowToMarionExportRow(mProperty propertyRow, mMarionExport marionExportRow)
        {

            marionExportRow.SPTBCode = propertyRow.PtdClassSub;
            //var legalHyphenIndex = propertyRow.Legal.IndexOf("-");
            //marionExportRow.Description1 = propertyRow.Legal.Substring(0, legalHyphenIndex);
            //marionExportRow.Description2 = propertyRow.Legal.Substring(legalHyphenIndex + 1);

            //Description1 has abstract and survey for minerals, property description for U&Is
            //Description2 has well and rrc# for minerals, unit count and taxing unit for U&Is            

            //TYPE PROPERTY CODES | INTEREST TYPE CODES
            //1 = REAL VALUE            | 1 = RI
            //2 = REAL VALUE NP         | 2 = OR
            //3 = PERSONAL PROPERTY     | 3 = OP
            //                          | 4 = WI
            //                          | 5 = RA

            marionExportRow.PropertyType = ConvertPropTypeToPropertyType(propertyRow);


            //marionExportRow.PropertyType=

            return marionExportRow;
        }

        private mMarionExport TranslateCadOwnerRowToMarionExportRow(mCadOwner cadOwnerRow, mMarionExport marionExportRow)
        {
            marionExportRow.OwnerNumber = int.Parse(cadOwnerRow.CadOwnerID);
            return marionExportRow;
        }

        private mMarionExport TranslateOwnerRowToMarionExportRow(mOwner owner, mMarionExport marionExportRow)
        {
            //var marionExportRow = new mMarionExport();

            marionExportRow.OwnerName = owner.NameSortCad;
            marionExportRow.StreetAddress = owner.Mail1;
            marionExportRow.CityStateZip = owner.MailCi + ", " + owner.MailSt + owner.MailZip;

            var agentKey = owner.AgentID;
            var agentCadOwnerID = "";
            if (agentKey != 66864)
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    agentCadOwnerID = db.ExecuteScalar($"SELECT TOP 1 CadOwnerID FROM tblCadOwners where NameID = '{agentKey}'") as string;
                }
                marionExportRow.AgentNumber = int.Parse(agentCadOwnerID);        //need to get this. how??
            }
            else
            {
                marionExportRow.AgentNumber = 0;
            }

            return marionExportRow;
        }

        private mMarionExport TranslateCadAccountRowToMarionExportRow(mCadAccount accountRow, mMarionExport marionExportRow)
        {
            //public string GeoRef { get; set; }                   //622,646 A,25   CUSTOMER GEO#     //tblCadOwners.CadAcctID
            marionExportRow.GeoRef = accountRow.CadAcctID;
            return marionExportRow;
        }

        private mMarionExport TranslateAccountRowToMarionExportRow(mAccount accountRow, mMarionExport marionExportRow)
        {
            //******************************** Here I want to lookup values from AbMarionImport instead **************************
            // I want to make sure that there is a one-to-one correspondence and order between tblAccount and AbMarionImport rows
            // to do this I created a <mAccountToImportMap> AbMarionAccountToImportMap
            // with this I want to add ImportID,Job,AccountNumber,AccountSequence,Description1,Description2,Acres
            //                         Jurisdictions,TaxableValue,MarketValue,ExemptionMinOwnerFlag,(NewTaxableValue????)






            //var marionExportRow = new mMarionExport();


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
            //public int AccountNumber { get; set; }               //668,674 S 7    MINERAL ACCOUNT NUMBER      //tblAccount.SeqNmbr
            marionExportRow.AccountNumber = res1 ? acctNumbr : 0;
            //public int AccountSequence { get; set; }             //675,681 S 7    MINERAL ACCOUNT SEQUENCE #  //tblAccount.SeqNmbr
            marionExportRow.AccountSequence = res2 ? acctSeq : 0; ;
            //******************************************


            marionExportRow.CurrentTaxYear = 2022;

            //public string Protest { get; set; }                  //22,22  A,1     PROTEST=P         //tblAccount.Protest_YN
            marionExportRow.Protest = "";       // at start of season there are no protests.

            //public decimal DecimalInterest { get; set; }         //34,40  N,76    DECIMAL INTEREST  //tblAccount.PctProp
            marionExportRow.DecimalInterest = (decimal)accountRow.PctProp;

            int intType = 0;
            switch (accountRow.PctType)
            {

                case "U":
                    intType = 0;
                    break;
                case "R":
                    intType = 1;
                    break;
                case "O":
                    intType = 2;
                    break;
                case "W":
                    intType = 4;
                    break;
            }

            //public int InterestType { get; set; }                //17,17  N,10    INTEREST TYPE     //tblAccount.PctType
            marionExportRow.InterestType = intType;

            return marionExportRow;
        }
        private static string ConvertPropTypeToPropertyType(mProperty propRow)
        {
            //TYPE PROPERTY CODES | INTEREST TYPE CODES
            //1 = REAL VALUE            | 1 = RI
            //2 = REAL VALUE NP         | 2 = OR
            //3 = PERSONAL PROPERTY     | 3 = OP
            //                          | 4 = WI
            //                          | 5 = RA
            var propType = propRow.PropType;
            string marionPropertyType = "";
            switch (propType)
            {
                //case 0:
                //    _intType = "U";
                //    break;
                case "R"://P&A REAL (includes minerals)
                    marionPropertyType = "1";
                    break;
                //case //P&A REAL (Inspection of Marion Import shows Improvements, Wag treats this as Personal Property)
                //    marionPropertyType = "P";
                //    break;
                case "P"://P&A PERSONAL PROPERTY
                    marionPropertyType = "3";
                    break;
            }
            return marionPropertyType;
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
