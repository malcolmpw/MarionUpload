using MarionUpload.Models;
using System.Collections.Generic;

namespace MarionUpload.Comparers
{
    public class OperatorComparer : IEqualityComparer<mMarionOperator>
    {
        public bool Equals(mMarionOperator x, mMarionOperator y)
        {
            return x.MarionOperatorName == y.MarionOperatorName;
        }

        public int GetHashCode(mMarionOperator obj)
        {
            return obj.MarionOperatorName.GetHashCode();
        }
    }
}