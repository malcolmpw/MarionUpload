using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tblCadOwners")]
    public class mCadOwner
    {
        [ExplicitKey]
        public int NameID { get; set; }
        [ExplicitKey]
        public string CadID { get; set; }
        public string CadOwnerID { get; set; }
        public bool delflag { get; set; }
    }
}
