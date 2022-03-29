using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{
    [Table("tblTract")]
    public class mTract
    {        
        [ExplicitKey]
        public long PropID { get;  set; }
        [ExplicitKey]
        public string TractID { get;  set; }
        public long LeaseID { get; set; }
        public string CadID { get; set; }
        public double LeasePct { get; set; }
        public DateTime UpdateDate { get; set; }
        public string   UpdateBy { get; set; }
        public bool Stat_YN { get; set; }
        public string StatReason { get; set; }
        public DateTime StatDate { get; set; }
        public string StatBy { get; set; }
        public bool delflag { get; set; }
            
    }
}