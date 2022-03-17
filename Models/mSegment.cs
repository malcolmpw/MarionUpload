using System;
using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("tblPersonal")]
    internal class mSegment
    {
        [ExplicitKey]
        public int PropID { get; set; }
        [ExplicitKey]
        public int PrsnlID { get; set; }        
                
        public DateTime PrsnlCreateDate { get; set; }
        public string PrsnlCreateBy { get; set; }
        public string PrsnlCreateWhy { get; set; }
        
        public DateTime PrsnlStatDate { get; set; }
        public string PrsnlStatBy { get; set; }
        public string PrsnlStatWhy { get; set; }
        public bool PrsnlStat_YN { get; set; }
        
        public bool delflag { get; set; }
    }
}