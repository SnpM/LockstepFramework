//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections; using FastCollections;
using System;
using Lockstep.Pathfinding;
namespace Lockstep
{
	public class GridNode
	{
		#region Constructor

		static GridNode ()
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

		public GridNode ()
		{

		}

		public void Setup (int _x, int _y)
		{
			gridX = _x;
			gridY = _y;
			gridIndex = GridManager.GetGridIndex (gridX, gridY);
			WorldPos.x = gridX * FixedMath.One + GridManager.OffsetX;
			WorldPos.y = gridY * FixedMath.One + GridManager.OffsetY;

		}

		public void Initialize ()
		{

			GenerateNeighbors ();
			LinkedScanNode = GridManager.GetScanNode (gridX / GridManager.ScanResolution, gridY / GridManager.ScanResolution);
			this.FastInitialize ();
		}

		public void FastInitialize ()
		{
			this.ClosedSetVersion = 0;
			this.HeapIndex = 0;
			this.HeapVersion = 0;
			this.GridVersion = 0;
		}

		#endregion

		#region

		static int _i;

		#endregion

		#region Collection Helpers

		public uint ClosedSetVersion;
		public uint HeapVersion;
		public uint HeapIndex;

		public uint GridVersion;

		#endregion


		#region Pathfinding

		public int gridX;
		public int gridY;
		public int gridIndex;

		public int gCost;
		public int hCost;
		public int fCost;
		public GridNode parent;
		private byte _obstacleCount;

		public byte ObstacleCount {
			get {

				return _obstacleCount;
			}
		}


		public bool Unwalkable {
			get {
				return _obstacleCount > 0;
			}
		}

		private bool _clearance;

		public bool Clearance {
			get {
				CheckUpdateValues ();
				return _clearance;
			}
		}

		bool _extraClearanceObsolete;
		private bool _extraClearnace;

		public bool ExtraClearance {
			get {
				CheckUpdateValues();
				if (_extraClearanceObsolete) {
					UpdateExtraClearance ();
				}
				return _extraClearnace;
			}
		}

		void CheckUpdateValues ()
		{
			if (GridVersion != GridManager.GridVersion) {
				UpdateValues ();
			}
		}

		void UpdateExtraClearance ()
		{
			_extraClearanceObsolete = false;

			if (Unwalkable) {
				_extraClearnace = false;
			} else {
				_extraClearnace = true;
				for (int i = 7; i >= 0; i--) {
					var neighbor = NeighborNodes [i];
					if (neighbor.IsNull () || !neighbor.Clearance) {
						_extraClearnace = false;
					}
				}
			}
		}

		void UpdateClearance ()
		{
			if (Unwalkable) {
				_clearance = false;
			} else {
				_clearance = true;
				//backward looping is faster

				for (int i = 7; i >= 0; i--) {
					var neighbor = NeighborNodes [i];
					if (neighbor.IsNull () || neighbor.Unwalkable) {
						_clearance = false;
					}
				}
			}
		}

		void UpdateValues ()
		{
			GridVersion = GridManager.GridVersion;

			//fast enough to just do it
			UpdateClearance();

			_extraClearanceObsolete = true;
		}

		static int CachedUnpassableCheckSize;

		internal static void PrepareUnpassableCheck (int size)
		{
			CachedUnpassableCheckSize = size;
		}

		internal bool Unpassable ()
		{
			if (CachedUnpassableCheckSize > Pathfinder.SmallSize) {
				if (CachedUnpassableCheckSize > Pathfinder.MediumSize) {
					return !ExtraClearance;
				}
				return !Clearance;
			}
			return Unwalkable;

		}

		public void AddObstacle ()
		{
			#if DEBUG
			if (this._obstacleCount == byte.MaxValue) {
				Debug.LogErrorFormat ("Too many obstacles on this node ({0})!", new Coordinate (this.gridX, this.gridY));
			}
			#endif
			this._obstacleCount++;
			GridManager.NotifyGridChanged ();
		}

		public void RemoveObstacle ()
		{
			if (this._obstacleCount == 0) {
				Debug.LogErrorFormat ("No obstacle to remove on this node ({0})!", new Coordinate (this.gridX, this.gridY));
			}
			this._obstacleCount--;
			GridManager.NotifyGridChanged ();
		}


		GridNode _node;



		public GridNode[] NeighborNodes = new GridNode[8];
		public Vector2d WorldPos;

		private void GenerateNeighbors ()
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

			for (i = -1; i <= 1; i++) {
				checkX = gridX + i;

				for (j = -1; j <= 1; j++) {
					if (i == 0 && j == 0) //Don't do anything for the same node
                        continue;
					checkY = gridY + j;
					if (GridManager.ValidateCoordinates (checkX, checkY)) {
						int neighborIndex;
						if ((i != 0 && j != 0)) {
							//Diagonal
							if (GridManager.UseDiagonalConnections) {
								neighborIndex = diagonalIndex++;
							} else
								continue;
						} else {
							neighborIndex = sideIndex++;
						}
						GridNode checkNode = GridManager.Grid [GridManager.GetGridIndex (checkX, checkY)];
						NeighborNodes [neighborIndex] = checkNode;
					}
				}
			}
			
			
		}

		static int dstX;
		static int dstY;
		public static int HeuristicTargetX;
		public static int HeuristicTargetY;

		public void CalculateHeuristic ()
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

		public void Add (LSInfluencer influencer)
		{
			LinkedScanNode.Add (influencer);
		}

		public void Remove (LSInfluencer influencer)
		{
			LinkedScanNode.Remove (influencer);
		}

		#endregion

		static int i, j, checkX, checkY, leIndex;

		public override int GetHashCode ()
		{
			return this.gridIndex;
		}

		public bool DoesEqual (GridNode obj)
		{
			return obj.gridIndex == this.gridIndex;
		}

		public override string ToString ()
		{
			return "(" + gridX.ToString () + ", " + gridY.ToString () + ")";
		}
	}
}
