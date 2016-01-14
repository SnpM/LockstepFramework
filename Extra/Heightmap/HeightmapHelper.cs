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

        const int CompressionShift = FixedMath.SHIFT_AMOUNT / 2;


        [SerializeField]
        private HeightMap[] _maps = new HeightMap[1];
        public HeightMap[] Maps {get {return _maps;}}

        public short[,] Scan(int scanLayers)
        {

            int widthPeriods = Size.x.Div(Resolution).CeilToInt();;
            int heightPeriods = Size.y.Div(Resolution).CeilToInt();
            short[,] heightMap = new short[widthPeriods, heightPeriods];

            Vector3 scanPos = _bottomLeft.ToVector3(HeightBounds.y.ToFloat());
            float dist = (HeightBounds.y - HeightBounds.x).ToFloat();

            float fRes = Resolution.ToFloat();
            for (int x = 0; x < widthPeriods; x++)
            {
                scanPos.z = 0;
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
                   
                    heightMap [x, y] = Compress(height);
                    scanPos.z += fRes;
                }
                scanPos.x += fRes;
            }

            return heightMap;
        }

        void OnDrawGizmos () {
            Gizmos.DrawCube(Vector3.zero,Vector3.one);
            float fRes = Resolution.ToFloat();
            Vector3 size = Vector3.one * (fRes * .95f);
            size.y = .1f;
            for (int i = 0; i < Maps.Length; i++) {
                HeightMap map = Maps[i];
                Vector3 drawPos = this.BottomLeft.ToVector3(0);
                for (int x = 0; x < map.Map.Width; x++) {
                    drawPos.z = 0;
                    for (int y = 0; y < map.Map.Height; y++) {
                        drawPos.y = Uncompress(map.Map[x,y]).ToFloat();
                        Gizmos.DrawCube (drawPos,size);
                        drawPos.z += fRes;
                    }
                    drawPos.x += fRes;
                }
            }
        }

        public short Compress (long value) {
            long compressed = value >> CompressionShift;
            if (compressed > short.MaxValue) compressed = short.MaxValue;
            else if (compressed < short.MinValue) compressed = short.MinValue;
            return (short)compressed;
        }
        public long Uncompress (short compressed) {
            return (long)(compressed << CompressionShift);
        }
    }
}