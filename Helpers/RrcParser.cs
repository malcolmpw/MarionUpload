using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarionUpload.Helpers
{
    public class RrcParser
    {
        public string GetRRCnumberFromImportRRCstring(string RRCstring)
        {
            string rrc = RRCstring.Trim();
            string pat = @"(\d+)";
            Regex re = new Regex(pat);
            var match = re.Match(rrc);
            var rrcNumber = "";
            if (match.Success)
            {
                rrcNumber = (match.Groups[0].Value).Trim();
            }

            return rrcNumber;
        }
    }
}
