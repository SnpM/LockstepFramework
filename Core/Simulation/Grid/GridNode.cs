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
		public bool[] NeighborDiagnal = new bool[8];
		public int NeighborCount = 0;
		public Vector2d WorldPos;
		public bool Obstructed;

		#region Variables for data structures
		public uint HeapIndex;
		public bool HeapContained;
		public uint ClosedSetVersion;
		#endregion

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
		static int i,j,checkX,checkY,leIndex;
		private void GenerateNeighbors ()
		{

			for ( i = -1; i <= 1; i++) {	
				checkX = gridX + i;
				if (checkX >= 0 && checkX < GridManager.NodeCount) {
					for ( j = -1; j <= 1; j++) {
						if (i == 0 && j == 0)
							continue;
						//if (i != 0 && j != 0) continue;

						checkY = gridY + j;
						if (checkY >= 0 && checkY < GridManager.NodeCount) {
							GridNode checkNode = GridManager.Grid [GridManager.GetGridIndex (checkX, checkY)];
							if (checkNode.Unwalkable) Obstructed = true;
							GetNeighborIndex (i,j);
							if (i != 0)
							{
								if (j != 0)
								{
									NeighborDiagnal[leIndex] = (true);
								}
								else {
									NeighborDiagnal[leIndex] = (false);
								}
							}
							else {
								NeighborDiagnal[leIndex] = (false);
							}
							NeighborNodes[leIndex] = checkNode;
							NeighborCount++;
						}
					}
				}
			}
		}
							public static int GetNeighborIndex (int _i, int _j)
							{
								leIndex = (_i + 1) * 3 + (_j + 1);
								if (leIndex > 4) leIndex--;
								return leIndex;
							}

		static int dstX;
		static int dstY;
		public static int HeuristicTargetX;
		public static int HeuristicTargetY;
		public void CalculateHeurustic ()
		{
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

			/*
			if (dstX > dstY) this.hCost = dstY * 141 + (dstX - dstY) * 100;
			else this.hCost = dstX * 141 + (dstY - dstX) * 100;

			fCost = gCost + hCost;
*/
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