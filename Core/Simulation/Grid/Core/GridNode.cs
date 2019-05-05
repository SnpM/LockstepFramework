//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;

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
			if (_x < 0 || _y < 0)
			{
				Debug.LogError("Cannot be negative!");
			}
			gridX = _x;
			gridY = _y;
			gridIndex = GridManager.GetGridIndex(gridX, gridY);
			WorldPos.x = gridX * FixedMath.One + GridManager.OffsetX;
			WorldPos.y = gridY * FixedMath.One + GridManager.OffsetY;

		}

		public void Initialize()
		{

			GenerateNeighbors();
			LinkedScanNode = GridManager.GetScanNode(gridX / GridManager.ScanResolution, gridY / GridManager.ScanResolution);
			_clearanceDegree = DEFAULT_DEGREE;
			_clearanceSource = DEFAULT_SOURCE;
			this.FastInitialize();
		}

		public void FastInitialize()
		{
			this.HeapIndex = 0;
			this.HeapVersion = 0;
			this.ClosedHeapVersion = 0;
			this.GridVersion = 0;
			this.CombinePathVersion = 0;
			this._obstacleCount = 0;
		}

		#endregion

		#region

		static int _i;

		#endregion

		#region Collection Helpers

		public uint HeapVersion;
		public uint ClosedHeapVersion;
		public uint HeapIndex;

		/// <summary>
		/// TODO: Maybe it will be more efficient for memory to not cache this?
		/// </summary>
		public uint GridVersion;

		#endregion


		#region Pathfinding

		public int gridX;
		public int gridY;
		public uint gridIndex;

		public int gCost;
		public int hCost;
		public int fCost;
		public GridNode parent;
		public GridNode combineTrailNode;
		private byte _obstacleCount;

		public byte ObstacleCount
		{
			get
			{

				return _obstacleCount;
			}
		}


		public bool Unwalkable
		{
			get
			{
				return _obstacleCount > 0;
			}
		}


		private byte _clearanceSource;
		internal byte ClearanceSource { get { return _clearanceSource; } }
		private byte _clearanceDegree;
		/// <summary>
		/// How many connections until the closest unwalkable node.
		/// If a big unit stands directly on this node, it won't be able to fit if the degree is too low.
		/// </summary>

		public byte ClearanceDegree { get { return _clearanceDegree; } }
		public byte GetClearanceDegree()
		{
			CheckUpdateValues();
			return _clearanceDegree;
		}

		void CheckUpdateValues()
		{
			if (GridVersion != GridManager.GridVersion)
			{
				UpdateValues();
			}
		}
		public const byte DEFAULT_DEGREE = byte.MaxValue;
		public const byte DEFAULT_SOURCE = byte.MaxValue;
		void UpdateClearance()
		{
			if (Unwalkable)
			{
				_clearanceDegree = 0;
				_clearanceSource = DEFAULT_SOURCE;
			}
			else
			{
				if (_clearanceSource <= 7)
				{

					//refresh source in case the map changed
					var source = NeighborNodes[_clearanceSource];
                    if (source.IsNull() == false)
                    {
                        var prevSourceDegree = source.ClearanceDegree;
                        if (source.ClearanceDegree < _clearanceDegree)
                        {
                            source.UpdateValues();
                            //Clearance from source can no longer be trusted!
                            if (source.ClearanceDegree != prevSourceDegree)
                            {
                                _clearanceDegree = DEFAULT_DEGREE;
                                _clearanceSource = DEFAULT_SOURCE;
                            }
                        }
                        else
                        {
                            _clearanceDegree = (byte)(source.ClearanceDegree + 1);
                        }
                    }
                }
                //This method isn't always 100% accurate but after several updates, it will have a better picture of the map
                //Clarification: _clearanceSource is the source of a blockage. It's cached so that when the map is changed, the source of the major block can be rechecked for changes.
                //TODO: Test this thoroughly and visualize
                for (int i = 7; i >= 0; i--)
                {
                    var neighbor = (NeighborNodes[i]);
                    if (neighbor.IsNull() || neighbor.Unwalkable)
                    {
                        _clearanceDegree = 1;
                        _clearanceSource = (byte)i;
                        break;
                    }
                    //Cap clearance to 8. Something larger than that won't work very well with pathfinding.
                    if (neighbor._clearanceDegree < ClearanceDegree && neighbor._clearanceDegree < 8)
                    {
                        _clearanceDegree = (byte)(neighbor._clearanceDegree + 1);
                        _clearanceSource = (byte)i;
                    }
                }
            }
		}


        /// <summary>
        /// Returns true if clearance degree changed.
        /// </summary>
        /// <returns></returns>
		void UpdateValues()
		{
			GridVersion = GridManager.GridVersion;

			//fast enough to just do it
			UpdateClearance();
		}

		#region CombinePath
		//This is the system used for groups of pathfinding queries to the same destination
		//If query 2 finds its way onto a node on the first query, it will use the rest of the first query
		public ulong CombinePathVersion;
		#endregion
		static int CachedUnpassableCheckSize;

		internal static void PrepareUnpassableCheck(int size)
		{
			CachedUnpassableCheckSize = size;
		}

		/// <summary>
		/// If this unit is too fat to fit.
		/// </summary>
		internal bool Unpassable()
		{
			if (true)
			{//CachedUnpassableCheckSize) {
			 //If there's an unwalkable within the size's number of connections, the unit cannot pass
				return GetClearanceDegree() < CachedUnpassableCheckSize;
			}
			return Unwalkable;

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
			GridManager.NotifyGridChanged();
		}

		public void RemoveObstacle()
		{
			if (this._obstacleCount == 0)
			{
				Debug.LogErrorFormat("No obstacle to remove on this node ({0})!", new Coordinate(this.gridX, this.gridY));
			}
			this._obstacleCount--;
			GridManager.NotifyGridChanged();
		}


		GridNode _node;



		public GridNode[] NeighborNodes = new GridNode[8];
		public Vector2d WorldPos;

		private void GenerateNeighbors()
		{
			//0-3 = sides, 4-7 = diagonals
			//0 = (-1, 0)
			//1 = (0,-1)
			//2 = (0,1)
			//3 = (1,0)
			//4 = (-1,-1)
			//5 = (-1,1)
			//6 = (1,-1)
			//7 = (1,1)
			int sideIndex = 0;
			int diagonalIndex = 4; //I learned how to spell [s]diagnal[/s] diagonal!!!

			for (i = -1; i <= 1; i++)
			{
				checkX = gridX + i;

				for (j = -1; j <= 1; j++)
				{
					if (i == 0 && j == 0) //Don't do anything for the same node
						continue;
					checkY = gridY + j;
					if (GridManager.ValidateCoordinates(checkX, checkY))
					{
						int neighborIndex;
						if ((i != 0 && j != 0))
						{
							//Diagonal
							if (GridManager.UseDiagonalConnections)
							{
								neighborIndex = diagonalIndex++;
							}
							else
								continue;
						}
						else
						{
							neighborIndex = sideIndex++;
						}
						GridNode checkNode = GridManager.Grid[GridManager.GetGridIndex(checkX, checkY)];
						NeighborNodes[neighborIndex] = checkNode;
					}
				}
			}


		}

		static int dstX;
		static int dstY;
		public static int HeuristicTargetX;
		public static int HeuristicTargetY;

		public void CalculateHeuristic()
		{


#if true
			//manhattan
			if (gridX > HeuristicTargetX)
				dstX = gridX - HeuristicTargetX;
			else
				dstX = HeuristicTargetX - gridX;
			if (gridY > HeuristicTargetY)
				dstY = gridY - HeuristicTargetY;
			else
				dstY = HeuristicTargetY - gridY;

			hCost = (dstX + dstY) * 100;
#elif true
			//octile
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

#elif false
			//euclidean
			dstX = HeuristicTargetX - gridX;
			dstY = HeuristicTargetY - gridY;
			double d = dstX * dstX + dstY * dstY;
			d = Math.Sqrt(d);
			hCost = (int)(d * 100);
#endif

			fCost = gCost + hCost;

		}

		#endregion


		#region Influence

		public int ScanX { get { return LinkedScanNode.X; } }

		public int ScanY { get { return LinkedScanNode.Y; } }

		public ScanNode LinkedScanNode;

		public void Add(LSInfluencer influencer)
		{
			LinkedScanNode.Add(influencer);
		}

		public void Remove(LSInfluencer influencer)
		{
			LinkedScanNode.Remove(influencer);
		}

		#endregion

        //TODO: Get rid of static microoptimization
		static int i, j, checkX, checkY, leIndex;

		public override int GetHashCode()
		{
			return (int)this.gridIndex;
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
