using UnityEngine;
using System.Collections;
using System;

namespace Lockstep.Data {
    [System.AttributeUsage(AttributeTargets.Field)]
    public sealed class RegisterDataAttribute : System.Attribute {
        public RegisterDataAttribute (string displayName/*, params SortInfo[] sorts*/) {
            this._displayName = displayName;
            //_sorts = sorts;
        }

        private string _displayName;
        public string DisplayName {get {return _displayName;}}
        /*
        private SortInfo[] _sorts;
        public SortInfo[] Sorts {get {return _sorts;}}
        */
    }
}