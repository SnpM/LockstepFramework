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


//TODO: System for combining paths. i.e. 1st path is found, 2nd path finds way onto 1st path, 2nd path returns rest of 1st path
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

        #region CombinePath Variables
        static uint combinePathVersion;
        #endregion
        #region Broadphase variables


        #endregion


        public static bool FindPath(Vector2d Start, Vector2d End, FastList<Vector2d> outputVectorPath, int unitSize = 1, uint combinePathVersion = 0)
        {
            if (!GetPathNodes(Start.x, Start.y, End.x, End.y, out node1, out node2))
                return false;
            if (FindRawPath(node1, node2, OutputPath, unitSize, combinePathVersion)) {
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
        public static bool FindPath(Vector2d End, GridNode startNode, GridNode endNode, FastList<Vector2d> outputVectorPath, int unitSize = 1, uint combinePathsVersion = 0)
        {

            if (FindRawPath(startNode, endNode, OutputPath, unitSize, combinePathsVersion)) {
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
        public static bool FindRawPath(GridNode _startNode, GridNode _endNode, FastList<GridNode> _outputPath, int _unitSize, uint _combinePathVersion)
        {
            //TODO: Not critical but there's a lot of room for better organization
            //i.e. All these static variables and methods goes into individual singleton classes
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
            combinePathVersion = _combinePathVersion;
            #endregion

            #region AStar Algorithm
            GridHeap.Add(startNode);
            GridNode.HeuristicTargetX = endNode.gridX;
            GridNode.HeuristicTargetY = endNode.gridY;



            GridNode.PrepareUnpassableCheck(unitSize); //Prepare Unpassable check optimizations
            if (_endNode.Unwalkable) {
                return false;
            }
            destinationIsReached = false;
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


                #region idk why this is here. I'll find out and delete later
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
                #endregion

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
            return destinationIsReached;
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
                AddBestNode();
                GridHeap.Add(neighbor);
            } else if (newMovementCostToNeighbor < neighbor.gCost) {
                AddBestNode();
                GridHeap.UpdateItem(neighbor);
            }
        }
        
        static void AddBestNode ()
        {
            neighbor.gCost = newMovementCostToNeighbor;

            //Optimized heuristic calculation
            neighbor.CalculateHeuristic();
            neighbor.parent = currentNode;

            //CombinePaths
            if (combinePathVersion != 0)
            {
                if (neighbor.CombinePathVersion == combinePathVersion)
                {
                    //We found our way onto the previous path!
                    DestinationReached();
                }
            }
        }

        static bool destinationIsReached;
        private static void DestinationReached()
        {
            //Faster to do this to end loop than to check every AnalyzeNode () call
            destinationIsReached = true;
            GridHeap.FastClear();

            outputPath.FastClear();
            TracePath.FastClear();

            currentNode = endNode;

            //GridNode oldNode = null;

            StartNodeIndex = startNode.gridIndex;
            while (currentNode.gridIndex != StartNodeIndex) {
                //Sets CombinePathVersion while tracing path
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

        /// <summary>
        /// Pathfinding queries require 2 valid nodes. When one is not valid, this is used to find the best nearest node to path to instead.
        /// </summary>
        private class AlternativeNodeFinder
        {
            public static AlternativeNodeFinder Instance = new AlternativeNodeFinder();
            int XGrid, YGrid, MaxTestDistance;
            GridNode closestNode;
            bool castNodeFound;
            Vector2d WorldPos;
            Vector2d OffsettedPos;
            public void SetValues(Vector2d worldPos, int xGrid, int yGrid, int maxTestDistance)
            {
                XGrid = xGrid;
                YGrid = yGrid;
                MaxTestDistance = maxTestDistance;
                WorldPos = worldPos;
                OffsettedPos = GridManager.GetOffsettedPos(worldPos) ;
                closestNode = null;
                castNodeFound = false;
                layer = 1;
            }
            int dirX, dirY;
            int layer;
            public GridNode GetNode ()
            {

                //Calculated closest side to raycast in first
                long xDif = OffsettedPos.x - XGrid;
                xDif = xDif.ClampOne();
                long yDif = OffsettedPos.y - YGrid;
                yDif = yDif.ClampOne();
                long nodeHalfWidth = FixedMath.One / 2;
                //Check to see if we should raycast towards corner first
                if ((xDif.Abs() >= nodeHalfWidth / 2) &&
                    (yDif.Abs() >= nodeHalfWidth / 2))
                {
                    dirX = FixedMath.RoundToInt(xDif);
                    dirY = FixedMath.RoundToInt(yDif);
                }
                else
                {
                    if (xDif.Abs() < yDif.Abs())
                    {
                        dirX = 0;
                        dirY = yDif.RoundToInt();
                    }
                    else
                    {
                        dirX = xDif.RoundToInt();
                        dirY = 0;
                    }
                }

                int layerStartX = dirX,
                    layerStartY = dirY;
                int iterations = 0; // <- this is for debugging
                for (layer = 1; layer <= this.MaxTestDistance;)
                {
                    GridNode checkNode = GridManager.GetNode(XGrid + dirX, YGrid + dirY);
                    if (checkNode != null)
                    {
                        this.CheckPathNode(checkNode);
                        if (this.castNodeFound)
                        {
                            return this.closestNode;
                        }
                    }
                    AdvanceRotation();
                    //If we make a full loop
                    if (layerStartX == dirX && layerStartY == dirY)
                    {
                        layer++;
                        //Advance a layer instead of rotation
                        if (dirX > 0) dirX = layer;
                        else if (dirX < 0) dirX = -layer;
                        if (dirY > 0) dirY = layer;
                        else if (dirY < 0) dirY = -layer;
                        layerStartX = dirX;
                        layerStartY = dirY;
                    }
                    iterations++;
                    if (iterations > 500)
                    {
                        Debug.Log("tew many");
                        break;
                    }
                }
                
                //If the cast node is found or the side has been checked, do not raycast on that side

                if (!castNodeFound)
                    return null;
                return closestNode;
            }
            //Advances the rotation clockwise
            void AdvanceRotation ()
            {
                //sides
                if (dirX == 0)
                {
                    //up
                    if (dirY == 1)
                        dirX = layer;
                    //down
                    else
                        dirX = -layer;
                }
                else if (dirY == 0)
                {
                    //right
                    if (dirX == 1)
                        dirY = -layer;
                    //left
                    else
                        dirY = layer;
                }
                //corners
                else if (dirX > 0)
                {
                    //top-right
                    if (dirY > 0)
                        dirY = 0;
                    //bot-right
                    else
                        dirX = 0;
                }
                else
                {
                    //top-left
                    if (dirY > 0)
                        dirX = 0;
                    else
                        dirY = 0;
                }
            }

            long closestDistance;
            void CheckPathNode(GridNode node)
            {
                if (node != null && node.Unwalkable == false)
                {
                    long distance = node.WorldPos.FastDistance(this.WorldPos);
                    if (closestNode == null || distance < closestDistance)
                    {
                        closestNode = node;
                        closestDistance = distance;
                        castNodeFound = true;
                    }
                    else
                    {
                        castNodeFound = false;
                    }
                }
            }
        }

        /// <summary>
        /// Finds closest next-best-node also when destination is off the grid
        /// </summary>
        /// <param name="from"></param>
        /// <param name="dest"></param>
        /// <param name="returnNode"></param>
        /// <returns></returns>
		public static bool GetPathNode(Vector2d from, Vector2d dest, out GridNode returnNode)
		{
			returnNode = GridManager.GetNode(dest.x, dest.y);
            if (returnNode == null)
            {
                //If null, it is off the grid. Raycast back onto grid for closest viable node to the destination.
                foreach (var coordinate in PanLineAlgorithm.FractionalLineAlgorithm.Trace(
                    dest.x.ToDouble(), dest.y.ToDouble(), from.x.ToDouble(), from.y.ToDouble()))
                {
                    returnNode = GridManager.GetNode(
                        FixedMath.Create(coordinate.X), FixedMath.Create(coordinate.Y));
                    if (returnNode != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (returnNode.Unwalkable)
            {
                return StarCast(dest, out returnNode);
            }
			return true;
		}

        /// <summary>
        /// Finds closest next-best-node
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="returnNode"></param>
        /// <returns></returns>
        public static bool GetPathNode(Vector2d dest, out GridNode returnNode)
        {
            returnNode = GridManager.GetNode(dest.x, dest.y);
            if (returnNode == null || returnNode.Unwalkable)
            {
                return StarCast(dest, out returnNode);
            }
            return true;
        }
        public static bool StarCast(Vector2d dest, out GridNode returnNode)
        {
            int xGrid, yGrid;
            GridManager.GetCoordinates(dest.x, dest.y, out xGrid, out yGrid);
            const int maxTestDistance = 3;
            AlternativeNodeFinder.Instance.SetValues(
                dest,
                xGrid, yGrid, maxTestDistance);
            returnNode = AlternativeNodeFinder.Instance.GetNode();
            if (returnNode == null)
            {
                return false;
            }
            return true;
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