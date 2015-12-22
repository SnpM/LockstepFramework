using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using System.Linq;
namespace Lockstep
{
    //Note: Ideally used for value types (i.e. Struct)
    public sealed class LSVariable
    {

        public LSVariable (PropertyInfo info) {
            Init (info, info.GetCustomAttributes(typeof (LockstepAttribute), true).FirstOrDefault() as LockstepAttribute);
        }

        public LSVariable (PropertyInfo info, LockstepAttribute attribute) {
            Init (info, attribute);
        }

        //Must be PropertyInfo for PropertyInfo .Get[Get/Set]Method ()
        private void Init (PropertyInfo info, LockstepAttribute attribute)
        {
            this.Info = info;
            //For the Value property... easier accessbility
            _getValue = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), info.GetGetMethod());

            if (DoReset = attribute.DoReset)
            {
                _setValue = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), info.GetSetMethod());
                //Sets the base value for resetting
                this._baseValue = this.Value;
            }
        }
        public bool DoReset {get; private set;}

        public PropertyInfo Info { get; private set; }

        private object _baseValue;

        object BaseValue { get { return _baseValue; } }

        Func<object> _getValue;
        Action<object> _setValue;

        /// <summary>
        /// Gets or sets the value of the target variable.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get
            {
                return _getValue();
            }
            private set
            {
                _setValue(value);
            }
        }

        public int Hash()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Resets the Value to its value at the creation of this LSVariable.
        /// </summary>
        public void Reset()
        {
            if (DoReset)
            this.Value = this.BaseValue;
        }
    }
}