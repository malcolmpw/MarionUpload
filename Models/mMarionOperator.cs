using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("AbMarionOperators")]
    public class mMarionOperator
    {
        [Key]
        public int MarionOperatorId { get; set; }
        public string OperatorName { get; set; }
        public string CompanyNameSub { get; set; }
        public string CompanyName { get; set; }
        public int CompanyID { get; set; }
        public bool OperatorFlag { get; set; }
        public bool Active { get; set; }
    }
}
