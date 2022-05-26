using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    public class mUnitPct
    {
        public string ColumnName { get; set; }
        public string JurisdictionID { get; set; }
        public string MarketValueString { get; set; }
        public float MarketValueFloat { get; set; }
        public float UnitPct { get; set; }
    }
}
