using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
	public class BoundsBlocker : EnvironmentObject
	{
		[SerializeField,FixedNumber]
		private long _mapWidth;
		[SerializeField,FixedNumber]
		private long _mapHeight;

		protected override void OnLateInitialize ()
		{
			long gridWidth = FixedMath.Create (GridManager.Width);
			long gridHeight = FixedMath.Create(GridManager.Height);
			long widthEdgeOffset = (gridWidth - _mapWidth) / 2;
			long heightEdgeOffset = (gridHeight - _mapHeight) / 2;
			Area gridArea = new Area(
				GridManager.OffsetX,
				GridManager.OffsetY,
				GridManager.OffsetX + gridWidth,
				GridManager.OffsetY + gridHeight
			);
			Area noBlockArea = new Area(
				gridArea.XCorner1 + widthEdgeOffset,
				gridArea.YCorner1 + heightEdgeOffset,
				gridArea.XCorner2 - widthEdgeOffset,
				gridArea.YCorner2 - heightEdgeOffset
			);

			//block top
			ManualBlocker.BlockArea (new Area(
				gridArea.XMin,
				noBlockArea.YMax,
				gridArea.XMax,
				gridArea.YMax
			));

			//block bottom
			ManualBlocker.BlockArea(new Area(
				gridArea.XMin,
				gridArea.YMin,
				gridArea.XMax,
				noBlockArea.YMin
			));

			//block right
			ManualBlocker.BlockArea(new Area(
				noBlockArea.XMax,
				noBlockArea.YMin,
				gridArea.XMax,
				noBlockArea.YMax
			));
			//block left
			ManualBlocker.BlockArea(new Area(
				gridArea.XMin,
				noBlockArea.YMin,
				noBlockArea.XMin,
				noBlockArea.XMax
			));

		}

		void BlockNode (long x, long y) {
			var node = GridManager.GetNode (x, y);
			if (node.IsNotNull())
				node.AddObstacle ();
		}
	}
}