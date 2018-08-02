using FastCollections;
using System.Collections.Generic;
using PanLineAlgorithm;
using System;

namespace Lockstep
{

	public static class Raycaster
	{
		internal static uint _Version { get; set; }

		public static readonly FastList<Vector2d> bufferIntersectionPoints = new FastList<Vector2d>();

		/// <summary>
		/// Conditional for raycasting. Reset after every raycast. Access Raycaster.CurBody for the current body being checked.
		/// </summary>
		/// <value>The conditional.</value>
		public static Func<bool> Conditional { get; set; }


		public static IEnumerable<LSBody> RaycastAll(Vector2d start, Vector2d end)
		{
			_Version++;
			LSBody.PrepareAxisCheck(start, end);
			foreach (FractionalLineAlgorithm.Coordinate coor in
				GetRelevantNodeCoordinates(start, end))
			{
				int indexX = coor.X;
				int indexY = coor.Y;
				if (!Partition.CheckValid(coor.X, coor.Y))
				{
					break;
				}
				PartitionNode node = Partition.GetNode(indexX, indexY);
				for (int i = node.ContainedDynamicObjects.Count - 1; i >= 0; i--)
				{
					LSBody body = PhysicsManager.SimObjects[node.ContainedDynamicObjects[i]];
					if (body.IsNotNull() && body.RaycastVersion != _Version)
					{
						if (Conditional.IsNull() || Conditional())
						{
							body.RaycastVersion = _Version;
							if (body.Overlaps(bufferIntersectionPoints))
							{
								yield return body;
							}
						}
					}
				}
			}
			Conditional = null;
			yield break;
		}


		public static LSBody CurBody { get; private set; }

		public static IEnumerable<LSBody> RaycastAll(Vector3d start3d, Vector3d end3d)
		{
			long startHeight = start3d.z;
			long dist2d = (end3d.ToVector2d() - start3d.ToVector2d()).Magnitude();

			if (dist2d != 0)
			{
				long heightSlope = (end3d.z - start3d.z).Div(dist2d);
				Vector2d start = start3d.ToVector2d();
				Vector2d end = end3d.ToVector2d();
				if (heightSlope == 0)
				{

				}
				else
				{
					//TODO: return bodies based on hit order
					foreach (LSBody body in RaycastAll(start, end))
					{
						bool heightIntersects = false;
						bool mined = false;
						bool maxed = false;
						for (int i = bufferIntersectionPoints.Count - 1; i >= 0; i--)
						{
							long dist = bufferIntersectionPoints[i].Distance(start);
							long heightAtBodyPosition = startHeight + (dist.Mul(heightSlope));

							//TODO: Make this more accurate
							if (heightAtBodyPosition < body.HeightMin)
							{
								mined = true;
							}
							else if (heightAtBodyPosition > body.HeightMax)
							{
								maxed = true;
							}
							else
							{
								heightIntersects = true;
								break;
							}
							if (mined && maxed)
							{
								heightIntersects = true;
								break;
							}
						}
						if (heightIntersects)
							yield return body;

					}
				}
			}
		}

		public static IEnumerable<FractionalLineAlgorithm.Coordinate> GetRelevantNodeCoordinates(Vector2d start, Vector2d end)
		{
			//Note: 99% sure this is deterministic enough for use in simulation.
			foreach (FractionalLineAlgorithm.Coordinate coor in FractionalLineAlgorithm.Trace(
				Partition.GetRelativeX(start.x).ToDouble(),
				Partition.GetRelativeX(start.y).ToDouble(),
				Partition.GetRelativeY(end.x).ToDouble(),
				Partition.GetRelativeY(end.y).ToDouble()))
			{
				//int indexX = coor.X;
				//int indexY = coor.Y;
				yield return coor;
			}
		}
	}
}