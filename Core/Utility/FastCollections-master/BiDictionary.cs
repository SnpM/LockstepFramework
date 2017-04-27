using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FastCollections
{
    public class BiDictionary<T1,T2> : Dictionary<T1,T2>
    {
        private Dictionary<T2,T1> _reverseMap = new Dictionary<T2, T1>();
        public new void Add (T1 item1, T2 item2) {
            base.Add(item1,item2);
            _reverseMap.Add(item2,item1);
        }
        public void Remove (T1 item1, T2 item2) {
            base.Remove(item1);
            _reverseMap.Remove (item2);
        }

        public T1 GetReversed (T2 key) {
            return _reverseMap[key];
        }
        public bool TryGetValueReversed (T2 key, out T1 value) {
            return _reverseMap.TryGetValue(key,out value);
        }
    }
}