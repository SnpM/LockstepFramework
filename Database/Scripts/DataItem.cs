using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lockstep.Data
{
    [System.Serializable]
    public class DataItem
    {
        public DataItem() {}

        public int MappedCode {get {return this.Name.GetHashCode ();}}
        [SerializeField]
        protected string
            _name;
        
        public string Name {
            get { return _name; } 
            set {_name = value;}
        }


        public override string ToString ()
        {
            return Name;
        }
        
        public override int GetHashCode ()
        {
            return Name.GetHashCode ();
        }

        public void Manage () {
            OnManage ();
        }
        protected virtual void OnManage () {

        }

        public void Inject (params object[] data) {
            OnInject (data);
        }

        protected virtual void OnInject (object[] data) {

        }

     
    }

   
}