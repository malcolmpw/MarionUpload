using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{
    [Table("tblAccount")]
    public class mAccount
    {
        [Key]
        public int AcctID { get; set; }
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
    }
}
