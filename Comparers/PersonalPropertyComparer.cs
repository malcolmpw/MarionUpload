using MarionUpload.Models;
using System;
using System.Collections.Generic;

namespace MarionUpload.Comparers
{
    public class PersonalPropertyComparer : IEqualityComparer<mMarionPersonalProperty>
    {
        public bool Equals(mMarionPersonalProperty x, mMarionPersonalProperty y)
        {
            // return x.LeaseNumber == y.LeaseNumber;
            var tuple1 = new Tuple<int, int>(x.OwnerNumber, x.LeaseNumber);
            var tuple2 = new Tuple<int, int>(y.OwnerNumber, y.LeaseNumber);
            return tuple1 == tuple2;
        }

        public int GetHashCode(mMarionPersonalProperty obj)
        {
            return obj.OwnerNumber.GetHashCode();
            //tuple1.GetHashCode();
            //tuple2.GetHashCode();
        }
    }
}
