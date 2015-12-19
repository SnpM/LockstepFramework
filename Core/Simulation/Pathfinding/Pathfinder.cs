//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

//Resources:
//Bresenham's Algorithm Implementation: ericw. (Source: http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html)
//AStar Algorithm Template: Sebastian Lague (Source: https://www.youtube.com/watch?v=-L-WgKMFuhE)
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class Pathfinder
	{
		#region Wrapper Variables
		static GridNode node1;
		static GridNode node2;
		static int IndexX, IndexY;
		static FastList<GridNode> TracePath = new FastList<GridNode> ();
		static FastList<GridNode> OutputPath = new FastList<GridNode> ();
		static int length;
		#endregion

		#region Astar Variables
		static GridNode currentNode;
		static GridNode neighbor;
		static int newMovementCostToNeighbor;
		static int i;
		static int StartNodeIndex;
		static bool FindStraight;
		static bool LastIsObstructed;
		#endregion

		#region Broadphase variables
		static int x0, y0, x1, y1;
		static int dx, dy, error, ystep, x, y, t;
		static int compare1, compare2;
		static int retX, retY;
		static bool steep;
		#endregion


		public static bool FindPath (Vector2d Start, Vector2d End, FastList<Vector2d> outputVectorPath, int unitSize = 1)
		{
			if (!GetPathNodes(Start.x,Start.y,End.x,End.y,out node1, out node2))
			    return false;
			if (FindPath (node1, node2, OutputPath, unitSize)) {
				outputVectorPath.FastClear ();
				length = OutputPath.Count - 1;
				for (i = 0; i < length; i++) {
					outputVectorPath.Add (OutputPath [i].WorldPos);
				}
				outputVectorPath.Add (End);
				return true;
			}
			return false;
		}
		public static bool FindPath (Vector2d End, GridNode startNode, GridNode endNode, FastList<Vector2d> outputVectorPath, int unitSize = 1)
		{

			if (startNode.Unwalkable || endNode.Unwalkable) return false;
			if (FindPath (startNode, endNode, OutputPath, unitSize)) {
				outputVectorPath.FastClear ();
				length = OutputPath.Count - 1;
				for (i = 0; i < length; i++) {
					outputVectorPath.Add (OutputPath [i].WorldPos);
				}
				outputVectorPath.Add (End);

				return true;
			}
			return false;
		}
		
        static GridNode startNode;
        static GridNode endNode;
        static FastList<GridNode> outputPath;
        static int unitSize;
        public class Test {
            public bool test () {return false;}

        }

		/// <summary>
		/// Finds a path and outputs it to <c>outputPath</c>. Note: outputPath is unpredictably changed.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if path was found and necessary, <c>false</c> if path to End is impossible or not found.
		/// </returns>
		/// <param name="startNode">Start node.</param>
		/// <param name="endNode">End node.</param>
		/// <param name="outputPath">Return path.</param>
		public static bool FindPath (GridNode _startNode, GridNode _endNode, FastList<GridNode> _outputPath, int _unitSize = 1)
		{

            startNode = _startNode;
            endNode = _endNode;
            outputPath = _outputPath;
            unitSize = _unitSize;

			#region Broadphase and Preperation
			if (endNode.Unwalkable) {
				return false;
			}

			if (startNode.Unwalkable) {
				return false;
			}
			
			outputPath.FastClear ();

			if (System.Object.ReferenceEquals (startNode, endNode)) {
				outputPath.Add (endNode);
				return true;
			}

			GridHeap.FastClear ();
			GridClosedSet.FastClear ();
			#endregion
			
			#region AStar Algorithm
			GridHeap.Add (startNode);
			GridNode.HeuristicTargetX = endNode.gridX;
			GridNode.HeuristicTargetY = endNode.gridY;

            GridNode.PrepareUnpassableCheck(unitSize); //Prepare Unpassable check optimizations
			while (GridHeap.Count > 0) {
				currentNode = GridHeap.RemoveFirst ();

				GridClosedSet.Add (currentNode);
				
				if (currentNode.gridIndex == endNode.gridIndex) {
					//Retraces the path then outputs it into outputPath
					//Also Simplifies the path
                    DestinationReached ();
					return true;
				}

				for (i = 0; i < 8; i++) {
					neighbor = currentNode.NeighborNodes [i];
                    GridNode.CheckNode = neighbor;
                    if (neighbor.IsNull() || currentNode.Unpassable() || GridClosedSet.Contains (neighbor)) {
						continue;
					}

					newMovementCostToNeighbor = currentNode.gCost + (GridNode.IsNeighborDiagnal [i] ? 141 : 100);

					if (!GridHeap.Contains (neighbor)) {
						neighbor.gCost = newMovementCostToNeighbor;
						
						//Optimized heuristic calculation
						neighbor.CalculateHeuristic ();
						neighbor.parent = currentNode;
						
						GridHeap.Add (neighbor);
					} else if (newMovementCostToNeighbor < neighbor.gCost) {
						neighbor.gCost = newMovementCostToNeighbor;

						//Optimized heuristic calculation
						neighbor.CalculateHeuristic ();
						neighbor.parent = currentNode;

						GridHeap.UpdateItem (neighbor);
					}
				}
			}
			#endregion
			return false;
		}

        private static void DestinationReached () {
            outputPath.FastClear ();
            TracePath.FastClear ();

            currentNode = endNode;

            StartNodeIndex = startNode.gridIndex;
            while (currentNode.gridIndex != StartNodeIndex) {
                TracePath.Add (currentNode);
                oldNode = currentNode;
                currentNode = currentNode.parent;
            }

            oldNode = startNode;
            currentNode = TracePath [TracePath.Count - 1];
            oldX = currentNode.gridX - oldNode.gridX;
            oldY = currentNode.gridY - oldNode.gridY;

            for (i = TracePath.Count - 2; i >= 0; i--) {
                oldNode = currentNode;
                currentNode = TracePath.innerArray [i];
                newX = currentNode.gridX - oldNode.gridX;
                newY = currentNode.gridY - oldNode.gridY;

                if (newX != oldX || newY != oldY) {

                    outputPath.Add (oldNode);
                    oldX = newX;
                    oldY = newY;
                }
                //outputPath.Add (currentNode);
            }
            outputPath.Add (endNode);
        }

		public static bool NeedsPath (GridNode startNode, GridNode endNode, int unitSize)
		{
            //Tests if there is a direct path. If there is, no need to run AStar.
			x0 = startNode.gridX;
			y0 = startNode.gridY;
			x1 = endNode.gridX;
			y1 = endNode.gridY;
			if (y1 > y0)
				compare1 = y1 - y0;
			else
				compare1 = y0 - y1;
			if (x1 > x0)
				compare2 = x1 - x0;
			else
				compare2 = x0 - x1;
			steep = compare1 > compare2;
			if (steep) {
				t = x0; // swap x0 and y0
				x0 = y0;
				y0 = t;
				t = x1; // swap x1 and y1
				x1 = y1;
				y1 = t;
			}
			if (x0 > x1) {
				t = x0; // swap x0 and x1
				x0 = x1;
				x1 = t;
				t = y0; // swap y0 and y1
				y0 = y1;
				y1 = t;
			}
			dx = x1 - x0;
				
			dy = (y1 - y0);
			if (dy < 0)
				dy = -dy;
				
			error = dx / 2;
			ystep = (y0 < y1) ? 1 : -1;
			y = y0;
            GridNode.PrepareUnpassableCheck(unitSize);

			for (x = x0; x <= x1; x++) {
				retX = (steep ? y : x);
				retY = (steep ? x : y);
					
				currentNode = GridManager.Grid[retX * GridManager.NodeCount + retY];
                GridNode.CheckNode = currentNode;
                if (currentNode.Unpassable()) {
					break;
				} else if (x == x1) {
					return false;
				}
					
				error = error - dy;
				if (error < 0) {
					y += ystep;
					error += dx;
				}
			}
			return true;
		}

		public static bool GetPathNodes (long StartX, long StartY, long EndX, long EndY, out GridNode startNode, out GridNode endNode)
		{
			startNode = GridManager.GetNode (StartX, StartY);
			if (startNode.Unwalkable) {
				for (i = 0; i < 8; i++) {
					currentNode = startNode.NeighborNodes [i];
					if (System.Object.ReferenceEquals (currentNode, null) == false && currentNode.Unwalkable == false) {
						startNode = currentNode;
						break;
					}
				}
				if (startNode.Unwalkable)
				{
					endNode = null;
					return false;
				}
			}
			endNode = GridManager.GetNode (EndX, EndY);
			if (endNode.Unwalkable) {
				for (i = 0; i < 8; i++) {
					currentNode = endNode.NeighborNodes [i];
					if (System.Object.ReferenceEquals (currentNode, null) == false && currentNode.Unwalkable == false) {
						endNode = currentNode;
						break;
					}
				}
				if (endNode.Unwalkable)
					return false;
			}
			return true;
		}
		static int xSign, ySign;
		public static bool GetPathNode (long X, long Y, out GridNode returnNode)
		{
			returnNode = GridManager.GetNode(X,Y);
			if (returnNode.Unwalkable)
			{
				xSign = X > returnNode.WorldPos.x ? 1 : -1;
				ySign = Y > returnNode.WorldPos.y ? 1 : -1;

				currentNode = GridManager.GetNode (returnNode.gridX + xSign, returnNode.gridY + ySign);
				if (currentNode == null || currentNode.Unwalkable)
				{
					currentNode = GridManager.GetNode (returnNode.gridX + xSign, returnNode.gridY);
					if (currentNode == null || currentNode.Unwalkable)
					{
						currentNode = GridManager.GetNode (returnNode.gridX, returnNode.gridY + ySign);
						if (currentNode == null || currentNode.Unwalkable)
						{
							return false;
						}
					}
				}
				returnNode = currentNode;
			}
			return true;
		}

		static int oldX, oldY;
		static int newX, newY;
		static GridNode oldNode;
	}
}