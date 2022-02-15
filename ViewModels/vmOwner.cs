using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using Dapper.Contrib;
using System.Data.SqlClient;
using System.Data;
using GalaSoft.MvvmLight.Command;
using MarionUpload.Models;
using Dapper.Contrib.Extensions;
using System.Text.RegularExpressions;

namespace MarionUpload.ViewModels
{
    public class vmOwner
    {
        const string ConnectionString = @"Data Source=WAGSQLSRV01\DEV;Initial Catalog=wagapp2_2021_Marion;Integrated Security=True;";
        const string ConnectionString2015 = @"Data Source=WAGSQLSRV01\DEV;Initial Catalog=WagData2015;Integrated Security=True;";
        public static ObservableCollection<mMarionOwner> MarionOwners { get; set; }
        public static ObservableCollection<mOwner2015> MarionOwners2015 { get; set; }
        public static ObservableCollection<mOwner> InsertedOwners { get; set; }
        public static Dictionary<string, Tuple<string, bool>> NameSel_YN { get; set; }
        public ICommand CommandImportOwners => new RelayCommand(OnImportOwners);
        public ICommand CommandUploadOwners => new RelayCommand(OnUploadOwners);

        public vmOwner()
        {
            MarionOwners = new ObservableCollection<mMarionOwner>();
            MarionOwners2015 = new ObservableCollection<mOwner2015>();
            NameSel_YN = new Dictionary<string, Tuple<string, bool>>();
        }

        private void OnImportOwners()
        {
            SelectOwnerDataFromMarionImportTable();
            SelectOwnerDataFromWagData2015();
        }

        private static void SelectOwnerDataFromWagData2015()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString2015))
            {
                string queryString = "SELECT distinct n.NameSortCad, n.NameSel_YN " +
                                                     "FROM[WagData2015].[dbo].[tblName] n " +
                                                     "inner join[WagData2015].[dbo].[tblAccount] a " +
                                                     "on n.NameID = a.NameID " +
                                                     "inner join[WagData2015].[dbo].tblProperty p " +
                                                     "on a.PropID = p.PropID " +
                                                     "where p.ControlCad = 'MAR' " +
                                                     "order by n.NameSortCad ";
                var results = db.Query<mOwner2015>(queryString);
                var distinctResults = results.Distinct(new OwnerNumberComparer2015()).ToList();
                //distinctResults.ForEach(owner => MarionOwners2015.Add(owner));
                foreach (mOwner2015 dr in distinctResults)
                {
                    var tuplePerson = Tuple.Create(dr.NameSortCad.Trim().ToUpper(), dr.NameSel_YN);
                    MarionOwners2015.Add(dr);
                    if (!NameSel_YN.ContainsKey(dr.NameSortCad.Trim().ToUpper()))
                    {
                        NameSel_YN.Add(dr.NameSortCad.Trim().ToUpper(), tuplePerson);
                    }
                }
                foreach (mMarionOwner mo in MarionOwners)
                {
                    //var Tuple.Create(mo.NameSortCad, mo.NameSel_YN);
                    var OwnerNameTrimmed = mo.OwnerName.Trim().ToUpper();
                    if (NameSel_YN.ContainsKey(OwnerNameTrimmed))
                    {
                        OwnerNameTrimmed = mo.OwnerName.Trim().ToUpper();
                        mo.NameSortCad = NameSel_YN[OwnerNameTrimmed].Item1;
                        mo.NameSel_YN = NameSel_YN[OwnerNameTrimmed].Item2;
                    }
                }
                foreach (mMarionOwner mo in MarionOwners)
                {
                    //parse NameSortCad based on NameSel_YN


                    //convert upper case strings to First character upper case only


                }
            }
        }

        private void OnUploadOwners()
        {
            UploadMarionOwnersToTblName();
        }

        void SelectOwnerDataFromMarionImportTable()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                var results = db.Query<mMarionOwner>("Select ImportID, OwnerNumber, OwnerName, InCareOf, StreetAddress, CityStateZip, AgentNumber From AbMarionImport");
                var distinctResults = results.Distinct(new OwnerNumberComparer()).ToList();
                distinctResults.ForEach(owner => MarionOwners.Add(owner));
            }
        }

        public List<mOwner> OwnersToInsert { get; set; }

        void UploadMarionOwnersToTblName()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                string sqlString = "Insert Into tblCadOwners(NameID, CadID) Values(@NameID, @CadID)";
                foreach (var marionOwner in MarionOwners)
                {
                    mOwner ownerToInsert = TranslateFrom_mMarionOwnerTo_mOwner(marionOwner);
                    //OwnersToInsert.Add(ownerToInsert);
                    // Insert resulting NameID and importedMarionOwner.OwnerNumber into a new table called tblMarionToOwner
                    db.Execute(sqlString, ownerToInsert);
                }
                string fields = "CadID, NameSort, Mail1 , MailCi ,MailSt, MailSt, MailZ, MailZ4 ,MailCo ,MailZip ,AgentID,Agent_YN,UpdateDate,UpdateBy ,NameSortCad,NameSel_YN ";
                string parameters = "@CadID, @NameSort, @Mail1 , @MailCi ,@MailSt, @MailSt, @MailZ, @MailZ4 ,@MailCo ,@MailZip ,@AgentID,@Agent_YN,@UpdateDate,@UpdateBy ,@NameSortCad,@NameSel_YN ";
                string sqlQuery = $"Insert Into tblCadOwners( {fields}) Values({parameters})";
                var result = db.Query<mOwner>(sqlQuery).ToList();
                InsertedOwners = new ObservableCollection<mOwner>(result);
            }
        }

        private DateTime _UpdateDate;
        private string _UpdateBy;

        private mOwner TranslateFrom_mMarionOwnerTo_mOwner(mMarionOwner importedMarionOwner)
        {
            NameSorts nsorts = new NameSorts();
            _UpdateDate = DateTime.Now;
            _UpdateBy = "MPW";

            var owner = new mOwner();

            owner.CadID = "MAR";
            owner.NameSortCad = importedMarionOwner.OwnerName.Trim();
            owner.NameSortFirst = owner.NameSortCad;
            if (!owner.NameSel_YN)
            {
                owner.NameH = "";
                owner.NameF = "";
                owner.NameM = "";
                owner.NameL1 = "";
                owner.NameL2 = "";
                owner.NameLS = "";
                owner.NameC = null;
                owner.Name2 = "";
                owner.NameSort = owner.NameL1 + ", " + owner.NameF + owner.NameM;// get this code from WagApp2
                owner.NameSort = nsorts.RebuildNameSort(owner).NameSort;
            }
            else
            {
                owner.NameH = null;
                owner.NameH = null;
                owner.NameF = null;
                owner.NameM = null;
                owner.NameL1 = null;
                owner.NameL2 = null;
                owner.NameLS = null;
                owner.NameC = owner.NameSortCad;
                owner.Name2 = null;
            }
            owner.Mail1 = importedMarionOwner.StreetAddress.Trim();
            var cityStateZip = importedMarionOwner.CityStateZip.Trim();
            owner.MailCi = cityStateZip.Substring(0, cityStateZip.Length - 7).Trim();
            var stateZip = cityStateZip.Substring(cityStateZip.Length - 7 + 1).Trim();
            owner.MailSt = stateZip.Substring(0, 2);
            owner.MailZ = stateZip.Substring(2);
            // Example:
            // Land O Lakes TX12345
            // 00000000001111111111
            // 01234567890123456789

            //Not Used:
            //Regex re = new Regex(@"\-\d{4}$");
            //var matchResult = re.Match(cityStateZip);
            //if (matchResult.Success)
            //{
            //    owner.MailZ4 = cityStateZip.Substring(26, 4);
            //}

            owner.AgentID = importedMarionOwner.AgentNumber.Trim();
            owner.Agent_YN = importedMarionOwner.AgentNumber.Trim() != "0";

            owner.UpdateDate = _UpdateDate;
            owner.UpdateBy = _UpdateBy;

            return owner;
        }
    }

    public class OwnerNumberComparer : IEqualityComparer<mMarionOwner>
    {
        public bool Equals(mMarionOwner x, mMarionOwner y)
        {
            return x.OwnerNumber == y.OwnerNumber;
        }

        public int GetHashCode(mMarionOwner obj)
        {
            return obj.OwnerNumber.GetHashCode();
        }
    }
    public class OwnerNumberComparer2015 : IEqualityComparer<mOwner2015>
    {
        public bool Equals(mOwner2015 x, mOwner2015 y)
        {
            return x.NameSortCad == y.NameSortCad;
        }

        public int GetHashCode(mOwner2015 obj)
        {
            return obj.NameSortCad.GetHashCode();
        }
    }

    public class NameSorts
    {
        public mOwner RebuildNameSort(mOwner Owner)
        {
            string sWorking = "";

            if (Owner.NameSel_YN)
            {
                if (Owner.NameC != "" && Owner.NameC != null) sWorking = Owner.NameC.Trim();
                if (Owner.NameCP != "" && Owner.NameCP != null)
                {
                    sWorking = GetTruncatedSubstring(Owner.NameC.Trim() + ", " + Owner.NameCP.Trim(), 45);
                }

                Owner.Addr1 = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSortCad = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSort = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSortFirst = GetTruncatedSubstring(sWorking, 40);
            }
            else
            {
                Owner.NameSel_YN = false;

                // Build NameSortFirst
                sWorking = "";

                if (Owner.NameF != "" && Owner.NameF != null) sWorking = Owner.NameF.Trim();
                if (Owner.NameM != "" && Owner.NameM != null) sWorking = sWorking + " " + Owner.NameM.Trim();
                if (Owner.NameL1 != "" && Owner.NameL1 != null) sWorking = sWorking + " " + Owner.NameL1.Trim();
                if (Owner.NameLS != "" && Owner.NameLS != null) sWorking = sWorking + " " + Owner.NameLS.Trim();
                if (Owner.NameL2 != "" && Owner.NameL2 != null) sWorking = sWorking + " " + Owner.NameL2.Trim();
                if (Owner.NameT != "" && Owner.NameT != null) sWorking = sWorking + " " + Owner.NameT.Trim();
                if (Owner.NameH != "" && Owner.NameH != null) sWorking = sWorking + ", " + Owner.NameH.Trim();


                Owner.Addr1 = GetTruncatedSubstring(sWorking, 40);
                Owner.NameSortFirst = GetTruncatedSubstring(sWorking, 40);


                // Build NameSort
                sWorking = "";

                if (Owner.NameL1 != "" && Owner.NameL1 != null) sWorking = Owner.NameL1.Trim();
                if (Owner.NameF != "" && Owner.NameF != null) sWorking = sWorking + ", " + Owner.NameF.Trim();
                if (Owner.NameM != "" && Owner.NameM != null) sWorking = sWorking + " " + Owner.NameM.Trim();
                if (Owner.NameLS != "" && Owner.NameLS != null) sWorking = sWorking + ", " + Owner.NameLS.Trim();
                if (Owner.NameL2 != "" && Owner.NameL2 != null) sWorking = sWorking + " " + Owner.NameL2.Trim();
                if (Owner.NameH != "" && Owner.NameH != null) sWorking = sWorking + " " + Owner.NameH.Trim();
                if (Owner.NameT != "" && Owner.NameT != null) sWorking = sWorking + ", " + Owner.NameT.Trim();

                Owner.NameSort = GetTruncatedSubstring(sWorking, 40);

                // Build NameSortCad
                sWorking = "";

                if (Owner.NameL1 != "" && Owner.NameL1 != null) sWorking = Owner.NameL1.Trim();
                if (Owner.NameF != "" && Owner.NameF != null) sWorking = sWorking + ", " + Owner.NameF.Trim();
                if (Owner.NameM != "" && Owner.NameM != null) sWorking = sWorking + ", " + Owner.NameM.Trim();
                if (Owner.NameLS != "" && Owner.NameLS != null) sWorking = sWorking + ", " + Owner.NameLS.Trim();
                if (Owner.NameL2 != "" && Owner.NameL2 != null) sWorking = sWorking + ", " + Owner.NameL2.Trim();

                Owner.NameSortCad = GetTruncatedSubstring(sWorking, 40);

            }
            return Owner;

            //SaveOwnerPrimaryData(Owner); // save any new changes before rebuilding namesort
            //Messenger.Default.Send(new OwnerUpdateNameSelectMessage() { Owner = Owner });

        }
        string GetTruncatedSubstring(string workingString, int trimLength)
        {
            if (workingString.Length > trimLength)
            {
                return workingString.Substring(0, trimLength);
            }
            return null;
        }
    }
}
