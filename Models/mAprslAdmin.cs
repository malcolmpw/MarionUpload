using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{
    [Table("tblAprslAdmin")]
    class mAprslAdmin
    {
        [ExplicitKey]
        public string Year { get; set; }
        [ExplicitKey]
        public int NameID { get; set; }
        [ExplicitKey]
        public string CadID { get; set; }
        public decimal ApprValCad { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}
