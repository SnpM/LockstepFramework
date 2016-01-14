using UnityEngine;
using System.Collections;

namespace Lockstep.Extra
{
    public class HeightmapHelper : BehaviourHelper
    {
        [SerializeField]
        private Vector2d _size = new Vector2d(100,100);
        public Vector2d Size {get {return _size;}}

        [SerializeField]
        private Vector2d _heightBounds = new Vector2d(-20,50);
        public Vector2d HeightBounds {get {return _heightBounds;}}

        [SerializeField]
        private Vector2d _bottomLeft = new Vector2d(-50,-50);

        public Vector2d BottomLeft { get { return _bottomLeft; } }

        /// <summary>
        /// Resolution scans per Unity meter
        /// </summary>
        [SerializeField,FixedNumber]
        private long _resolution = FixedMath.Half;
        public long Resolution { get { return _resolution; } }



        [SerializeField]
        private HeightMap[] _maps = new HeightMap[1];
        public HeightMap[] Maps {get {return _maps;}}

        public long[,] Scan(int scanLayers)
        {
            int widthPeriods = Size.x.Div(Resolution).CeilToInt();;
            int heightPeriods = Size.y.Div(Resolution).CeilToInt();
            long[,] heightMap = new long[widthPeriods, heightPeriods];

            Vector3 scanPos = _bottomLeft.ToVector3(HeightBounds.y);
            float dist = (HeightBounds.y - HeightBounds.x).ToFloat();

            float fRes = Resolution.ToFloat();
            for (int x = 0; x < widthPeriods; x++)
            {
                for (int y = 0; y < heightPeriods; y++)
                {
                    RaycastHit hit;
                    long height;
                    if (Physics.Raycast(scanPos, Vector3.down, out hit, dist, scanLayers, QueryTriggerInteraction.UseGlobal))
                    {
                        height = FixedMath.Create(hit.point.y);
                    } else
                    {
                        height = HeightBounds.x;
                    }
                    heightMap [x, y] = height;
                    scanPos.y += fRes;
                }
                scanPos.x += fRes;
            }

            return heightMap;
        }
    }
}