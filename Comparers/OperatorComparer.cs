using MarionUpload.Models;
using System.Collections.Generic;

namespace MarionUpload.Comparers
{
    public class OperatorComparer : IEqualityComparer<mMarionOperator>
    {
        public bool Equals(mMarionOperator x, mMarionOperator y)
        {
            return x.OperatorName == y.OperatorName;
        }

        public int GetHashCode(mMarionOperator obj)
        {
            return obj.OperatorName.GetHashCode();
        }
    }
}