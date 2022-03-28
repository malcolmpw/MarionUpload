using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tblCadLease")]
    public class mCadLease
    {
        [ExplicitKey]
        public string CadId { get; set; }
        [ExplicitKey]
        public long LeaseId { get; set; }
        public string CadLeaseId { get; internal set; }
    }
}
