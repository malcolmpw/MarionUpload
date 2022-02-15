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
        public static Dictionary<string,Tuple<string,bool>> NameSel_YN { get; set; }
        public ICommand CommandImportOwners => new RelayCommand(OnImportOwners);
        public ICommand CommandUploadOwners => new RelayCommand(OnUploadOwners);

        public vmOwner()
        {
            MarionOwners = new ObservableCollection<mMarionOwner>();
            MarionOwners2015 = new ObservableCollection<mOwner2015>();
            NameSel_YN = new Dictionary<string, Tuple<string,bool>>();
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
                    var tuplePerson = Tuple.Create(dr.NameSortCad.Trim().ToUpper(),dr.NameSel_YN);
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
                foreach(mMarionOwner mo in MarionOwners) { }
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
                string sqlQuery = "Insert Into tblCadOwners(NameID, CadID) Values(@NameID, @CadID)";
                foreach (var marionOwner in MarionOwners)
                {
                    mOwner ownerToInsert = TranslateFrom_mMarionOwnerTo_mOwner(marionOwner);
                    //OwnersToInsert.Add(ownerToInsert);
                    // Insert resulting NameID and importedMarionOwner.OwnerNumber into a new table called tblMarionToOwner
                    db.Execute(sqlQuery, ownerToInsert);
                }
            }
        }

        private DateTime _UpdateDate;
        private string _UpdateBy;

        private mOwner TranslateFrom_mMarionOwnerTo_mOwner(mMarionOwner importedMarionOwner)
        {
            _UpdateDate = DateTime.Now;
            _UpdateBy = "MPW";

            var owner = new mOwner();
            owner.AgentID = importedMarionOwner.AgentNumber.Trim();
            owner.Agent_YN = importedMarionOwner.AgentNumber.Trim() != "0";
            owner.Mail1 = importedMarionOwner.StreetAddress.Trim();
            var cityStateZip = importedMarionOwner.CityStateZip.Trim();
            owner.MailCi = cityStateZip.Substring(0, 18).Trim();
            owner.MailSt = cityStateZip.Substring(18, 2);
            owner.MailZ = cityStateZip.Substring(20, 5);

            Regex re = new Regex(@"\-\d{4}$");
            var matchResult = re.Match(cityStateZip);
            if (matchResult.Success)
            {
                owner.MailZ4 = cityStateZip.Substring(26, 4);
            }

            owner.NameSort = importedMarionOwner.OwnerName.Trim();
            owner.UpdateDate = _UpdateDate;
            owner.UpdateBy = _UpdateBy;
            owner.CadID = "MAR";


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
}