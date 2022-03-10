using MarionUpload.Models;
using System.Collections.Generic;

namespace MarionUpload.Comparers
{
    public class PropertyComparer : IEqualityComparer<mMarionProperty>
    {
        public bool Equals(mMarionProperty x, mMarionProperty y)
        {
            return x.LeaseNumber == y.LeaseNumber;
        }

        public int GetHashCode(mMarionProperty obj)
        {
            return obj.LeaseNumber.GetHashCode();
        }
    }
}
