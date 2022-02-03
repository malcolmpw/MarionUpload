using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Models
{
    public class mUnit
    {
        public string UnitID { get; set; }
        public string UnitName { get; set; }
        public string UnitCntyCode { get; set; }
        public string UnitTypeCode { get; set; }
        public bool UnitMultiCntyFlag { get; set; }
        public string AppraisalDistrict { get; set; }
        public string CadID { get; set; }
    }
}
