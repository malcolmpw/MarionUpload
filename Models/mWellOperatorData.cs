using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Models
{
    public class mWellOperatorData
    {
        public string RrcOpr { get; set; }
        public string RrcLease { get; set; }
        public string LpdLeaseName { get; set; }
        //public DateTime LpdStatusDate { get; set; }
        //public DateTime OprDODateRecvd { get; set; }
        //public DateTime OprDODatePosted { get; set; }
        public bool LpdStatus { get; set; }
    }
}
