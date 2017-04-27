using UnityEngine;
using System.Collections; using FastCollections;
using System;

namespace Lockstep.Data {
    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public sealed class RegisterDataAttribute : System.Attribute {
        public RegisterDataAttribute (string displayName) {
            this._dataName = displayName;
        }

        private string _dataName;
        public string DataName {get {return _dataName;}}


    }
}