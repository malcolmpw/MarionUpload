using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{
    [Table("tblLease")]
    public class mLease
    {
        [Key]
        public int LeaseID { get; set; }
        public string CadPropID { get; set; }
        public int LeaseOprID { get; set; }
        public string LeaseNameWag { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool Stat_YN { get; set; }
        public string StatReason { get; set; }
        public DateTime StatDate { get; set; }
        public string StatBy { get; set; }
        [Computed]
        public string RrcLease { get; set; }
    }
}
