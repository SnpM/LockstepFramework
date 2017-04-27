using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    public struct Coordinate : ICommandData
    {
        public int x;
        public int y;

        public Coordinate(int X, int Y)
        {
            x = X;
            y = Y;
        }

		public static Coordinate FloorToCoordinate (Vector2d v2) {
			Coordinate coor;
			coor.x = v2.x.ToInt();
			coor.y = v2.y.ToInt();
			return coor;
		}
		public static Coordinate RoundToCoordinate (Vector2d v2) {
			Coordinate coor;
			coor.x = v2.x.RoundToInt();
			coor.y = v2.y.RoundToInt();
			return coor;
		}
		public static Coordinate CeilToCoordinate (Vector2d v2) {
			Coordinate coor;
			coor.x = v2.x.CeilToInt();
			coor.y = v2.y.CeilToInt();
			return coor;
		}

        public override string ToString()
        {
            return "(" + x.ToString() + ", " + y.ToString() + ")";
        }

        public void Write (Writer writer) {
            writer.Write (x);
            writer.Write (y);
        }
        public void Read (Reader reader) {
            this.x = reader.ReadInt ();
            this.y = reader.ReadInt();
        }

        #if UNITY_EDITOR
        public void OnSerializeGUI()
        {
            x = EditorGUILayout.IntField("X", x);
            y = EditorGUILayout.IntField("Y", y);
        }
        #endif
    }
}