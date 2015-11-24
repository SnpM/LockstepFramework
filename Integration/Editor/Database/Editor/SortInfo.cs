using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lockstep.Data
{
    public struct SortInfo<T>
    {
        public SortInfo(string name, Func<T,int> leDegreeGetter)
        {
            this.sortName = name;
            this.degreeGetter = leDegreeGetter;
        }

        public string sortName;
        public Func<T,int> degreeGetter;
    }
}