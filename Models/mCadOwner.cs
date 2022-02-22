using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tblCadOwners")]
    class mCadOwner
    {
        [Key]
        public int NameID { get; set; }
        [Key]
        public string CadID { get; set; }
        public string CadOwnerID { get; set; }
        public bool delflag { get; set; }
    }
}
