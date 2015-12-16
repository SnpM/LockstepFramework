using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public sealed class GridSettings : MonoBehaviour
    {
        [SerializeField]
        private Vector2d _mapCenter;
        public Vector2d Offset {
            get 
            {
                return _mapCenter - new Vector2d(_mapWidth / 2, _mapHeight / 2);
            }
        }


        [SerializeField]
        private int _mapWidth = 100;
        public int MapWidth {get {return _mapWidth;}}
        [SerializeField]
        private int _mapHeight = 100;
        public int MapHeight {get {return _mapHeight;}}


        #if UNITY_EDITOR
        public bool Show;
        public float ShowHeight;

        void OnDrawGizmos () {
            if (!Show) return;
            Gizmos.color = Color.green;
            Vector3 offset = Offset.ToVector3(ShowHeight);
            Vector3 scale = Vector3.one * .5f;
            for (int x = 0; x < MapWidth; x++) {
                for (int y = 0; y < MapHeight; y++) {
                    Vector3 drawPos = new Vector3 (x,0f,y);
                    drawPos += offset;
                    Gizmos.DrawCube(drawPos,scale);
                }
            }
        }
        #endif       
    }
}