using UnityEngine;
using System.Collections;
using Lockstep;
namespace Lockstep.Extra
{
    [System.Serializable]
    public class HeightMap
    {
        public HeightMap (long[,] map) {
            _map = (Long2D)Long2D.Clone(map);
        }
        [SerializeField]
        private LayerMask _scanLayers;
        public LayerMask ScanLayers {get {return _scanLayers;}}

        [SerializeField,HideInInspector]
        private Long2D _map = new Long2D();
        public Long2D Map {get {return _map;}}
    }
}