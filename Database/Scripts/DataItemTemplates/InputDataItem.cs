 using UnityEngine;
using System.Collections;

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