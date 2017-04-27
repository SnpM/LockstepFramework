 using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    [System.Serializable]
    public class InputDataItem : DataItem
    {
        public InputDataItem (string name) {
            _name = name;
        }
        public InputDataItem (string name, KeyCode keyCode) {
            _name = name;
        }
    }
}