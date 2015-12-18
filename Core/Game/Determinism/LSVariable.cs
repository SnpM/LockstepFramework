using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

namespace Lockstep
{
    //Note: Ideally used for value types (i.e. Struct)
    internal sealed class LSVariable
    {
        //Must be PropertyInfo for PropertyInfo .Get[Get/Set]Method ()
        public LSVariable(PropertyInfo info)
        {
            this.Info = info;
            //For the Value property... more accessbility
            _getValue = (Func<object>)Delegate.CreateDelegate(typeof (Func<object>),info.GetGetMethod());
            _setValue = (Action<object>)Delegate.CreateDelegate(typeof (Action<object>),info.GetSetMethod());

            //Sets the base value fore resetting
            this._baseValue = this.Value;
        }

        public PropertyInfo Info {get; private set;}

        private object _baseValue;
        object BaseValue {get {return _baseValue;}}

        Func<object> _getValue;
        Action<object> _setValue;
        /// <summary>
        /// Gets or sets the value of the target variable.
        /// </summary>
        /// <value>The value.</value>
        public object Value {
            get {
                return _getValue();
            }
            private set {
                _setValue(value);
            }
        }

        public int Hash () {
            return Value.GetHashCode();
        }

        public bool ValueEquals (LSVariable other) {
            //Compares the hash of the 2 variable Values
            return this.Hash () == other.Hash ();
        }

        /// <summary>
        /// Resets the Value to its value at the creation of this LSVariable.
        /// </summary>
        public void Reset () {
            this.Value = this.BaseValue;
        }
    }
}