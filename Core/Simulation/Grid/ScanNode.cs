using UnityEngine;
using System.Collections;
namespace Lockstep{
	public class ScanNode {

		public ScanNode (int x, int y)
		{
			X = x;
			Y = y;
		}

		public FastBucket<LSAgent> LocatedAgents = new FastBucket<LSAgent> ();
		public int X;
		public int Y;
	}
}