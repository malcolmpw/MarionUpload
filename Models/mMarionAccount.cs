using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionDistributeImport.Models
{
    [Table("AbMarionImport")]
    public class mMarionAccount
    {
        [Key]
        public int ImportID { get; set; }
        public int OwnerNumber { get; set; }
        public int LeaseNumber { get; set; }
        public int InterestType { get; set; }
        public string SPTBCode { get; set; }
        public string Protest { get; set; }
        public float DecimalInterest { get; set; }
        public int AccountNumber { get; set; }
        public int AccountSequence { get; set; }
    }
}
