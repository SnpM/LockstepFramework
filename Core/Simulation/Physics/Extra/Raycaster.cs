using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class Raycaster
	{
		public static uint _Version = 1;
		public LSBody Hit;
		public FastList<LSBody> Hits = new FastList<LSBody> ();
		public FastList<Vector2d> HitPoints = new FastList<Vector2d> ();

		#region Method Variables
		static int i, j;
		static long x0, y0, x1, y1;
		static long dx, dy, error, ystep, x, y, t;
		static long compare1, compare2;
		static long retX, retY;
		static bool steep;
		static long SwapValue;

		static bool DidHit;
		static bool MadeContact;
		static long Projection, TestMin, TestMax;
		static long Mag, AxisX, AxisY, AxisMin, AxisMax, PerpProj;
		static long XMin, XMax, YMin, YMax;
		#endregion


		public bool RaycastAll (Vector2d From, Vector2d To, int ExceptionID)
		{
			return InternalRaycast (From,To,ExceptionID);
		}
		private bool InternalRaycast (Vector2d From, Vector2d To, int ExceptionID)
		{
			_Version++;

			MadeContact = false;
			Hits.FastClear ();

			
			const int StepSize = 1 << Partition.ShiftSize;
			x0 = From.x;
			y0 = From.y;
			x1 = To.x;
			y1 = To.y;
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
			ystep = (y0 < y1) ? StepSize : -StepSize;
			y = y0;
			
			AxisX = From.x - To.x;
			AxisY = From.y - To.y;
			Mag = FixedMath.Sqrt ((AxisX * AxisX + AxisY * AxisY) >> FixedMath.SHIFT_AMOUNT);
			if (Mag == 0)
				return false;
			AxisX = FixedMath.Div (AxisX, Mag);
			AxisY = FixedMath.Div (AxisY, Mag);
			AxisMin = Vector2d.Dot (AxisX, AxisY, From.x, From.y);
			AxisMax = Vector2d.Dot (AxisX, AxisY, To.x, To.y);
			if (AxisMin > AxisMax)
			{
				SwapValue = AxisMin;
				AxisMin = AxisMax;
				AxisMax = SwapValue;
			}
			PerpProj = Vector2d.Dot (-AxisY, AxisX, From.x, From.y);

			XMin = From.x;
			XMax = To.x;
			if (XMin > XMax)
			{
				SwapValue = XMin;
				XMin = XMax;
				XMax = SwapValue;
			}
			YMin = From.y;
			YMax = To.y;
			if (YMin > YMax)
			{
				SwapValue = YMin;
				YMin = YMax;
				YMax = SwapValue;
			}
			x = x0;
			while (true) {
				if (steep)
				{
					retX = (y - Partition.OffsetX) / StepSize;
					retY = (x - Partition.OffsetY) / StepSize;
				}
				else {
					retX = (x - Partition.OffsetX) / StepSize;
					retY = (y - Partition.OffsetY) / StepSize;
				}
				
				PartitionNode node = Partition.Nodes [retX * Partition.Count + retY];
				if (node.Count > 0) {
					
					for (i = 0; i < node.Count; i++) {

						DidHit = false;

						LSBody body = PhysicsManager.SimObjects [node [i]];
						if (body.RaycastVersion != _Version && body.ID != ExceptionID) {
							body.RaycastVersion = _Version;
							switch (body.Shape) {
							case ColliderType.Circle:
								Projection = Vector2d.Dot (AxisX, AxisY, body.Position.x, body.Position.y);
								TestMin = Projection - body.Radius;
								TestMax = Projection + body.Radius;
								if (TestMin < AxisMax) {
									if (TestMax > AxisMin) {
										Projection = Vector2d.Dot (-AxisY, AxisX, body.Position.x, body.Position.y);
										TestMin = Projection - body.Radius;
										TestMax = Projection + body.Radius;
										if (PerpProj < TestMax && PerpProj > TestMin) {
											DidHit = true;
										}
									}
								}
								break;
								
							case ColliderType.AABox:
								if (AxisMin < body.XMax) {
									if (AxisMax > body.XMin) {
										if (PerpProj < body.YMax) {
											if (PerpProj > body.YMin) {

												DidHit = true;
											}
										}
									}
								}
								break;
							}
							if (DidHit) {
								Hits.Add (body);
								MadeContact = true;
								break;
							}
						}
					}

				}

				error = error - dy;
				if (error < 0) {
					y += ystep;
					error += dx;
				}

				if (x >= x1)
				{
					break;
				}
				x += StepSize;

			}
			return MadeContact;
		}




	}
}