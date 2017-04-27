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
using System.Collections; using FastCollections;
using System.Collections.Generic;

namespace Lockstep.Pathfinding
{
	public static class Pathfinder
	{
		public const int SmallSize = 2;
		public const int MediumSize = 3;

		#region Wrapper Variables

		static GridNode node1;
		static GridNode node2;
		static int IndexX, IndexY;
		static FastList<GridNode> TracePath = new FastList<GridNode>();
		static FastList<GridNode> OutputPath = new FastList<GridNode>();
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
		static bool hasInvalidEdge;

		#endregion

		#region Broadphase variables


		#endregion


		//todo: implement this with SmoothPath
		public static bool FindPath(Vector2d Start, Vector2d End, FastList<Vector2d> outputVectorPath, int unitSize = 1)
		{
			if (!GetPathNodes(Start.x, Start.y, End.x, End.y, out node1, out node2))
				return false;
			if (FindRawPath(node1, node2, OutputPath, unitSize)) {
				SmoothPath(OutputPath, End, outputVectorPath, unitSize);
				return true;
			}
			return false;
		}
		public static void SmoothPath(FastList<GridNode> nodePath, Vector2d End, FastList<Vector2d> outputVectorPath, int unitSize)
		{
			outputVectorPath.FastClear();
			length = nodePath.Count - 1;
			//culling out unneded nodes


			var StartNode = nodePath[0];
			outputVectorPath.Add(StartNode.WorldPos);
			GridNode oldNode = StartNode;
			long oldX = 0;
			long oldY = 0;
			long newX = 0;
			long newY = 0;
			for (i = 1; i < length; i++) {

				GridNode node = nodePath[i];

				bool important = false;
				if (unitSize <= SmallSize) {
					important = !node.Clearance;
				} else if (unitSize <= MediumSize) {
					important = !node.ExtraClearance;
				} else {
					important = true;
				}
				//important = true;
				if (important) {

					newX = node.gridX - oldNode.gridX;
					newY = node.gridY - oldNode.gridY;
					if (
						(newX <= 1 && newX >= -1) &&
						(newY <= 1 && newY >= -1)
					) {
						if (newX == oldX && newY == oldY) {
							if (oldX != 0 || oldY != 0) {
								outputVectorPath.RemoveAt(outputVectorPath.Count - 1);
							}
						} else {
							oldX = newX;
							oldY = newY;
						}
					} else {
						oldX = 0;
						oldY = 0;
					}
					outputVectorPath.Add(node.WorldPos);

					oldNode = node;

				}
			}
			outputVectorPath.Add(End);

		}
		public static bool FindPath(Vector2d End, GridNode startNode, GridNode endNode, FastList<Vector2d> outputVectorPath, int unitSize = 1)
		{

			if (FindRawPath(startNode, endNode, OutputPath, unitSize)) {
				SmoothPath(OutputPath, End, outputVectorPath, unitSize);

				return true;
			}
			return false;
		}


		public class Test
		{
			public bool test()
			{
				return false;
			}

		}

		#region method sharing variables
		static GridNode startNode;
		static GridNode endNode;
		static FastList<GridNode> outputPath;
		static int unitSize;
		#endregion
		/// <summary>                        
		/// Finds a path and outputs it to <c>outputPath</c>. Note: outputPath is unpredictably changed.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if path was found and necessary, <c>false</c> if path to End is impossible or not found.
		/// </returns>
		/// <param name="startNode">Start node.</param>
		/// <param name="endNode">End node.</param>
		/// <param name="outputPath">Return path.</param>
		public static bool FindRawPath(GridNode _startNode, GridNode _endNode, FastList<GridNode> _outputPath, int _unitSize = 1)
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

			outputPath.FastClear();

			if (System.Object.ReferenceEquals(startNode, endNode)) {
				outputPath.Add(endNode);
				return true;
			}

			GridHeap.FastClear();
			GridClosedSet.FastClear();
			#endregion

			#region AStar Algorithm
			GridHeap.Add(startNode);
			GridNode.HeuristicTargetX = endNode.gridX;
			GridNode.HeuristicTargetY = endNode.gridY;

			GridNode.PrepareUnpassableCheck(unitSize); //Prepare Unpassable check optimizations
			if (_endNode.Unwalkable) {
				return false;
			}
			while (GridHeap.Count > 0) {
				currentNode = GridHeap.RemoveFirst();
#if false
				Gizmos.DrawCube(currentNode.WorldPos.ToVector3(), Vector3.one);
#endif

				if (currentNode.gridIndex == endNode.gridIndex) {
					//Retraces the path then outputs it into outputPath
					//Also Simplifies the path
					DestinationReached();
					return true;
				}


				/*
				for (i = 0; i < 8; i++) {
					neighbor = currentNode.NeighborNodes [i];
					if (CheckNeighborInvalid ()) {
						//continue;
						//microoptimization... continue is more expensive than letting the loop pass at the end
					} else {
						//0-3 = sides, 4-7 = diagonals
						if (i < 4) {
							newMovementCostToNeighbor = currentNode.gCost + 100;
						} else {
							if (i == 4) {
								if (!GridManager.UseDiagonalConnections)
									break;
							}
							newMovementCostToNeighbor = currentNode.gCost + 141;
						}

						AnalyzeNode();
					}
				}
				*/
				hasInvalidEdge = false;
				for (int i = 0; i < 4; i++) {
					neighbor = currentNode.NeighborNodes[i];
					if (CheckNeighborInvalid()) {
						hasInvalidEdge = true;
					} else {
						newMovementCostToNeighbor = currentNode.gCost + 100;
						AnalyzeNode();
					}
				}

				if (hasInvalidEdge) {
					const int maxCornerObstructions = 2;
					#region inlining diagonals
					neighbor = currentNode.NeighborNodes[4];
					if (!CheckNeighborInvalid()) {
						if (GetObstructionCount(0, 1) <= maxCornerObstructions) {
							newMovementCostToNeighbor = currentNode.gCost + 141;
							AnalyzeNode();
						}
					}

					neighbor = currentNode.NeighborNodes[5];
					if (!CheckNeighborInvalid()) {
						if (GetObstructionCount(0, 2) <= maxCornerObstructions) {
							newMovementCostToNeighbor = currentNode.gCost + 141;
							AnalyzeNode();
						}
					}
					neighbor = currentNode.NeighborNodes[6];
					if (!CheckNeighborInvalid()) {
						if (GetObstructionCount(3, 1) <= maxCornerObstructions) {
							newMovementCostToNeighbor = currentNode.gCost + 141;
							AnalyzeNode();
						}
					}
					neighbor = currentNode.NeighborNodes[7];
					if (!CheckNeighborInvalid()) {
						if (GetObstructionCount(3, 2) <= maxCornerObstructions) {
							newMovementCostToNeighbor = currentNode.gCost + 141;
							AnalyzeNode();
						}
					}
					#endregion
				} else {
					//no need for specific stuff when edges are all valid
					for (int i = 4; i < 8; i++) {
						neighbor = currentNode.NeighborNodes[i];
						if (CheckNeighborInvalid()) {
						} else {
							newMovementCostToNeighbor = currentNode.gCost + 141;
							AnalyzeNode();
						}
					}
				}
				GridClosedSet.Add(currentNode);

			}
			#endregion
			return false;
		}

		static int GetObstructionCount(int index1, int index2)
		{
			if (CheckInvalid(currentNode.NeighborNodes[index1])) {
				if (CheckInvalid(currentNode.NeighborNodes[index2])) {
					return 2;
				}
				return 1;
			}
			if (CheckInvalid(currentNode.NeighborNodes[index2]))
				return 1;
			return 0;
		}

		static bool CheckInvalid(GridNode gridNode)
		{
			return gridNode.IsNull() || GridClosedSet.Contains(gridNode) || gridNode.Unpassable();
		}

		static bool CheckNeighborInvalid()
		{
			return neighbor.IsNull() || GridClosedSet.Contains(neighbor) || neighbor.Unpassable();
		}

		static void AnalyzeNode()
		{
			if (!GridHeap.Contains(neighbor)) {
				neighbor.gCost = newMovementCostToNeighbor;

				//Optimized heuristic calculation
				neighbor.CalculateHeuristic();
				neighbor.parent = currentNode;

				GridHeap.Add(neighbor);
			} else if (newMovementCostToNeighbor < neighbor.gCost) {
				neighbor.gCost = newMovementCostToNeighbor;

				//Optimized heuristic calculation
				neighbor.CalculateHeuristic();
				neighbor.parent = currentNode;

				GridHeap.UpdateItem(neighbor);
			}
		}

		private static void DestinationReached()
		{

			outputPath.FastClear();
			TracePath.FastClear();

			currentNode = endNode;

			//GridNode oldNode = null;

			StartNodeIndex = startNode.gridIndex;
			while (currentNode.gridIndex != StartNodeIndex) {
				TracePath.Add(currentNode);
				//oldNode = currentNode;
				currentNode = currentNode.parent;

			}

			currentNode = TracePath[TracePath.Count - 1];
			for (i = TracePath.Count - 2; i >= 0; i--) {
				//oldNode = currentNode;
				currentNode = TracePath.innerArray[i];
				outputPath.Add(currentNode);
			}
		}

		public static bool NeedsPath(GridNode startNode, GridNode endNode, int unitSize)
		{
			int dx, dy, error, ystep, x, y, t;
			int x0, y0, x1, y1;
			int compare1, compare2;
			int retX, retY;
			bool steep;

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

				currentNode = GridManager.Grid[GridManager.GetGridIndex(retX, retY)];
				if (currentNode != null && currentNode.Unpassable()) {
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

		public static bool GetPathNodes(long StartX, long StartY, long EndX, long EndY, out GridNode startNode, out GridNode endNode)
		{
			startNode = GridManager.GetNode(StartX, StartY);
			if (startNode.Unwalkable) {
				for (i = 0; i < 8; i++) {
					currentNode = startNode.NeighborNodes[i];
					if (System.Object.ReferenceEquals(currentNode, null) == false && currentNode.Unwalkable == false) {
						startNode = currentNode;
						break;
					}
				}
				if (startNode.Unwalkable) {
					endNode = null;
					return false;
				}
			}
			endNode = GridManager.GetNode(EndX, EndY);
			if (endNode.Unwalkable) {
				for (i = 0; i < 8; i++) {
					currentNode = endNode.NeighborNodes[i];
					if (System.Object.ReferenceEquals(currentNode, null) == false && currentNode.Unwalkable == false) {
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

		public static bool GetPathNode(long x, long y, out GridNode returnNode)
		{
			pathNodeX = x;
			pathNodeY = y;
			returnNode = GridManager.GetNode(x, y);

			if (returnNode == null || returnNode.Unwalkable) {
				int xGrid, yGrid;
				GridManager.GetCoordinates(x, y, out xGrid, out yGrid);
				const int maxTestDistance = 3;
				closestNode = null;

				//raycast on grid in 4 direction: Left, right, up, and down
				for (int i = xGrid - 1; i >= xGrid - maxTestDistance; i--) {
					CheckPathNode(GridManager.GetNode(i, yGrid));
					if (castNodeFound) break;
				}

				for (int i = xGrid + 1; i <= xGrid + maxTestDistance; i++) {
					CheckPathNode(GridManager.GetNode(i, yGrid));
					if (castNodeFound) break;

				}
				for (int i = yGrid + 1; i <= yGrid + maxTestDistance; i++) {
					CheckPathNode(GridManager.GetNode(xGrid, i));
					if (castNodeFound) break;
				}

				for (int i = yGrid - 1; i >= yGrid - maxTestDistance; i--) {
					CheckPathNode(GridManager.GetNode(xGrid, i));
					if (castNodeFound) break;
				}

				if (closestNode == null) {
					return false;
				}
				returnNode = closestNode;
			}

			return true;
		}
		static long pathNodeX, pathNodeY;
		static GridNode closestNode;
		static long closestDistance;
		static bool castNodeFound;
		static void CheckPathNode(GridNode node)
		{
			if (node != null && node.Unwalkable == false) {
				long distance = node.WorldPos.FastDistance(pathNodeX, pathNodeY);
				if (closestNode == null || distance < closestDistance) {
					closestNode = node;
					closestDistance = distance;
					castNodeFound = true;
				} else {
					castNodeFound = false;
				}
			}
		}


        public static bool GetClosestViableNode(Vector2d from, Vector2d dest, int pathingSize, out GridNode returnNode)
        {
            returnNode = GridManager.GetNode(dest.x, dest.y);
            if (returnNode.Unwalkable)
            {
                bool valid = false;
                PanLineAlgorithm.FractionalLineAlgorithm.Coordinate cacheCoord = new PanLineAlgorithm.FractionalLineAlgorithm.Coordinate();
                bool validTriggered = false;
                pathingSize = (pathingSize + 1) / 2;
                int minSqrMag = pathingSize * pathingSize;
                minSqrMag *= 2;
                foreach (var coordinate in PanLineAlgorithm.FractionalLineAlgorithm.Trace(dest.x.ToDouble(), dest.y.ToDouble(), from.x.ToDouble(), from.y.ToDouble()))
                {
                    currentNode = GridManager.GetNode(FixedMath.Create(coordinate.X), FixedMath.Create(coordinate.Y));
                    if (!validTriggered)
                    {
                        if (currentNode != null && currentNode.Unwalkable == false)
                        {
                            validTriggered = true;
                        }
                        else
                            cacheCoord = coordinate;
                    }
                    if (validTriggered)
                    {
                        if (currentNode == null || currentNode.Unwalkable)
                        {
                        }
                        else
                        {
                            //calculate sqrMag to last invalid node
                            int testMag = coordinate.X - cacheCoord.X;
                            testMag *= testMag;
                            int buffer = coordinate.Y - cacheCoord.Y;
                            buffer *= buffer;
                            testMag += buffer;
                            if (testMag >= minSqrMag)
                            {
                                valid = true;
                                break;
                            }
                        }
                    }
                }
                if (!valid)
                {
                    return false;
                }
                returnNode = currentNode;
            }
            return true;
        }
    }
}