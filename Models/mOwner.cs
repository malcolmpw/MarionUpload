using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{

    [Table("tblName")]
    public class mOwner
    {
        [Key]
        public int NameID { get; set; }
        public string CadID { get; set; }       
        public string NameSort { get; set; }     //BEG   4 - END  33
        public string Mail1 { get; set; } //streetaddress
        public string MailCi { get; set; }  // Maps to CityStateZip
        public string MailSt { get; set; }   // Maps to CityStateZip
        public string MailZ { get; set; }    // Maps to CityStateZip
        public string MailZ4 { get; set; }  // Maps to CityStateZip
        public string MailCo { get; set; }  // Maps to Country
        public string MailZip { get; set; }  // Maps to CityStateZip
        public string AgentID { get; set; } //BEG 118 - END 121
        public bool Agent_YN { get; set; } // based on if AgentNumber is 0 or not.
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }   
        public string NameSortCad { get; set; }
        public bool NameSel_YN { get; set; }
    }
}
