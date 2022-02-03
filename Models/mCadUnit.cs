using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Models
{
    public class mCadUnit
    {
        public string CadID { get; set; }
        public string UnitID { get; set; }
        public bool CadAppraised { get; set; }
        public bool GlobalUnit { get; set; }
        public bool FirstUnit { get; set; }
        public bool active { get; set; }
    }
}
