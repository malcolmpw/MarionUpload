using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace MarionUpload.Helpers
{
    public class RRCHelper
    {
        static public string FormatRRC(string rrcLease, out string rrcOperId)
        {
           rrcOperId = "";
           using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
           {
                var formattedRRC = rrcLease;
                 rrcOperId = db.ExecuteScalar($"SELECT TOP 1 RrcOpr FROM tblWell where RrcLease = '{formattedRRC}'") as string;
                if (string.IsNullOrEmpty(rrcOperId))
                {
                    formattedRRC = rrcLease.PadLeft(5, '0');  // oil well
                    rrcOperId = db.ExecuteScalar($"SELECT TOP 1 RrcOpr FROM tblWell where RrcLease = '{formattedRRC}'") as string;
                    //wellLpdID = db.ExecuteScalar($"SELECT TOP 1 LpdID FROM tblWell where RrcLease = '{formattedRRC}'") as string;

                    if (string.IsNullOrEmpty(rrcOperId))
                    {
                        formattedRRC = rrcLease.PadLeft(6, '0');
                        rrcOperId = db.ExecuteScalar($"SELECT TOP 1 RrcOpr FROM tblWell where RrcLease = '{formattedRRC}'") as string;
                        //wellLpdID = db.ExecuteScalar($"SELECT TOP 1 LpdID FROM tblWell where RrcLease = '{formattedRRC}'") as string;
                    }
                }

                if (string.IsNullOrEmpty(rrcOperId))
                {
                    return rrcLease;
                }
                else
                {
                    return formattedRRC;
                }
            }
        }
    }
}
