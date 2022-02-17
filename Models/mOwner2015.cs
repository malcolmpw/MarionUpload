using System;

namespace MarionUpload.Models
{
    //[Table("tblNameX")]
    public class mOwner2015
    {
        //[Key]
        public string PtdClass { get; set; }
        public string PtdClassSub { get; set; }
        public string PropType { get; set; }
        public string Legal { get; set; }
        public string ControlCad { get; set; }
        public string UpdateWhy { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}