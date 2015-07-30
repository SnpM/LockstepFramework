using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class InfluenceManager
	{

		public static void Initialize ()
		{

		}

		public static void Simulate ()
		{

		}

		public static int GenerateDeltaCount (int radius)
		{
			return (int)((radius * radius * FixedMath.Pi) / FixedMath.One);
		}
	}
}