using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Helpers
{
    public class ConnectionStringHelper
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MarionConnectionString"].ConnectionString;
        public static readonly string BackupConnectionString = ConfigurationManager.ConnectionStrings["MarionBackupConnectionString"].ConnectionString;
        public static readonly string ConnectionString2017 = ConfigurationManager.ConnectionStrings["Wag2017ConnectionString"].ConnectionString;
    }
}
