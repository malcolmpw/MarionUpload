using Dapper.Contrib.Extensions;

namespace MarionUpLoad.Models
{
    [Table("tblCadProperty")]
    class mCadProperty
    {
        [Key]
        public int PropID { get; set; }
        [Key]
        public string CadID { get; set; }
        public string CadPropid { get; set; }
        public double CadPct { get; set; }
        public bool delflag { get; set; }
    }
}
