using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("AbMarionImport")]
    public class mMarionOwner
    {
        [Key]
        public int ImportID { get; set; }        
        public int OwnerNumber { get; set; }   
        public string OwnerName { get; set; }     
        public string InCareOf { get; set; }      
        public string StreetAddress { get; set; } 
        public string CityStateZip { get; set; }
        public int AgentNumber { get; set; }  
        public string NameSortCad { get; set; }
        public bool NameSel_YN { get; set; }       
    }
}
