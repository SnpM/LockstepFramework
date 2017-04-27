using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System;

namespace Lockstep.Data
{
    public delegate int DataItemSorter(DataItem item);

    public class SortInfo
    {
        

        public SortInfo(string name, DataItemSorter sorter)
        {
            
            this._sortName = name;
            this._degreeGetter = sorter;
        }

        public string _sortName;
        public DataItemSorter _degreeGetter;
        public string sortName {get {return _sortName;}}
        public DataItemSorter degreeGetter {get {return _degreeGetter;}}
    }
}