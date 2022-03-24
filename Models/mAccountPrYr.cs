using Dapper.Contrib.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;
//using Dapper.SimpleCRUD;
//using Dapper.Contrib;

namespace MarionUpload.Models
{
    [Table("tlkpAccountPrYr")]
    public class mAccountPrYr
    {
        public string AcctLegal { get; set; }
        public char PctType { get; set; }
        public float PctProp { get; set; }
        public bool Protest_YN { get; set; }
        public string PTDcode { get; set; }
        public long NameID { get; internal set; }
        public long PropID { get; internal set; }
        public string Cad { get; set; }
        public bool Stat_YN { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public decimal ValAcctCur { get; set; }
        public decimal ValAcctCrt { get; set; }
        public decimal valacctPrYr { get; set; }
        public decimal AcctValPrYr { get; set; }
        public bool ValAcctLock { get; set; }
        public char division { get; set; }
        public bool ProtestResolved_YN { get; set; }
        public bool Supp_YN { get; set; }
        public bool BatchProtest_YN { get; set; }
        public bool BatchWithdraw_YN { get; set; }
        public bool Prc { get; set; }
        public bool delflag { get; set; }
        public bool corr_yn { get; set; }
    }
}

