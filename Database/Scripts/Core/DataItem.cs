using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System;

namespace Lockstep.Data
{
    [System.Serializable]
    public class DataItem
    {
        public DataItem (string name) {
            _name = name;
        }
        public DataItem() {}

        [SerializeField]
        protected string
            _name;
        
        public string Name {
            get { return _name; } 
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