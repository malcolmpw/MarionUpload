using System.Data;
using System.Data.SqlClient;
using System.Windows.Input;
using Dapper;
using GalaSoft.MvvmLight.Command;
using MarionUpload.Helpers;

namespace MarionUpload.ViewModels
{
    public class vmLookupUnits
    {
        public ICommand CommandPopulateTlkpUnitWithMarion => new RelayCommand(OnPopulateTlkpUnitWithMarionCounty);


        const string ImportIntoTlkpCadUnitTable = "Insert Into [dbo].[tlkpCadUnit] SELECT * " +
                                                    "from [WagData2017].[dbo].[tlkpCadUnit] where CadId = 'MAR' and CadAppraised = 1";

        const string ImportIntoTlkpUnitTable = "Insert Into [dbo].[tlkpUnit] SELECT * " +
                                                    "from [WagData2017].[dbo].[tlkpUnit] where CadId = 'MAR'";

        private void OnPopulateTlkpUnitWithMarionCounty()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                db.Execute(ImportIntoTlkpCadUnitTable);
                db.Execute(ImportIntoTlkpUnitTable);
            }
        }
    }
}
