using System.Collections.Generic;
using Lockstep;
using System.IO;
using System;
using UnityEngine;
using FastCollections;

namespace Lockstep
{
	public static partial class DeltaCache
	{
		static readonly string FilePath = Application.dataPath + "/" + "DeltaCache.cs";
	#region Settings and caching deltas

		const int cacheSize = 20;

		public static void GenerateCache ()
		{
			int radius = cacheSize;
			setupCoordinates.FastClear ();
			sqrRadius = radius * radius;
			for (int i = -radius; i <= radius; i++) {
				for (int j = -radius; j <= radius; j++) {
					sqrMag = i * i + j * j;
					if (sqrMag <= sqrRadius) {
						setupCoordinates.Add (new SetupCoordinate (i, j));
					}
				}
			}
			Array.Sort (setupCoordinates.innerArray, 0, setupCoordinates.Count);
			int CountMinusOne = setupCoordinates.Count - 1;
			using (StreamWriter writer = new StreamWriter(FilePath)) {
				string s = "namespace Lockstep {public static partial class DeltaCache { ";
				s += "public static readonly sbyte[] CacheX = new sbyte [] {";
				for (int i = 0; i < CountMinusOne; i++) {
					s += setupCoordinates [i].x.ToString () + ",";
				}
				s += setupCoordinates [setupCoordinates.Count - 2].x.ToString ();
				s += "};";
				s += "public static readonly sbyte[] CacheY = new sbyte [] {";
				for (int i = 0; i < CountMinusOne; i++) {
					s += setupCoordinates [i].y.ToString () + ",";
				}
				s += setupCoordinates [setupCoordinates.Count - 2].y.ToString ();
				s += "};";
				s += "}}";

				writer.Write (s);
			}
			Debug.Log ("Cache saved in: " + FilePath);
		}
	#endregion

	#region Static Helpers
		static int sqrMag;
		static int sqrRadius;
		static FastList<SetupCoordinate> setupCoordinates = new FastList<SetupCoordinate> (400);
	#endregion

		private struct SetupCoordinate :IComparable
		{
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
}
/*
public struct InfluenceCoordinate
{
	public InfluenceCoordinate (int xPos, int yPos)
	{
		x = xPos;
		y = yPos;
	}

	public readonly int x;
	public readonly int y;

	public int sqrMagnitude {
		get{ return x * x + y * y;}
	}

	public override string ToString ()
	{
		return ("(" + x.ToString () + ", " + y.ToString () + ")");
	}
}*/
