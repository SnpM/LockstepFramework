using UnityEngine;
using System.Collections; using FastCollections;
using System.Reflection;
using System;
using System.Linq;
namespace Lockstep
{
    //Note: Ideally used for value types (i.e. Struct)
    public sealed class LSVariable
    {

        public LSVariable (object lockstepObject, PropertyInfo info) {
            
            Init (lockstepObject, info, info.GetCustomAttributes(typeof (LockstepAttribute), true).FirstOrDefault() as LockstepAttribute);
        }

        public LSVariable (object lockstepObject, PropertyInfo info, LockstepAttribute attribute) {
            Init (lockstepObject, info, attribute);
        }

        //Must be PropertyInfo for PropertyInfo .Get[Get/Set]Method ()
        private void Init (object lockstepObject, PropertyInfo info, LockstepAttribute attribute)
        {
            this.Info = info;
            this.LockstepObject = lockstepObject;

            //For the Value property... easier accessbility
            //_getValue = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), info.GetGetMethod().);

            if (DoReset = attribute.DoReset)
            {
               // _setValue = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), info.GetSetMethod());
                //Sets the base value for resetting
                this._baseValue = this.Value;
            }
        }
        public bool DoReset {get; private set;}

        public PropertyInfo Info { get; private set; }
        public object LockstepObject {get; private set;}

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
                return Info.GetValue (LockstepObject, null);
            }
            private set
            {
                Info.SetValue (LockstepObject, value, null);
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