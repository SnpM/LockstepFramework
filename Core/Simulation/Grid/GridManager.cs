using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Lockstep
{
	public static class GridManager
	{
		public const int NodeCount = 64;
		public static GridNode[] Grid;
		public static LayerMask UnwalkableMask;
		public const long OffsetX = FixedMath.One * -32;
		public const long OffsetY = FixedMath.One * -32;

		public static void Generate ()
		{
			Grid = new GridNode[NodeCount * NodeCount];
			for (int i = 0; i < NodeCount; i++)
			{
				for (int j = 0; j < NodeCount; j++)
				{
					Grid[i * NodeCount + j] = new GridNode(i,j);
				}
			}
		}
		
		public static void Initialize ()
		{
			for (int k = 0; k < NodeCount * NodeCount; k++)
			{
				Grid[k].Initialize ();
			}
		}


		public static GridNode GetNode (int xGrid, int yGrid)
		{
			return Grid [GetGridIndex(xGrid,yGrid)];
		}

		
		static int indexX;
		static int indexY;
		public static GridNode GetNode (long xPos, long yPos)
		{
			indexX = (int)((xPos + FixedMath.Half - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT);
			indexY = (int)((yPos + FixedMath.Half - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT);
			return (Grid[GetGridIndex (indexX, indexY)]);
		}

		public static int GetGridIndex (int xGrid, int yGrid)
		{
			if (xGrid < 0) xGrid = 0;
			else if (xGrid >= NodeCount) xGrid = NodeCount - 1;
			if (yGrid < 0) yGrid = 0;
			else if (yGrid >= NodeCount) yGrid = NodeCount - 1;
			return xGrid * NodeCount + yGrid;
		}
			
		public static void ApplyWeight (int xpos, int ypos, int weight)
		{
			Grid[xpos * NodeCount + ypos].Weight = weight;
		}

		static int i, j;
		public static void ApplyWeight (int xmin, int xmax, int ymin, int ymax, int weight)
		{
			for ( i = xmin; i <= xmax; i++)
			{
				for ( j = ymin; j <= ymax; j++)
				{
					Grid[i * NodeCount + j].Weight = weight;
				}
			}
		}
	}
}