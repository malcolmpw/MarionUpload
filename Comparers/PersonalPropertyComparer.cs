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
            var tupleX = new Tuple<int, int>(x.OwnerNumber, x.LeaseNumber);
            var tupleY = new Tuple<int, int>(y.OwnerNumber, y.LeaseNumber);
            return tupleX == tupleY;
        }

        public int GetHashCode(mMarionPersonalProperty obj)
        {
            return obj.OwnerNumber.GetHashCode();
            //tuple1.GetHashCode();
            //tuple2.GetHashCode();
        }
    }
}
