using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tlkpCadUnit")]
    public class mTlkpUnit
    {
        [ExplicitKey]
        public string UnitID { get; set; }
        public string UnitName { get; set; }
        public string UnitCntyCode { get; set; }
        public string UnitTypeCode { get; set; }
        public bool UnitMultiCntyFlag { get; set; }
        public string AppraisalDistrict { get; set; }
        [ExplicitKey]
        public string CadID { get; set; }
    }
}
