using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionDistributeImport.Helpers
{
    public class ConnectionStringHelper
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MarionConnectionString"].ConnectionString;

        public static readonly string ConnectionString2015 = ConfigurationManager.ConnectionStrings["Wag2015ConnectionString"].ConnectionString;
    }
}
