using UnityEngine;
using System.Collections;

namespace Lockstep
{
	[System.Serializable]
	public struct Area
	{
		public Area (long xcorner1, long ycorner1, long xcorner2, long ycorner2) {
			XCorner1 = xcorner1;
			YCorner1 = ycorner1;
			XCorner2 = xcorner2;
			YCorner2 = ycorner2;
		}
		[FixedNumber]
		public long XCorner1;
		[FixedNumber]
		public long YCorner1;
		[FixedNumber]
		public long XCorner2;
		[FixedNumber]
		public long YCorner2;
		public long XMin {get {return XCorner1 < XCorner2 ? XCorner1 : XCorner2;}}
		public long YMin {get {return YCorner1 < YCorner2 ? YCorner1 : YCorner2;}}
		public long XMax {get {return XCorner1 > XCorner2 ? XCorner1 : XCorner2;}}
		public long YMax {get {return YCorner1 > YCorner2 ? YCorner1 : YCorner2;}}
	}
}
