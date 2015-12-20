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
        #region Constructor

        static GridNode()
        {
            /*for (i = -1; i <= 1; i++) {
				for (j = -1; j <= 1; j++) {
					
					if (i == 0 && j == 0)
						continue;
					if ((i != 0 && j != 0))
						continue;
				}
			}*/
        }

        public GridNode()
        {

        }

        public void Setup(int _x, int _y)
        {
            gridX = _x;
            gridY = _y;
            gridIndex = GridManager.GetGridIndex(gridX, gridY);
            WorldPos.x = gridX * FixedMath.One + GridManager.OffsetX;
            WorldPos.y = gridY * FixedMath.One + GridManager.OffsetY;

        }

        public void Initialize()
        {

            this.FastInitialize();

            GenerateNeighbors();
            LinkedScanNode = GridManager.GetScanNode(gridX / GridManager.ScanResolution, gridY / GridManager.ScanResolution);
        }

        public void FastInitialize()
        {
            this.ClosedSetVersion = 0;
            this.HeapIndex = 0;
            this.HeapVersion = 0;
        }

        #endregion

        #region

        static int _i;

        #endregion

        #region Collection Helpers

        public uint ClosedSetVersion;
        public uint HeapVersion;
        public uint HeapIndex;

        #endregion


        #region Pathfinding

        public int gridX;
        public int gridY;
        public int gridIndex;

        public int gCost
        {
            get
            {
                return _gCost + Weight;
            }
            set
            {
                _gCost = value;
            }
        }

        private int _gCost;
        public int hCost;
        public int fCost;
        public GridNode parent;
        private byte _obstacleCount;

        public byte ObstacleCount
        {
            get
            {

                return _obstacleCount;
            }
        }

        public void UpdateUnwalkable()
        {
            _unwalkable = this._obstacleCount > 0;
        }

        private bool _unwalkable;

        public bool Unwalkable
        {
            get
            {
                return _unwalkable;
            }
        }

        public void AddObstacle()
        {
            #if DEBUG
            if (this._obstacleCount == byte.MaxValue)
            {
                Debug.LogErrorFormat("Too many obstacles on this node ({0})!", new Coordinate(this.gridX, this.gridY));
            }
            #endif
            this._obstacleCount++;
            this.UpdateUnwalkable();
        }

        public void RemoveObstacle()
        {
            if (this._obstacleCount == 0)
            {
                Debug.LogErrorFormat("No obstacle to remove on this node ({0})!", new Coordinate(this.gridX, this.gridY));
            }
            this._obstacleCount--;
            this.UpdateUnwalkable();
        }

        static int CachedSize;
        static int CachedLargeDeltaCount;
        static Func<bool> CachedUnpassableFunction;

        public static void PrepareUnpassableCheck(int size)
        {
            CachedSize = size;
            CachedLargeDeltaCount = GridManager.GenerateDeltaCount((CachedSize + 1) / 2);

            /*if (CachedSize == 1)
                CachedUnpassableFunction = () => false;
            else if (CachedSize <= 3) {
                CachedUnpassableFunction = () => CheckNode.UnpassableMedium ();
            }
            else {
                CachedUnpassableFunction = () => CheckNode.UnpassableLarge ();
            }*/
        }

        public bool Unpassable()
        {
            if (this._unwalkable)
                return true;
            if (CachedSize == 1)
            {
                return false;
            }
            if (CachedSize <= 3)
            {
                return UnpassableMedium();
            } else
            {
                return UnpassableLarge();
            }
        }

        public bool Unpassable(int size)
        {
            PrepareUnpassableCheck(size);
            return this.Unpassable();
        }

        public bool UnpassableNormal()
        {
            return false;
        }

        public bool UnpassableMedium()
        {
            for (_i = 0; _i < 8; i++)
            {
                GridNode node = NeighborNodes [_i];
                if (node != null)
                if (node._unwalkable)
                    return true;
            }
            return false;
        }

        public bool UnpassableLarge()
        {
            for (_i = 1; _i < CachedLargeDeltaCount; _i++)
            {
                GridNode node = GridManager.GetNode(DeltaCache.CacheX [_i] + this.gridX, DeltaCache.CacheY [_i] + this.gridY);
                if (node.Unwalkable)
                    return true;

            }
            return false;
        }

        public int Weight;
        public GridNode[] NeighborNodes = new GridNode[8];
        public Vector2d WorldPos;
        public static readonly bool[] IsNeighborDiagnal = new bool[]
        {
            true,
            false,
            true,
            false,
            false,
            true,
            false,
            true
        };

        private void GenerateNeighbors()
        {
            for (i = -1; i <= 1; i++)
            {
                checkX = gridX + i;

                for (j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    //if ((i != 0 && j != 0)) continue; //Disables diagnal connections 
                    checkY = gridY + j;
                    if (GridManager.ValidateCoordinates(checkX, checkY))
                    {
                        
                        GridNode checkNode = GridManager.Grid [GridManager.GetGridIndex(checkX, checkY)];
                        NeighborNodes [GetNeighborIndex(i, j)] = checkNode;
                    }
                }
            }
			
			
        }

        public static int GetNeighborIndex(int _i, int _j)
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

		
        static int dstX;
        static int dstY;
        public static int HeuristicTargetX;
        public static int HeuristicTargetY;

        public void CalculateHeuristic()
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

        #endregion


        #region Influence

        public int ScanX { get { return LinkedScanNode.X; } }

        public int ScanY { get { return LinkedScanNode.Y; } }

        public ScanNode LinkedScanNode;
        const int weightPerUnit = 100;

        public void Add(LSInfluencer influencer)
        {
            //Weight += weightPerUnit;
            LinkedScanNode.Add(influencer);
        }

        public void Remove(LSInfluencer influencer)
        {
            //Weight -= weightPerUnit;
            LinkedScanNode.Remove(influencer);
        }

        #endregion

        static int i, j, checkX, checkY, leIndex;

        public override int GetHashCode()
        {
            return this.gridIndex;
        }

        public bool DoesEqual(GridNode obj)
        {
            return obj.gridIndex == this.gridIndex;
        }

        public override string ToString()
        {
            return "(" + gridX.ToString() + ", " + gridY.ToString() + ")";
        }
    }
}
