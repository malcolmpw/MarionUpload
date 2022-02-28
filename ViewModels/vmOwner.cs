using Dapper;
using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight.Command;
using MarionDistributeImport.Helpers;
using MarionUpload.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Input;

namespace MarionUpload.ViewModels
{
    public class vmOwner
    {
        
        const string MarionCounty2015QueryString = "SELECT distinct n.NameSortCad, n.NameSel_YN, " +
                                                            "n.NameH, n.NameF, n.NameM, n.NameL1, n.NameL2, n.NameLS, n.NameC, n.NameCP, n.Name2 " +
                                                             "FROM[WagData2015].[dbo].[tblName] n " +
                                                             "inner join[WagData2015].[dbo].[tblAccount] a " +
                                                             "on n.NameID = a.NameID " +
                                                             "inner join[WagData2015].[dbo].tblProperty p " +
                                                             "on a.PropID = p.PropID " +
                                                             "where p.ControlCad = 'MAR' " +
                                                             "order by n.NameSortCad ";

        public ObservableCollection<mMarionOwner> MarionOwners { get; set; }
        public ObservableCollection<mOwner> MarionOwners2015 { get; set; }
        public ObservableCollection<mOwner> InsertedOwners { get; set; }
        public Dictionary<string, mOwner> NameSortCadMap { get; set; }

        public ICommand CommandImportOwners => new RelayCommand(OnImportOwners);
        public ICommand CommandUploadOwners => new RelayCommand(OnUploadOwners);

        public vmOwner()
        {
            MarionOwners = new ObservableCollection<mMarionOwner>();
            MarionOwners2015 = new ObservableCollection<mOwner>();
            NameSortCadMap = new Dictionary<string, mOwner>();
        }

        private void OnImportOwners()
        {
            SelectOwnerDataFromMarionImportTable();
            SelectOwnerDataFromWagData2015();
        }

        private void SelectOwnerDataFromWagData2015()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString2015))
            {
                string queryString = MarionCounty2015QueryString;
                var results = db.Query<mOwner>(queryString);
                var distinctResults = results.Distinct(new OwnerNumberComparer2015()).ToList();
                CreateNameSelectDictionary(distinctResults);

                // Loop through each marion owner and fill in the NameSelect info.
                foreach (mMarionOwner marionOwner in MarionOwners)
                {
                    var ownerNameTrimmed = marionOwner.OwnerName.Trim().ToUpper();
                    if (NameSortCadMap.ContainsKey(ownerNameTrimmed))
                    {
                        FillMarionOwnerWithNameSelectFlag(marionOwner);
                    }
                }

            }
        }

        private string FillMarionOwnerWithNameSelectFlag(mMarionOwner marionOwner)
        {
            string ownerNameTrimmed;
            {
                ownerNameTrimmed = marionOwner.OwnerName.Trim().ToUpper();
                marionOwner.NameSortCad = ownerNameTrimmed;
                marionOwner.NameSel_YN = NameSortCadMap[ownerNameTrimmed].NameSel_YN;
            }

            return ownerNameTrimmed;
        }

        private void CreateNameSelectDictionary(List<mOwner> distinctResults)
        {
            foreach (mOwner dr in distinctResults)
            {
                MarionOwners2015.Add(dr);
                string nameSortCadModified = dr.NameSortCad.Trim().ToUpper();
                if (!NameSortCadMap.ContainsKey(nameSortCadModified))
                {
                    NameSortCadMap.Add(nameSortCadModified, dr);
                }
            }
        }

        private void OnUploadOwners()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                foreach (mMarionOwner _marionOwner in MarionOwners)
                {
                    var populatedOwner = TranslateFrom_mMarionOwnerTo_mOwner(_marionOwner);
                    var primaryKey = db.Insert<mOwner>(populatedOwner);
                    NameIdMap.Add(_marionOwner.OwnerNumber, primaryKey);

                    var populatedCadOwner = TranslateFrom_mMarionPropertyTo_mCadOwner(_marionOwner);
                    var primaryCadOwnerKey = db.Insert<mCadOwner>(populatedCadOwner);
                    OwnerNumberToNameIdMap.Add(_marionOwner.OwnerNumber, primaryCadOwnerKey);
                }
            }
         //   UploadMarionOwnersToTblName();
        }

        private mCadOwner TranslateFrom_mMarionPropertyTo_mCadOwner(mMarionOwner marionOwner)
        {
            throw new NotImplementedException();
        }

        void SelectOwnerDataFromMarionImportTable()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                var results = db.Query<mMarionOwner>("Select ImportID, OwnerNumber, OwnerName, InCareOf, StreetAddress, CityStateZip, AgentNumber From AbMarionImport");
                var distinctResults = results.Distinct(new OwnerNumberComparer()).ToList();
                distinctResults.ForEach(owner => MarionOwners.Add(owner));
            }
        }

        public List<mOwner> OwnersToInsert { get; set; }
        public static IDictionary<int, long> NameIdMap { get; private set; } = new Dictionary<int, long>();
        public IDictionary<int, long> OwnerNumberToNameIdMap { get; private set; } = new Dictionary<int, long>();

        private DateTime _updateDate;
        private string _updateBy;

        private mOwner TranslateFrom_mMarionOwnerTo_mOwner(mMarionOwner importedMarionOwner)
        {
            NameSorts nsorts = new NameSorts();
            _updateDate = DateTime.Now;
            _updateBy = "MPW";

            var owner = new mOwner();

            owner.CadID = "MAR";
            owner.NameSortCad = importedMarionOwner.OwnerName.Trim();
            var matchingOwner = NameSortCadMap[owner.NameSortCad];
            owner.NameSortFirst = owner.NameSortCad;
            owner.NameH = matchingOwner.NameH; // search the NameSortCad for titles, use SELECT distinct[NameH] FROM[WagData2015].[dbo].[tblName]                
            owner.NameF = matchingOwner.NameF; // split and parse
            owner.NameM = matchingOwner.NameM; // split and parse
            owner.NameL1 = matchingOwner.NameL1; // split and parse
            owner.NameL2 = matchingOwner.NameL2; // split and parse
            owner.NameLS = matchingOwner.NameLS; // search the NameSortCad for titles, use SELECT distinct[NameLS FROM[WagData2015].[dbo].[tblName]
            owner.NameC = matchingOwner.NameC;
            owner.NameCP = matchingOwner.NameCP;
            owner.Name2 = matchingOwner.Name2; // search the NameSortCad for titles, use SELECT distinct[Name2] FROM[WagData2015].[dbo].[tblName]
                                // these may be taken from WagData2015 for the old list of marion owners in tblName.               
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
            owner.Agnt_YN = importedMarionOwner.AgentNumber.Trim() != "0";

            owner.UpdateDate = _updateDate;
            owner.UpdateBy = _updateBy;

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
    public class OwnerNumberComparer2015 : IEqualityComparer<mOwner>
    {
        public bool Equals(mOwner x, mOwner y)
        {
            return x.NameSortCad == y.NameSortCad;
        }

        public int GetHashCode(mOwner obj)
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
