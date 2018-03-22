using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using Lockstep.Data;
using Lockstep.Experimental;
namespace Lockstep.Experimental
{
    [System.Serializable]
	[DataItem (true, 0, false, true, null)]
    public class ResourceDataItem : DataItem
    {
        [SerializeField]
        private Sprite _visual;
        public Sprite Visual { get { return _visual; } }
    }
}