using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tblUnitProperty")]
    public class mUnitProperty
    {
        [Key]
        public string UnitID { get; set; }
        [Key]
        public int PropID { get; set; }
        public double UnitPct { get; set; }
        public bool delflag { get; set; }
    }
}
