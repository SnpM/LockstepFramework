//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class GridManager
	{
		public const int NodeCount = 128;
		public const int ScanNodeCount = NodeCount / ScanResolution;
		public static GridNode[] Grid;
		private static ScanNode[] ScanGrid;
		public const int ScanResolution = 4;
		public const int SqrScanResolution = ScanResolution * ScanResolution;
		public const long OffsetX = FixedMath.One * -64;
		public const long OffsetY = FixedMath.One * -64;

		public static void Generate ()
		{
			ScanGrid = new ScanNode[ScanNodeCount * ScanNodeCount];
			for (int i = 0; i < NodeCount / ScanResolution; i++) {
				for (int j = 0; j < NodeCount / ScanResolution; j++) {
					ScanGrid [GetScanIndex (i, j)] = new ScanNode (i, j);
				}
			}
			Grid = new GridNode[NodeCount * NodeCount];
			for (int i = 0; i < NodeCount; i++) {
				for (int j = 0; j < NodeCount; j++) {
					Grid [i * NodeCount + j] = new GridNode (i, j);
				}
			}
		}
		
		public static void Initialize ()
		{
			for (int k = 0; k < NodeCount * NodeCount; k++) {
				Grid [k].Initialize ();
			}
		}

		public static GridNode GetNode (int xGrid, int yGrid)
		{
			//if (xGrid < 0 || xGrid >= NodeCount || yGrid < 0 || yGrid >= NodeCount) return null;

			return Grid [GetGridIndex (xGrid, yGrid)];
		}
		
		static int indexX;
		static int indexY;

		public static GridNode GetNode (long xPos, long yPos)
		{
			indexX = (int)((xPos + FixedMath.Half - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT);
			indexY = (int)((yPos + FixedMath.Half - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT);
			return (GetNode (indexX, indexY));
		}

		public static void GetCoordinates (long xPos, long yPos, out int xGrid, out int yGrid)
		{
			xGrid = (int)((xPos + FixedMath.Half - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT);
			yGrid = (int)((yPos + FixedMath.Half - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT);
		}

		public static void GetScanCoordinates (long xPos, long yPos, out int xGrid, out int yGrid)
		{
			//xGrid = (int)((((xPos + FixedMath.Half - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT) + ScanResolution / 2) / ScanResolution);
			//yGrid = (int)((((yPos + FixedMath.Half - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT) + ScanResolution / 2) / ScanResolution);
			ScanNode scanNode =  GetNode (xPos, yPos).LinkedScanNode;
			xGrid = scanNode.X;
			yGrid = scanNode.Y;
		}

		public static ScanNode GetScanNode (int xGrid, int yGrid)
		{
			//if (xGrid < 0 || xGrid >= NodeCount || yGrid < 0 || yGrid >= NodeCount) return null;

			return ScanGrid [GetScanIndex (xGrid, yGrid)];
		}

		public static int GetGridIndex (int xGrid, int yGrid)
		{
			if (xGrid < 0)
				xGrid = 0;
			if (xGrid >= NodeCount)
				xGrid = NodeCount - 1;
			if (yGrid < 0)
				yGrid = 0;
			if (yGrid >= NodeCount)
				yGrid = NodeCount - 1;
			return xGrid * NodeCount + yGrid;
		}

		public static int GetScanIndex (int xGrid, int yGrid)
		{
			if (xGrid < 0)
				xGrid = 0;
			if (xGrid >= ScanNodeCount)
				xGrid = ScanNodeCount - 1;
			if (yGrid < 0)
				yGrid = 0;
			if (yGrid >= ScanNodeCount)
				yGrid = ScanNodeCount - 1;
			return xGrid * ScanNodeCount + yGrid;
		}

		public static void ApplyWeight (int xpos, int ypos, int weight)
		{
			Grid [xpos * NodeCount + ypos].Weight = weight;
		}

		static int i, j;

		public static void ApplyWeight (int xmin, int xmax, int ymin, int ymax, int weight)
		{
			for (i = xmin; i <= xmax; i++) {
				for (j = ymin; j <= ymax; j++) {
					Grid [i * NodeCount + j].Weight = weight;
				}
			}
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