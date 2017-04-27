using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;
namespace Lockstep
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
        private Short2D _map;
        public Short2D Map {get {return _map;}}

        public long GetHeight (int gridX, int gridY) {
            if (!_map.IsValidIndex(gridX,gridY))
                return 0;
            return HeightmapSaver.Uncompress(_map[gridX,gridY]);
        }
    }
}