using Dapper.Contrib.Extensions;

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
    }
}
