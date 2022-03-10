using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.ViewModels
{
    [Table("tblTract")]
    public class mTract
    {        
        [ExplicitKey]
        public long PropId { get;  set; }
        [ExplicitKey]
        public string TractId { get;  set; }
        public long LeaseId { get; set; }
        public string CadId { get; set; }
        public DateTime UpdateDate { get; set; }
        public string   UpdateBy { get; set; }
        public bool Stat_YN { get; set; }
        public string StatReason { get; set; }
        public DateTime StatDate { get; set; }
        public string StatBy { get; set; }
        public bool delflag { get; set; }
            
    }
}