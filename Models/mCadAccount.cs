using Dapper.Contrib.Extensions;
using System;

namespace MarionDistributeImport.Models
{
    [Table("tblCadAccount")]
    class mCadAccount
    {
        [Key]
        public int AcctID { get; set; }
        [Key]
        public string CadID { get; set; }
        public string CadAcctID { get; set; }
        public bool Lock_YN { get; set; }
        public string ExportCd { get; set; }
        public DateTime ExportDate { get; set; }
        public bool delflag { get; set; }
        public string CadAcctID_pre { get; set; }

    }
}
