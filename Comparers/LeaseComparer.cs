﻿using MarionUpload.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarionUpload.Comparers
{
   public  class LeaseComparer : IEqualityComparer<mMarionLease>
    {
        public bool Equals(mMarionLease x, mMarionLease y)
        {
            return x.LeaseNumber == y.LeaseNumber;
        }

        public int GetHashCode(mMarionLease obj)
        {
            return obj.LeaseNumber.GetHashCode();
        }
    }
}
