//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System;
namespace Lockstep
{
	public static class Partition
	{
		#region Settings
		public const int DefaultCount = 512;
		public const int AdditionalShiftSize = 2;
		public const int ShiftSize = FixedMath.SHIFT_AMOUNT + AdditionalShiftSize;
		public static int BoundX { get; private set; } //Lower bound X
		public static int BoundY { get; private set; } //Lower bound Y
													   //Offset due to partition nodes being centered on grid positions
		public const long Offset = (1 << ShiftSize) / 2;

		#endregion

		public static uint _Version = 1;
		public static Array2D<PartitionNode> Nodes = new Array2D<PartitionNode> (DefaultCount, DefaultCount);
		private static readonly FastBucket<PartitionNode> ActivatedNodes = new FastBucket<PartitionNode> ();
		private static readonly FastList<PartitionNode> AllocatedNodes = new FastList<PartitionNode> ();

		public static void Setup ()
		{
			_Version = 1;


			BoundX = -DefaultCount / 2;
			BoundY = -DefaultCount / 2;

		}

		public static void Initialize ()
		{
			for (int i = AllocatedNodes.Count - 1; i >= 0; i--) {
				AllocatedNodes [i].Reset ();
			}

			ActivatedNodes.FastClear ();
			AllocatedNodes.FastClear ();
		}

		public static void Deactivate ()
		{
		}

		static int GridXMin, GridXMax, GridYMin, GridYMax;

		public static void UpdateObject (LSBody Body, bool repartition = true)
		{

			GetGridBounds (Body);

			if (
				repartition == false ||
				(Body.PastGridXMin != GridXMin ||
				Body.PastGridXMax != GridXMax ||
				Body.PastGridYMin != GridYMin ||
			     Body.PastGridYMax != GridYMax)) {

				for (int o = Body.PastGridXMin; o <= Body.PastGridXMax; o++) {
					for (int p = Body.PastGridYMin; p <= Body.PastGridYMax; p++) {
						PartitionNode node = GetNode (o, p);
						if (Body.Immovable) {
							node.RemoveImmovable (Body.ID);
						} else {
							node.Remove (Body.ID);
						}
					}
				}
				if (repartition) {
					PartitionObject (Body, true);
				}

			}
		}

		private static void GetGridBounds (LSBody Body)
		{
			GridXMin = GetGridX (Body.XMin);
			GridXMax = GetGridX (Body.XMax);
			GridYMin = GetGridY (Body.YMin);
			GridYMax = GetGridY (Body.YMax);
			int iterationCount = 0;
			while (CheckSize (GridXMin, GridXMax, GridYMin, GridYMax)) {
				iterationCount++;
				if (iterationCount >= 5) {
					break;
				}
				GridXMin = GetGridX (Body.XMin);
				GridXMax = GetGridX (Body.XMax);
				GridYMin = GetGridY (Body.YMin);
				GridYMax = GetGridY (Body.YMax);
			}
		}

		public static void PartitionObject (LSBody Body, bool gridBoundsCalculated = false)
		{
			if (gridBoundsCalculated == false)
				GetGridBounds (Body);

			Body.PastGridXMin = GridXMin;
			Body.PastGridXMax = GridXMax;
			Body.PastGridYMin = GridYMin;
			Body.PastGridYMax = GridYMax;

			for (int i = GridXMin; i <= GridXMax; i++) {
				for (int j = GridYMin; j <= GridYMax; j++) {
					PartitionNode node = GetNode (i, j);
					if (Body.Immovable)
                    {
                        node.AddImmovable (Body.ID);
                    }
					else
						node.Add (Body.ID);

				}
			}
		}
		/// <summary>
		/// Returns true if size changed. False if not.
		/// </summary>
		/// <returns><c>true</c>, if size was checked, <c>false</c> otherwise.</returns>
		/// <param name="gridXMin">Grid X minimum.</param>
		/// <param name="gridXMax">Grid X max.</param>
		/// <param name="gridYMin">Grid Y minimum.</param>
		/// <param name="gridYMax">Grid Y max.</param>
		private static bool CheckSize (int gridXMin, int gridXMax, int gridYMin, int gridYMax)
		{
			if (GridXMin < 0 || GridXMax >= Nodes.Width || GridYMin < 0 || GridYMax >= Nodes.Height) {
				int boundXMin = Math.Min (GridXMin, 0) + BoundX;
				int boundXMax = Math.Max (GridXMax + 1, Nodes.Width) + BoundX;
				int boundYMin = Math.Min (GridYMin, 0) + BoundY;
				int boundYMax = Math.Max (GridYMax + 1, Nodes.Height) + BoundY;

				int newWidth = boundXMax - boundXMin;
				int newHeight = boundYMax - boundYMin;

				Nodes.Resize (newWidth, newHeight);
				int xShift = BoundX - boundXMin;
				int yShift = BoundY - boundYMin;
				Nodes.Shift (xShift, yShift);

				BoundX = boundXMin;
				BoundY = boundYMin;

				//Populating new array slots
				//TODO: Optimize? Any clean way to do this? Worth it?
				//DERP! Just allocate nodes as needed.

				/*for (int i = Nodes.InnerArray.Length - 1; i >= 0; i--) {
                    if (Nodes.InnerArray[i] == null)
                        Nodes.InnerArray[i] = new PartitionNode();
                }*/

				return true;
			}
			return false;
		}

		static int id1, id2;
		static CollisionPair pair;

		/// <summary>
		/// World pos to partition index.
		/// </summary>
		/// <returns>The grid x.</returns>
		/// <param name="xPos">X position.</param>
		public static int GetGridX (long xPos)
		{
			xPos += Offset;
			return (int)((xPos) >> ShiftSize) - BoundX;
		}
		public static int GetGridY (long yPos)
		{
			yPos += Offset;
			return (int)((yPos) >> ShiftSize) - BoundY;
		}
		/// <summary>
		/// World pos to relative position on grid.
		/// </summary>
		/// <returns>The relative x.</returns>
		/// <param name="xPos">X position.</param>
		public static long GetRelativeX (long xPos)
		{
			return (xPos >> AdditionalShiftSize) - (FixedMath.Create (BoundX));
		}
		public static long GetRelativeY (long yPos)
		{
			return (yPos >> AdditionalShiftSize) - (FixedMath.Create (BoundY));
		}
		/// <summary>
		/// Index to world position.
		/// </summary>
		/// <returns>The world x.</returns>
		/// <param name="gridX">Grid x.</param>
		public static long GetWorldX (int gridX)
		{
			return (FixedMath.Create (gridX + BoundX)) << AdditionalShiftSize;
		}
		public static long GetWorldY (int gridY)
		{
			return (FixedMath.Create (gridY + BoundY)) << AdditionalShiftSize;
		}
		public static bool CheckValid (int indexX, int indexY)
		{
			return indexX >= 0 && indexX < Nodes.Width && indexY >= 0 && indexY < Nodes.Height;
		}
		public static PartitionNode GetNode (int indexX, int indexY)
		{
			PartitionNode node = Nodes [indexX, indexY];
			if (node.IsNull ()) {
				node = new PartitionNode ();
				Nodes [indexX, indexY] = node;
			}
			return node;
		}

		public static int count;
		public static void CheckAndDistributeCollisions ()
		{
			count = 0;
			_Version++;
			for (int i = ActivatedNodes.PeakCount - 1; i >= 0; i--) {
				if (ActivatedNodes.arrayAllocation [i]) {
					PartitionNode node = ActivatedNodes [i];
					node.Distribute ();
				}
			}
			//Debug.Log (count + " pairs checked");
		}

		public static int AddNode (PartitionNode node)
		{
			int activationID = ActivatedNodes.Add (node);
			AllocatedNodes.Add (node);
			return activationID;
		}

		public static void RemoveNode (int id)
		{
			ActivatedNodes.RemoveAt (id);
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