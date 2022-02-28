using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tblUnitProperty")]
    public class mUnitProperty
    {
        [ExplicitKey]
        public string UnitID { get; set; }
        [ExplicitKey]
        public int PropID { get; set; }
        public double UnitPct { get; set; }
        public bool delflag { get; set; }
    }
}
