﻿using System;
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
        public ObservableCollection<mMarionOwner> MarionOwners { get; set; }
        public ICommand CommandImportOwners => new RelayCommand(OnImportOwners);
        public ICommand CommandUploadOwners => new RelayCommand(OnUploadOwners);

        public vmOwner()
        {
            MarionOwners = new ObservableCollection<mMarionOwner>();
        }

        private void OnImportOwners()
        {
            SelectOwnerDataFromMarionImportTable();
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

        void UploadMarionOwnersToTblName()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                foreach (var marionOwner in MarionOwners)
                {
                    mOwner owner = TranslateToOwner(marionOwner);
                    var result = db.Insert<mOwner>(owner);
                    // Insert resulting NameID and importedMarionOwner.OwnerNumber into a new table called tblMarionToOwner
                }
            }
        }

        private mOwner TranslateToOwner(mMarionOwner importedMarionOwner)
        {
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
                owner.MailZ4 = cityStateZip.Substring(26,4);
            }

            owner.NameSort = importedMarionOwner.OwnerName.Trim();

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

}