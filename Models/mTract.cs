using Dapper.Contrib.Extensions;
using System;
using System.Runtime.ConstrainedExecution;

namespace MarionUpload.Models
{
    [Table("tblTract")]
    public class mTract
    {
        [ExplicitKey]
        public long PropID { get; set; }
        [ExplicitKey]
        public string TractID { get; set; }
        public long LeaseID { get; set; }
        public string CadID { get; set; }
        public double LeasePct { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool Stat_YN { get; set; }
        //public string Memo { get; set; }
        public string StatReason { get; set; }
        public DateTime StatDate { get; set; }
        public string StatBy { get; set; }
        public bool delflag { get; set; }

        //public decimal well78Cur { get; set; }
        //public decimal well18Cur { get; set; }

        //public string OprLeaseName { get; set; }
        //public string OprLeaseID { get; set; }
        //public string OprDOID { get; set; }
        //public DateTime OprDODate { get; set; }
        //public DateTime OprDODateRecvd { get; set; }
        //public DateTime OprDODatePosted { get; set; }
        //public string OprDOPostBy { get; set; }
        //public bool OprNotWI_YN { get; set; }

    }
}