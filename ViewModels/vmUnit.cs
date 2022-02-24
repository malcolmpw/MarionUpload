﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using GalaSoft.MvvmLight.Command;

namespace MarionUpload.ViewModels
{
    public class vmUnit
    {
        public ICommand CommandPopulateTlkpUnitWithMarion => new RelayCommand(OnPopulateTlkpUnitWithMarionCounty);

        const string ConnectionString = @"Data Source=WAGSQLSRV01\DEV;Initial Catalog=wagapp2_2021_Marion;Integrated Security=True;";

        const string ImportIntoTlkpCadUnitTable = "Insert Into [wagapp2_2021_Marion].[dbo].[tlkpCadUnit] SELECT * from [WagData2015].[dbo].[tlkpCadUnit] where CadId = 'MAR' and CadAppraised = 1";
        const string ImportIntoTlkpUnitTable = "Insert Into [wagapp2_2021_Marion].[dbo].[tlkpUnit] SELECT * from [WagData2015].[dbo].[tlkpUnit] where CadId = 'MAR'";

        private void OnPopulateTlkpUnitWithMarionCounty()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                db.Execute(ImportIntoTlkpCadUnitTable);
                db.Execute(ImportIntoTlkpUnitTable);
            }
        }
    }
}
