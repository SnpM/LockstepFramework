using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    [System.Serializable]
    public class InputDataItem : DataItem
    {
        public InputDataItem (string name, KeyCode keyCode) {
            _name = name;
            _keyCode = keyCode;
        }

        [SerializeField]
        private KeyCode _keyCode;
        public KeyCode KeyCode {get {return _keyCode;}}
    }
}