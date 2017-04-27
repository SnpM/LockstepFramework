using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    [System.Serializable]
    public class PathObjectDataItem : MetaDataItem
    {
        [SerializeField]
        private PathObject _object;

        public PathObject Object { get { return _object; } }
    }
}