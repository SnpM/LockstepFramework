using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public static class LSUtility
	{

		const uint Y = 842502087, Z = 3579807591, W = 273326509;
		public static uint Seed = 1;
		private static uint y = Y, z = Z, w;

		public static void Initialize (uint seed)
		{
			Seed = seed;
			y = Y;
			z = Z;
			w = W;
		}

		public static uint GetRandom (uint Count)
		{
			uint t = (Seed ^ (Seed << 11));
			Seed = y;
			y = z;
			z = w;
			return ((0xFFFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) % Count);
		}

		public static int GetRandom (int Count)
		{
			uint t = (Seed ^ (Seed << 11));
			Seed = y;
			y = z;
			z = w;
			return (int)((0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) % Count);
		}


	}
}