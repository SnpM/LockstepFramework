 //=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace Lockstep
{
	public static class Partition
	{
		#region Settings
		public const int DefaultCount = 64;
        public const int ShiftSize = FixedMath.SHIFT_AMOUNT + 2;
        public static int BoundX {get; private set;} //Lower bound X
        public static int BoundY {get; private set;} //Lower bound Y
		#endregion

		public static uint _Version = 1;
        public static Array2D<PartitionNode> Nodes = new Array2D<PartitionNode> (DefaultCount, DefaultCount);
		public static readonly FastBucket<PartitionNode> ActivatedNodes = new FastBucket<PartitionNode>();
        private static readonly FastList<PartitionNode> AllocatedNodes = new FastList<PartitionNode>();

		public static void Setup ()
		{
			_Version = 1;


            BoundX = -DefaultCount / 2;
            BoundY = -DefaultCount / 2;

		}

		public static void Initialize () {
            ActivatedNodes.FastClear ();
            for (int i = AllocatedNodes.Count - 1; i >= 0; i--) {
                AllocatedNodes[i].Reset ();
			}
		}

		static int GridXMin, GridXMax, GridYMin, GridYMax;

        public static void UpdateObject (LSBody Body) {
            GetGridBounds (Body);


			if (Body.PastGridXMin != GridXMin ||
				Body.PastGridXMax != GridXMax ||
				Body.PastGridYMin != GridYMin ||
				Body.PastGridYMax != GridYMax) {
				for (int o = Body.PastGridXMin; o <= Body.PastGridXMax; o++) {
					for (int p = Body.PastGridYMin; p <= Body.PastGridYMax; p++) {
                        PartitionNode node = GetNode(o,p);
						node.Remove (Body.ID);
					}
				}

				for (int i = GridXMin; i <= GridXMax; i++) {
					for (int j = GridYMin; j <= GridYMax; j++) {
                        PartitionNode node = GetNode(i,j);

						node.Add (Body.ID);
					}
				}


				Body.PastGridXMin = GridXMin;
				Body.PastGridXMax = GridXMax;
				Body.PastGridYMin = GridYMin;
				Body.PastGridYMax = GridYMax;
			}
		}

        private static void GetGridBounds (LSBody Body) {
            GridXMin = GetGridX (Body.XMin);
            GridXMax = GetGridX (Body.XMax);
            GridYMin = GetGridY (Body.YMin);
            GridYMax = GetGridY (Body.YMax);
            int iterationCount = 0;
            while (CheckSize (GridXMin, GridXMax, GridYMin, GridYMax)) {
                iterationCount ++;
                if (iterationCount >= 5) {
                    break;
                }
                GridXMin = GetGridX (Body.XMin);
                GridXMax = GetGridX (Body.XMax);
                GridYMin = GetGridY (Body.YMin);
                GridYMax = GetGridY (Body.YMax);
            }
        }
            
		public static void PartitionObject (LSBody Body) {
            GetGridBounds (Body);

			Body.PastGridXMin = GridXMin;
			Body.PastGridXMax = GridXMax;
			Body.PastGridYMin = GridYMin;
			Body.PastGridYMax = GridYMax;

			for (int i = GridXMin; i <= GridXMax; i++) {
				for (int j = GridYMin; j <= GridYMax; j++) {
                    PartitionNode node = GetNode(i,j);
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
        private static bool CheckSize (int gridXMin, int gridXMax, int gridYMin, int gridYMax) {
            if (GridXMin < 0 || GridXMax >= Nodes.Width || GridYMin < 0 || GridYMax >= Nodes.Height)
            {
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

        public static int GetGridX (long xPos) {
            return (int)((xPos) >> ShiftSize) - BoundX;
        }
        public static int GetGridY (long yPos) {
            return (int)((yPos) >> ShiftSize) - BoundY;
        }

        public static PartitionNode GetNode (int indexX, int indexY) {
            PartitionNode node = Nodes[indexX,indexY];
            if (node.IsNull ())
            {
                node = new PartitionNode();
                Nodes[indexX,indexY] = node;
            }
            return node;
        }

		public static void CheckAndDistributeCollisions ()
		{

			_Version++;
            for (int i = ActivatedNodes.PeakCount - 1; i >= 0; i--) {
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