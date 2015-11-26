 //=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class Partition
	{
		#region Settings
		public const int Count = 128;
		public const int ShiftSize = FixedMath.SHIFT_AMOUNT + 2;
		public const long OffsetX = -(1 << ShiftSize) * Count / 2;
		public const long testX = -FixedMath.One * 32;
		public const long OffsetY = -(1 << ShiftSize) * Count / 2;
		#endregion

		public static uint _Version = 1;
		public static PartitionNode[] Nodes = new PartitionNode[Count * Count];
		public static readonly FastBucket<PartitionNode> ActivatedNodes = new FastBucket<PartitionNode>();

		public static void Setup ()
		{
			_Version = 1;
			for (int i = 0; i < Count * Count; i++) {
				Nodes [i] = new PartitionNode ();
			}
		}

		public static void Initialize () {
			ActivatedNodes.FastClear ();
			for (int i = 0; i < Count * Count; i++) {
				Nodes[i].Initialize ();
			}
		}

		static long GridXMin, GridXMax, GridYMin, GridYMax;

        public static void UpdateObject(LSBody Body) {

			GridXMin = (Body.XMin - OffsetX) >> ShiftSize;
			GridXMax = ((Body.XMax - OffsetX) >> ShiftSize);
			GridYMin = ((Body.YMin - OffsetY) >> ShiftSize);
			GridYMax =((Body.YMax - OffsetY) >> ShiftSize);
			#if UNITY_EDITOR
			if (GridXMin < 0 || GridXMax >= Count || GridYMin < 0 || GridYMax >= Count)
			{
				Debug.LogError ("Body with ID " + Body.ID.ToString () + " is out of partition bounds.");
				return;
			}
			#endif
			if (Body.PastGridXMin != GridXMin ||
				Body.PastGridXMax != GridXMax ||
				Body.PastGridYMin != GridYMin ||
				Body.PastGridYMax != GridYMax) {
				for (long o = Body.PastGridXMin; o <= Body.PastGridXMax; o++) {
					for (long p = Body.PastGridYMin; p <= Body.PastGridYMax; p++) {
						PartitionNode node = Nodes [o * Count + p];

						node.Remove (Body.ID);
					}
				}

				for (long i = GridXMin; i <= GridXMax; i++) {
					for (long j = GridYMin; j <= GridYMax; j++) {
						PartitionNode node = Nodes [i * Count + j];

						node.Add (Body.ID);
					}
				}


				Body.PastGridXMin = GridXMin;
				Body.PastGridXMax = GridXMax;
				Body.PastGridYMin = GridYMin;
				Body.PastGridYMax = GridYMax;
			}
		}

		public static void PartitionObject (LSBody Body) {
			GridXMin = ((Body.XMin - OffsetX) >> ShiftSize);
			GridXMax = ((Body.XMax - OffsetX) >> ShiftSize);
			GridYMin = ((Body.YMin - OffsetY) >> ShiftSize);
			GridYMax = ((Body.YMax - OffsetY) >> ShiftSize);
			Body.PastGridXMin = GridXMin;
			Body.PastGridXMax = GridXMax;
			Body.PastGridYMin = GridYMin;
			Body.PastGridYMax = GridYMax;
			for (long i = GridXMin; i <= GridXMax; i++) {
				for (long j = GridYMin; j <= GridYMax; j++) {
					PartitionNode node = Nodes [i * Count + j];
					node.Add (Body.ID);
				}
			}
		}

		static int id1, id2;
		static CollisionPair pair;

		public static void CheckAndDistributeCollisions ()
		{

			_Version++;
			int activatedPeakCount = ActivatedNodes.PeakCount;
			for (int i = 0; i < activatedPeakCount; i++) {
				if (ActivatedNodes.arrayAllocation[i])
				{
					PartitionNode node = ActivatedNodes[i];
					node.Distribute ();
				}
			}

		}
	}
	public enum Quadrant : int
	{
		RightTop,
		RightBot,
		LeftBot,
		LeftTop
	}

}