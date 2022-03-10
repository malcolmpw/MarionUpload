using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Models
{
    [Table("tblCadLease")]
    public class mCadLease
    {
        [ExplicitKey]
        public string CadId { get; set; }
        [ExplicitKey]
        public long LeaseId { get; set; }
        public int CadLeaseId { get; internal set; }
    }
}
