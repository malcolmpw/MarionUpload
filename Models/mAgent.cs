using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Models
{
    public class mAgent
    {
        //public int OwnerNumber {get; set; }
        public string AgentNumber { get; set; }   //BEG   1 - END   3
        public string AgentName { get; set; }     //BEG   4 - END  33
        public string InCareOf { get; set; }      //BEG  34 - END  64
        public string StreetAddress { get; set; } //BEG  64 - END  93
        public string City { get; set; }          //BEG  94 - END 109
        public string State { get; set; }         //BEG 110 - END 111
        public string Zip { get; set; }           //BEG 112 - END 116
        public string Zip4 { get; set; }          //BEG 118 - END 121
    }
}
