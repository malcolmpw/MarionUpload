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
        
        public string NameH { get; set; }
        public string NameF { get; set; }
        public string NameM { get; set; }
        public string NameL1 { get; set; }
        public string NameL2 { get; set; }
        public string NameLS { get; set; }
        public string NameT { get; set; }
        public string NameC { get; set; }
        public string NameCP { get; set; }
        public bool NameSel_YN { get; set; }
        public string Name2 { get; set; }
        public string NameSort { get; set; }
        public string NameSortFirst { get; set; }
        public string NameSortCad { get; set; }        
        public string Mail1 { get; set; } 
        public string MailCi { get; set; }
        public string MailSt { get; set; }
        public string MailZ { get; set; } 
        public string MailZ4 { get; set; }
        public string MailCo { get; set; }
        public string MailZip { get; set; }
        public int AgentID { get; set; }
        public bool Agnt_YN { get; set; } 
        public string Addr1 { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool Ntc2Agent_YN { get; set; }
        public bool Stmnt2Agent_YN { get; set; }
        public bool Stat_YN { get; set; }
    }
}
