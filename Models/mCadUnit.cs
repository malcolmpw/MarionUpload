using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tlkpCadUnit")]
    public class mCadUnit
    {
        [Key]
        public string CadID { get; set; }
        [Key]
        public string UnitID { get; set; }
        public string CadUnitIDText { get; set; }
        public bool CadAppraised { get; set; }
        public bool GlobalUnit { get; set; }
        public bool FirstUnit { get; set; }
        public bool active { get; set; }
    }
}
