using System.Collections.Generic;
using Lockstep;
using System;
public class RangeDelta {
	#region Static Helpers
	static int i,j;
	static int sqrMag;
	static int sqrRadius;
	static FastList<SetupCoordinate> setupCoordinates = new FastList<SetupCoordinate>(400);
	#endregion

	public readonly InfluenceCoordinate[] coordinates;
	public RangeDelta (int radius)
	{
		setupCoordinates.FastClear ();
		sqrRadius = radius * radius;
		for (i = -radius; i <= radius; i++)
		{
			for (j = -radius; j <= radius; j++)
			{
				sqrMag = i * i + j * j;
				if (sqrMag <= sqrRadius)
				{
					setupCoordinates.Add (new SetupCoordinate(i,j));
				}
			}
		}
		Array.Sort (setupCoordinates.innerArray,0,setupCoordinates.Count);

		coordinates = new InfluenceCoordinate[setupCoordinates.Count];
		for (i = 0; i < setupCoordinates.Count; i++)
		     coordinates[i] = new InfluenceCoordinate (setupCoordinates[i].x,setupCoordinates[i].y);
	}

	private struct SetupCoordinate : IComparable{
		public SetupCoordinate (int xPos, int yPos)
		{
			x = xPos;
			y = yPos;
			sqrMagnitude = x * x + y * y;
		}
		public int x;
		public int y;
		public int sqrMagnitude;

		public int CompareTo (System.Object other)
		{
			return sqrMagnitude - ((SetupCoordinate)other).sqrMagnitude;
		}
		public int Compare (SetupCoordinate me, SetupCoordinate other)
		{
			return me.sqrMagnitude - other.sqrMagnitude;
		}
	}
}
public struct InfluenceCoordinate {
	public InfluenceCoordinate (int xPos, int yPos)
	{
		x = xPos;
		y = yPos;
	}
	public readonly int x;
	public readonly int y;
	public int sqrMagnitude {
		get{return x * x + y * y;}
	}
	public override string ToString ()
	{
		return ("(" + x.ToString () + ", " + y.ToString () + ")");
	}
}
