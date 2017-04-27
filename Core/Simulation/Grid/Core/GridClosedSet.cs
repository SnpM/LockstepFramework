using UnityEngine;
using System.Collections; using FastCollections;
namespace Lockstep {
	public static class GridClosedSet {
		public static uint _Version = 1;

		public static void Add (GridNode item)
		{
			item.ClosedSetVersion = _Version;
		}

		public static bool Contains (GridNode item)
		{
			return item.ClosedSetVersion == _Version;
		}

		static int i;
		public static void FastClear ()
		{
			_Version++;
		}
	}
}