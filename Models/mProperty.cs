using Dapper.Contrib.Extensions;
using System;


namespace MarionUpload.Models
{
    [Table("tblProperty")]
    public class mProperty
    {
        [Key]
        public int PropId { get; set; }
        public string PtdClass { get; set; }
        public string PtdClassSub { get; set; }
        public string PropType { get; set; }
        public string Legal { get; set; }
        public string ControlCad { get; set; }
        public string UpdateWhy { get; set; }
        public string CreateBy { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string CreateWhy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
