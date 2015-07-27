//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public class GridNode
	{
		public int gridX;
		public int gridY;
		public int gridIndex;
		public int gCost;
		public int hCost;
		public int fCost;
		public GridNode parent;
		public bool Unwalkable;
		public int Weight;
		public GridNode[] NeighborNodes = new GridNode[8];
		static uint[] NeighborUnwalkableVersions = new uint[8];
		public static uint _Version;
		public Vector2d WorldPos;
		public bool Obstructed;
		public bool IsVertex;
		public static readonly bool[] IsNeighborDiagnal = new bool[] {
			true,
			false,
			true,
			false,
			false,
			true,
			false,
			true
		};

		
		#region Variables for data structures
		public uint HeapIndex;
		public bool HeapContained;
		public uint ClosedSetVersion;
		#endregion
		static GridNode ()
		{
			for (i = -1; i <= 1; i++) {
				for (j = -1; j <= 1; j++) {
					
					if (i == 0 && j == 0)
						continue;
					if ((i != 0 && j != 0))
						continue;
 				}
			}
		}

		public GridNode (int _x, int _y)
		{
			gridX = _x;
			gridY = _y;
			gridIndex = gridX * GridManager.NodeCount + gridY;
			WorldPos.x = gridX * FixedMath.One + GridManager.OffsetX;
			WorldPos.y = gridY * FixedMath.One + GridManager.OffsetY;
		}

		public void Initialize ()
		{
			GenerateNeighbors ();
		}

		static int i, j, checkX, checkY, leIndex;

		private void GenerateNeighbors ()
		{
			_Version++;
			for (i = -1; i <= 1; i++) {
				checkX = gridX + i;
				if (checkX >= 0 && checkX < GridManager.NodeCount) {
					for (j = -1; j <= 1; j++) {

						checkY = gridY + j;
						if (checkY >= 0 && checkY < GridManager.NodeCount) {
							GridNode checkNode = GridManager.Grid [GridManager.GetGridIndex (checkX, checkY)];

							if (i == 0 && j == 0)
								continue;

							if (checkNode.Unwalkable) {
								Obstructed = true;
								NeighborUnwalkableVersions [GetUnwalkableIndex (i, j)] = _Version;
							}

							//if ((i != 0 && j != 0)) continue;
							NeighborNodes [GetNeighborIndex (i, j)] = checkNode;
						}
					}
				}
			}

			//CheckIsVertex ();

		}

		private void CheckIsVertex ()
		{
			if (GetNeighborUnwalkable (-1, -1) && !GetNeighborUnwalkable (-1, 0) && !GetNeighborUnwalkable (0, -1)) {
				IsVertex = true;

			} else if (GetNeighborUnwalkable (-1, 1) && !GetNeighborUnwalkable (-1, 0) && !GetNeighborUnwalkable (0, 1)) {
				IsVertex = true;

			} else if (GetNeighborUnwalkable (1, -1) && !GetNeighborUnwalkable (1, 0) && !GetNeighborUnwalkable (0, -1)) {
				IsVertex = true;

			} else if (GetNeighborUnwalkable (1, 1) && !GetNeighborUnwalkable (1, 0) && !GetNeighborUnwalkable (0, 1)) {
				IsVertex = true;
			}
		}

		public static int GetNeighborIndex (int _i, int _j)
		{
			/*
			if (_j == 0) {
				if (_i == -1)
					leIndex = 0;
				else
					leIndex = 3;
			} else {
				if (_j == -1)
					leIndex = 1;
				else
					leIndex = 2;
			}*/
			leIndex = (_i + 1) * 3 + (_j + 1);
			if (leIndex > 3)
				leIndex--;
			return leIndex;
		}

		private static int GetUnwalkableIndex (int _i, int _j)
		{
			leIndex = (_i + 1) * 3 + (_j + 1);
			if (leIndex > 3)
				leIndex--;
			return leIndex;
		}

		public bool GetNeighborUnwalkable (int _i, int _j)
		{
			leIndex = (_i + 1) * 3 + (_j + 1);
			if (leIndex > 3)
				leIndex--;
			return NeighborUnwalkableVersions [leIndex] == _Version;
		}

		static int dstX;
		static int dstY;
		public static int HeuristicTargetX;
		public static int HeuristicTargetY;

		public void CalculateHeuristic ()
		{
			/*
			//Euclidian
			dstX = HeuristicTargetX - gridX;
			dstY = HeuristicTargetY - gridY;
			hCost = (dstX * dstX + dstY * dstY);
			/*if (hCost > 1) {

				n = (hCost / 2) + 1;
				n1 = (n + (hCost / n)) / 2;  
				while (n1 < n) {
					n = n1;  
					n1 = (n + (hCost / n)) / 2;  
				}
				hCost = n;
			}

			fCost = gCost + hCost;
*/

			/*
			if (gridX > HeuristicTargetX)
				dstX = gridX - HeuristicTargetX;
			else
				dstX = HeuristicTargetX - gridX;
			if (gridY > HeuristicTargetY)
				dstY = gridY - HeuristicTargetY;
			else
				dstY = HeuristicTargetY - gridY;

			hCost = (dstX + dstY) * 100;
			fCost = gCost + hCost;
			*/

			if (gridX > HeuristicTargetX)
				dstX = gridX - HeuristicTargetX;
			else
				dstX = HeuristicTargetX - gridX;
			
			if (gridY > HeuristicTargetY)
				dstY = gridY - HeuristicTargetY;
			else
				dstY = HeuristicTargetY - gridY;

			if (dstX > dstY)
				this.hCost = dstY * 141 + (dstX - dstY) * 100;
			else
				this.hCost = dstX * 141 + (dstY - dstX) * 100;

			fCost = gCost + hCost;

		}

		public override int GetHashCode ()
		{
			return this.gridIndex;
		}

		public bool DoesEqual (GridNode obj)
		{
			return obj.gridIndex == this.gridIndex;
		}

	}
}