using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lockstep.Data
{
    public struct SortInfo
    {
        public SortInfo(string name, Func<DataItem,int> leDegreeGetter)
        {
            this.sortName = name;
            this.degreeGetter = leDegreeGetter;
        }

        public string sortName;
        public Func<DataItem,int> degreeGetter;
    }
}