using MarionUpload.Models;
using System.Collections.Generic;

namespace MarionUpload.Comparers
{
    public class MineralPropertyComparer : IEqualityComparer<mMarionMineralProperty>
    {
        public bool Equals(mMarionMineralProperty x, mMarionMineralProperty y)
        {
            return x.LeaseNumber == y.LeaseNumber;
        }

        public int GetHashCode(mMarionMineralProperty obj)
        {
            return obj.LeaseNumber.GetHashCode();
        }
    }
}
