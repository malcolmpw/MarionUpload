using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{
    [Table("AbMarionAccountToImportMap")]
    public class mAccountToImportMap
    {
        [Key]
        public int MapID { get; set; }
        public int AcctID { get; set; }
        public int ImportID { get; set; }
    }
}
