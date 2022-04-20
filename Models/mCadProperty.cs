using Dapper.Contrib.Extensions;

namespace MarionUpLoad.Models
{
    [Table("tblCadProperty")]
    public class mCadProperty
    {
        [ExplicitKey]
        public int PropID { get; set; }
        [ExplicitKey]
        public string CadID { get; set; }
        public string CadPropid { get; set; }
        public double CadPct { get; set; }
        public bool delflag { get; set; }
    }
}
