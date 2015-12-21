using UnityEngine;
using System.Collections;
using System;

namespace Lockstep.Data {
    [System.AttributeUsage(AttributeTargets.Field)]
    public sealed class RegisterDataAttribute : System.Attribute {
        public RegisterDataAttribute (string displayName) {
            this._displayName = displayName;
        }

        private string _displayName;
        public string DisplayName {get {return _displayName;}}


    }
}