using MarionUpload.Models;
using System.Collections.Generic;

namespace MarionUpload.Comparers
{
    public class PersonalPropertyComparer : IEqualityComparer<mMarionPersonalProperty>
    {
        public bool Equals(mMarionPersonalProperty x, mMarionPersonalProperty y)
        {
            return x.OwnerNumber == y.OwnerNumber;
        }

        public int GetHashCode(mMarionPersonalProperty obj)
        {
            return obj.OwnerNumber.GetHashCode();
        }
    }
}
