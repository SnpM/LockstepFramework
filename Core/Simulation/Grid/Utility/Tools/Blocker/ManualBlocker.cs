using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

namespace Lockstep
{
	public class ManualBlocker : EnvironmentObject
	{
		[SerializeField]
		private Area[] _blockAreas;

		Area[] BlockAreas { get { return _blockAreas; } }


		protected override void OnLateInitialize ()
		{
			for (int i = 0; i < BlockAreas.Length; i++) {
				var block = BlockAreas [i];
				BlockArea(block);
			}
		}
		public static void BlockArea (Area block) {
			long xMin = block.XMin;
			long xMax = block.XMax;
			long yMin = block.YMin;
			long yMax = block.YMax;

			for (long x = xMin; x <= xMax; x += FixedMath.One) {
				for (long y = yMin; y <= yMax; y += FixedMath.One) {
					var node = GridManager.GetNode (x, y);
					if (node.IsNotNull())
						node.AddObstacle ();
				}
			}
		}
	}
}