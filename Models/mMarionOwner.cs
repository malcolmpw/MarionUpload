using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("AbMarionImport")]
    public class mMarionOwner
    {
        [Key]
        public string ImportID { get; set; }
        //public int OwnerNumber {get; set; }
        public string OwnerNumber { get; set; }   //BEG   1 - END   3
        public string OwnerName { get; set; }     //BEG   4 - END  33
        public string InCareOf { get; set; }      //BEG  34 - END  64
        public string StreetAddress { get; set; } //BEG  64 - END  93
        public string CityStateZip { get; set; }
        public string AgentNumber { get; set; }          //BEG 118 - END 121
    }
}
