//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class GridManager
	{
        public const int DefaultCapacity = 64 * 64;

        public static int Width {get; private set;}
        public static int Height {get; private set;}
        public static int ScanHeight {get; private set;}
        public static int ScanWidth {get; private set;}
        public static int GridSize {get; private set;}
        public static int ScanGridSize {get; private set;}
		public static GridNode[] Grid;
		private static ScanNode[] ScanGrid;
		public const int ScanResolution = 4;
		public const int SqrScanResolution = ScanResolution * ScanResolution;
        public static long OffsetX {get; private set;}
        public static long OffsetY {get; private set;}
        private static bool _useDiagonalConnections = true;
        public static bool UseDiagonalConnections {
            get {
                return _useDiagonalConnections;
            }
            private set {
                _useDiagonalConnections = value;
            }
        }
		public static void NotifyGridChanged () {

		}
		public static bool GridChanged {get; private set;}
		public static uint GridVersion {get; private set;}

        private static bool _settingsChanged = true;

        public static readonly GridSettings DefaultSettings = new GridSettings();

        private static GridSettings _settings;
        /// <summary>
        /// GridSettings for the GridManager's simulation. Make sure you set this property ONLY if you wish to change the settings.
        /// Changes will apply to the next session.
        /// </summary>
        /// <value>The settings.</value>
        public static GridSettings Settings {
            get {
                return _settings;
            }
            set {
                _settings = value;
                _settingsChanged = true;
            }
        }

        private static FastStack<GridNode> CachedGridNodes = new FastStack<GridNode> (GridManager.DefaultCapacity);
        private static FastStack<ScanNode> CachedScanNodes = new FastStack<ScanNode> (GridManager.DefaultCapacity);
        static void ApplySettings () {
            Width = Settings.Width;
            Height = Settings.Height;
            ScanHeight = Height / ScanResolution;
            ScanWidth = Width / ScanResolution;
            GridSize = Width * Height;
            OffsetX = Settings.XOffset;
            OffsetY = Settings.YOffset;

            ScanGridSize = ScanHeight * ScanWidth;
            UseDiagonalConnections = Settings.UseDiagonalConnections;
        }
		private static void Generate ()
		{



        #region Pooling; no need to create all those nodes again

            if (Grid != null) {
                int min = Grid.Length;
                CachedGridNodes.EnsureCapacity (min);
                for (int i = min - 1; i >= 0; i--) {
					if (LockstepManager.PoolingEnabled)
						CachedGridNodes.Add (Grid[i]);
                }
            }


            if (ScanGrid != null) {
                int min = ScanGrid.Length;
                CachedScanNodes.EnsureCapacity(min);
                for (int i = min - 1; i >= 0; i--) {
					if (LockstepManager.PoolingEnabled)
						CachedScanNodes.Add(ScanGrid[i]);
                }
            }
        #endregion


			ScanGrid = new ScanNode[ScanGridSize];
            for (int i = ScanWidth - 1; i >= 0; i--) {
                for (int j = ScanHeight - 1; j >= 0; j--) {
                    ScanNode node = CachedScanNodes.Count > 0 ? CachedScanNodes.Pop() : new ScanNode ();
                    node.Setup(i,j);
                    ScanGrid [GetScanIndex (i, j)] = node;
				}
			}
			Grid = new GridNode[GridSize];
            for (int i = Width - 1; i >= 0; i--) {
				for (int j = Height - 1; j >= 0; j--) {
                    GridNode node = CachedGridNodes.Count > 0 ? CachedGridNodes.Pop() : new GridNode ();
                    node.Setup(i,j);
                    Grid [GetGridIndex (i,j)] = node;
				}
			}
		}
		

        public static int GenerateDeltaCount(int size)
        {
            long fixSize = FixedMath.Create(size);
            int ret = FixedMath.Mul(FixedMath.Mul(fixSize, fixSize), FixedMath.Pi).CeilToInt();
            return ret;
        }


        public static void Setup () {
            //Nothing here to see
        }

		public static void Initialize ()
		{
			GridVersion = 1;
			GridChanged = false;
			if (!LockstepManager.PoolingEnabled)
				_settingsChanged = true;
            if (_settingsChanged) {
                if (_settings == null)
                    _settings = DefaultSettings;
                ApplySettings ();

                Generate ();

                for (int i = GridSize - 1; i >= 0; i--) {
    				Grid [i].Initialize ();
    			}
            }
            else {
                //If we're using the same settings, no need to generate a new grid or neighbors
                for (int i = GridSize - 1; i >= 0; i--) {
                    Grid[i].FastInitialize();
                }
            }
		}

		public static void LateSimulate () {
			if (GridChanged)
				GridVersion++;
		}

		public static GridNode GetNode (int xGrid, int yGrid)
		{
            if (xGrid < 0 || xGrid >= Grid.Length || yGrid < 0 || yGrid >= Grid.Length) 
				return null;
			return Grid [GetGridIndex (xGrid, yGrid)];
		}
		
		static int indexX;
		static int indexY;

		public static GridNode GetNode (long xPos, long yPos)
		{
			GetCoordinates (xPos, yPos, out indexX, out indexY);
			if (!ValidateCoordinates (indexX, indexY))
				return null;
			return (GetNode (indexX, indexY));
		}

		public static bool ValidateCoordinates (int xGrid, int yGrid)
		{
			return xGrid >= 0 && xGrid < Width && yGrid >= 0 && yGrid < Height;
		}

		public static bool ValidateIndex (int index)
		{
			return index >= 0 && index < GridSize;
		}

		public static void GetCoordinates (long xPos, long yPos, out int xGrid, out int yGrid)
		{
			xGrid = (int)((xPos + FixedMath.Half - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT);
			yGrid = (int)((yPos + FixedMath.Half - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT);
		}

		public static bool GetScanCoordinates (long xPos, long yPos, out int xGrid, out int yGrid)
		{
			//xGrid = (int)((((xPos + FixedMath.Half - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT) + ScanResolution / 2) / ScanResolution);
			//yGrid = (int)((((yPos + FixedMath.Half - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT) + ScanResolution / 2) / ScanResolution);

			GridNode gridNode = GetNode (xPos, yPos);
			if (gridNode.IsNull())
			{
				xGrid = 0;
				yGrid = 0;
				return false;
			}

			ScanNode scanNode =  gridNode.LinkedScanNode;
			xGrid = scanNode.X;
			yGrid = scanNode.Y;

			return true;
		}

		public static ScanNode GetScanNode (int xGrid, int yGrid)
		{
			//if (xGrid < 0 || xGrid >= NodeCount || yGrid < 0 || yGrid >= NodeCount) return null;
			if (!ValidateScanCoordinates (xGrid, yGrid))
				return null;
			return ScanGrid [GetScanIndex (xGrid, yGrid)];
		}

		public static int GetGridIndex (int xGrid, int yGrid)
		{
			return xGrid * Height + yGrid;
		}

		public static bool ValidateScanCoordinates (int scanX, int scanY)
		{
			return scanX >= 0 && scanX < ScanWidth && scanY >= 0 && scanY < ScanHeight;
		}

		public static int GetScanIndex (int xGrid, int yGrid)
		{
			return xGrid * ScanHeight + yGrid;
		}

		public static int ToGridX (this long xPos)
		{
			return (xPos - OffsetX).RoundToInt ();
		}

		public static int ToGridY (this long yPos)
		{
			return (yPos - OffsetY).RoundToInt ();
		}
	}
}