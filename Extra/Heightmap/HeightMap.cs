using UnityEngine;
using System.Collections;
using Lockstep;
namespace Lockstep.Extra
{
    [System.Serializable]
    public class HeightMap
    {
        public HeightMap (short[,] map) {
            _map = (Short2D)Short2D.Clone(map);
        }
        [SerializeField]
        private LayerMask _scanLayers;
        public LayerMask ScanLayers {get {return _scanLayers;}}

        [SerializeField,HideInInspector]
        private Short2D _map = new Short2D();
        public Short2D Map {get {return _map;}}
    }
}