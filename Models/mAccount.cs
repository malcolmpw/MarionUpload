using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
