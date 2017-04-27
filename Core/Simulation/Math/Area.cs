using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
	[System.Serializable]
	public struct Area
	{
		public Area (long c1x, long c1y, long c2x, long c2y)
		{
			Corner1 = new Vector2d (c1x, c1y);
			Corner2 = new Vector2d (c2x, c2y);
		}
		public Area (Vector2d corner1, Vector2d corner2) {
			Corner1 = corner1;
			Corner2 = corner2;
		}
		[FixedNumber]
		public Vector2d Corner1;
		[FixedNumber]
		public Vector2d Corner2;

		public long XCorner1 {get {return Corner1.x;}}
		public long XCorner2 {get {return Corner2.x;}}
		public long YCorner1 {get {return Corner1.y;}}
		public long YCorner2 {get {return Corner2.y;}}
		public long XMin {get {return XCorner1 < XCorner2 ? XCorner1 : XCorner2;}}
		public long YMin {get {return YCorner1 < YCorner2 ? YCorner1 : YCorner2;}}
		public long XMax {get {return XCorner1 > XCorner2 ? XCorner1 : XCorner2;}}
		public long YMax {get {return YCorner1 > YCorner2 ? YCorner1 : YCorner2;}}
	}
}
