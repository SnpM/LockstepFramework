using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class CollisionPair
	{
		public LSBody Body1;
		public LSBody Body2;
		private long CacheSqrDistance;
		private CollisionType LeCollisionType;
		public bool IsColliding;
		private bool DoPhysics = true;
		public bool Active;
		public uint PartitionVersion;
		private PhysicsFavor physicsFavor;
		public static long DistX;
		public static long DistY;
		public static long PenetrationX;
		public static long PenetrationY;
		public static CollisionPair CurrentCollisionPair;

		public void Initialize (LSBody b1, LSBody b2)
		{
			PartitionVersion = 0;
			Body1 = b1;
			Body2 = b2;

			IsColliding = false;
			DistX = 0;
			DistY = 0;
			PenetrationX = 0;
			PenetrationY = 0;

			CacheSqrDistance = b1.Radius + b2.Radius;
			CacheSqrDistance *= CacheSqrDistance;

			LeCollisionType = CollisionType.None;
			if (Body1.Shape == ColliderType.None || Body2.Shape == ColliderType.None) {
			} else if (Body1.Shape == ColliderType.Circle) {
				if (Body2.Shape == ColliderType.Circle) {
					LeCollisionType = CollisionType.Circle_Circle;
				} else if (Body2.Shape == ColliderType.AABox) {
					LeCollisionType = CollisionType.Circle_AABox;

				} else if (Body2.Shape == ColliderType.Polygon) {
					LeCollisionType = CollisionType.Circle_Polygon;
				}
			} else if (Body1.Shape == ColliderType.AABox) {
				if (Body2.Shape == ColliderType.Circle) {
					LeCollisionType = CollisionType.Circle_AABox;
				} else if (Body2.Shape == ColliderType.AABox) {
					LeCollisionType = CollisionType.AABox_AABox;
				} else if (Body2.Shape == ColliderType.Polygon) {
					LeCollisionType = CollisionType.AABox_Polygon;
				}
			} else if (Body1.Shape == ColliderType.Polygon) {
				if (Body2.Shape == ColliderType.Circle) {
					LeCollisionType = CollisionType.Circle_Polygon;
				} else if (Body2.Shape == ColliderType.AABox) {
					LeCollisionType = CollisionType.AABox_Polygon;
				} else if (Body2.Shape == ColliderType.Polygon) {
					LeCollisionType = CollisionType.Polygon_Polygon;
				}
			}

			if (DoPhysics) {
				if (Body1.Immovable) {
					physicsFavor = PhysicsFavor.Favor1;
				} else if (Body2.Immovable) {
					physicsFavor = PhysicsFavor.Favor2;
				} else if (Body1.Priority > Body2.Priority) {
					physicsFavor = PhysicsFavor.Favor1;
				} else if (Body2.Priority > Body1.Priority) {
					physicsFavor = PhysicsFavor.Favor2;
				} else {
					physicsFavor = PhysicsFavor.Same;
				}
			}
			Active = true;
		}

		public void Deactivate ()
		{
			Active = false;
		}

		static long dist, depth;

		private void DistributeCollision ()
		{
			if (Body1.OnContact != null)
				Body1.OnContact (Body2);
			if (Body2.OnContact != null)
				Body2.OnContact (Body1);

			if (DoPhysics) {
				switch (LeCollisionType) {
				case CollisionType.Circle_Circle:
					DistX = Body1.Position.x - Body2.Position.x;
					DistY = Body1.Position.y - Body2.Position.y;
					dist = FixedMath.Sqrt ((DistX * DistX + DistY * DistY) >> FixedMath.SHIFT_AMOUNT);
						
					if (dist == 0) {
						Body1.Position.x += (LSUtility.GetRandom (1 << (FixedMath.SHIFT_AMOUNT - 2)) + 1);
						Body1.Position.y -= (LSUtility.GetRandom (1 << (FixedMath.SHIFT_AMOUNT - 2)) + 1);
						return;
					}
						
					depth = (Body1.Radius + Body2.Radius - dist);

					if (depth < 0)
						return;

					DistX = (DistX * depth / dist);
					DistY = (DistY * depth / dist);

					//Resolving collision
					if (physicsFavor == PhysicsFavor.Favor1) {
						Body2.Position.x -= DistX;
						Body2.Position.y -= DistY;
						Body2.PositionChanged = true;
					} else if (physicsFavor == PhysicsFavor.Favor2) {
						Body1.Position.x += DistX;
						Body1.Position.y += DistY;
						Body1.PositionChanged = true;
					} else {
						DistX /= 4;
						DistY /= 4;
						if (Body1.Velocity.Dot (Body2.Velocity.x, Body2.Velocity.y) <= 0)
						{
							Body1.Velocity.x += DistX;//FixedMath.Mul(DistX, Body1.VelocityMagnitude);
							Body1.Velocity.y += DistY;//FixedMath.Mul(DistY, Body1.VelocityMagnitude);
							Body1.VelocityChanged = true;

							Body2.Velocity.x -= DistX;//FixedMath.Mul(DistX, Body2.VelocityMagnitude);
							Body2.Velocity.y -= DistY;//FixedMath.Mul(DistY, Body2.VelocityMagnitude);
							Body2.VelocityChanged = true;

							Body1.Position.x += DistX;
							Body1.Position.y += DistY;
							Body2.Position.x -= DistX;
							Body2.Position.y -= DistY;
						} else {
							Body1.Position.x += DistX;
							Body1.Position.y += DistY;
							Body2.Position.x -= DistX;
							Body2.Position.y -= DistY;
						}
						Body1.PositionChanged = true;
						Body2.PositionChanged = true;
					}
					break;
				case CollisionType.Circle_AABox:
					if (Body1.Shape == ColliderType.AABox) {
						DistributeCircle_Box (Body1, Body2);
					} else {
						DistributeCircle_Box (Body2, Body1);
					}
					break;
							
				case CollisionType.Circle_Polygon:

					break;
				}
			}

		}

		public void CheckAndDistributeCollision ()
		{
			if (!Active)
				return;
			CurrentCollisionPair = this;

			if (CheckCollision ()) {
				if (IsColliding == false) {
					if (Body1.OnContactEnter != null)
						Body1.OnContactEnter (Body2);
					if (Body2.OnContactEnter != null)
						Body2.OnContactEnter (Body1);
					IsColliding = true;
				} else {

				}
				DistributeCollision ();
			} else {
				if (IsColliding) {
					if (Body1.OnContactExit != null) {
						Body1.OnContactExit (Body2);
					}
					if (Body2.OnContactExit != null) {
						Body2.OnContactExit (Body1);
					}
					IsColliding = false;
				} else {

				}
			}

		}
		public bool CheckCollision ()
		{
			if (!(Body1.PositionChanged || Body2.PositionChanged || Body1.RotationChanged || Body2.RotationChanged)) {
				return IsColliding;
			}

			switch (LeCollisionType) {
			case CollisionType.None:
				break;

			//Check
			case CollisionType.Circle_Circle:
				return CheckCircle ();
				break;
			
			//Check
			case CollisionType.Circle_AABox:
				if (CheckBox ()) {
					if (CheckCircle ()) {

						if (Body1.Shape == ColliderType.AABox) {
							if (CheckCircle_Box (Body1, Body2)) {

								return true;
							}

						} else {
							if (CheckCircle_Box (Body2, Body1)) {
								return true;
							}
						}
					}
				}

				break;
			
			case CollisionType.Circle_Polygon:
				return false;
				if (CheckCircle ()) {
					if (Body1.Shape == ColliderType.Circle) {
						if (CheckCircle_Poly (Body1, Body2)) {
							return true;
						}
					} else if (CheckCircle_Poly (Body2, Body1)) {
						return true;
					}
				}
				break;

			//Check
			case CollisionType.AABox_AABox:
				return false;
				if (DoPhysics) {
					if (CheckCircle ()) {
						if (CheckBox ()) {
							return true;
						}
					}
				} else {
					if (CheckBox ()) {
						return true;
					}
				}
				break;

			case CollisionType.AABox_Polygon:
				return false;
				if (CheckCircle ()) {
					if (Body1.Shape == ColliderType.AABox) {
						if (CheckBox_Poly (Body1, Body2))
							return true;
					} else {
						if (CheckBox_Poly (Body2, Body1)) {

							return true;
						}
					}
				}
				break;

			case CollisionType.Polygon_Polygon:
				return false;
				if (CheckCircle ()) {
					if (CheckPoly_Poly (Body1, Body2)) {
						return true;
					}
				}
				break;
			}

			return false;
		}

		public bool CheckCircle ()
		{

			DistX = Body1.Position.x - Body2.Position.x;
			DistY = Body1.Position.y - Body2.Position.y;
			if ((DistX * DistX + DistY * DistY) <= CacheSqrDistance) {
				return true;
			}

			if (Body1.VelocityMagnitude != 0 || Body2.VelocityMagnitude != 0) {
				DistX = Body1.FuturePosition.x - Body2.FuturePosition.x;
				DistY = Body1.FuturePosition.y - Body2.FuturePosition.y;
				if ((DistX * DistX + DistY * DistY) <= CacheSqrDistance) {
					return true;
				}
			}

			return false;
		}

		public bool CheckBox ()
		{
			if (Body1.XMin <= Body2.XMax) {
				if (Body1.XMax >= Body2.XMin) {
					if (Body1.YMin <= Body2.YMax) {
						if (Body1.YMax >= Body2.YMin) {
							return true;
						}
					}
				}
			}
			if (Body1.VelocityMagnitude != 0 || Body2.VelocityMagnitude != 0) {
				if (Body1.FutureXMin < Body2.FutureXMax) {
					if (Body1.FutureXMax > Body2.FutureXMin) {
						if (Body1.FutureYMin < Body2.FutureYMax) {
							if (Body1.FutureYMax > Body2.FutureYMin) {
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		static long Mag;

		public void HandleImmovableCircleBoxCollision (LSBody imov, LSBody mov)
		{

		}

		public static bool CheckBox_Poly (LSBody box, LSBody poly)
		{
			bool Right = poly.Position.x > box.Position.x;
			bool Top = poly.Position.y > box.Position.y;
			bool xPassed = false;
			bool yPassed = false;
			int vertCount = poly.RealPoints.Length;
			for (int i = 0; i < vertCount; i++) {
				if (!xPassed) {
					if (Right) {
						if (poly.RealPoints [i].x <= box.XMax)
							xPassed = true;
					} else {
						if (poly.RealPoints [i].x >= box.XMin)
							xPassed = true;
					}
				}
				if (!yPassed) {
					if (Top) {
						if (poly.RealPoints [i].y <= box.YMax)
							yPassed = true;
					} else {
						if (poly.RealPoints [i].y >= box.YMin)
							yPassed = true;
					}
				}
				if (xPassed && yPassed)
					return true;
			}
			return false;
		}

		public static bool CheckCircle_Poly (LSBody circle, LSBody poly)
		{
			int EdgeCount = poly.EdgeNorms.Length;
			for (int i = 0; i < EdgeCount; i++) {
				Vector2d axis = poly.EdgeNorms [i];
				long CircleProjection = circle.Position.Dot (axis.x, axis.y);
				long CircleMin = CircleProjection - circle.Radius;
				long CircleMax = CircleProjection + circle.Radius;

				long PolyMin;
				long PolyMax;
				ProjectPolygon (axis.x, axis.y, poly, out PolyMin, out PolyMax);

				if (!CheckOverlap (CircleMin, CircleMax, PolyMin, PolyMax)) {
					return false;
				}
			}


			return true;
		}

		static long xDist;
		static long yDist;
		static bool xMore;
		static bool yMore;
		static bool Collided;
		static long xAbs, yAbs;

		public void DistributeCircle_Box (LSBody box, LSBody circle)
		{
			xMore = circle.Position.x > box.Position.x;
			yMore = circle.Position.y > box.Position.y;

			if (xMore) {
				PenetrationX = (circle.XMin - box.XMax);
			} else {
				PenetrationX = (circle.XMax - box.XMin);
			}
			if (yMore) {
				PenetrationY = (circle.YMin - box.YMax);
			} else {
				PenetrationY = (circle.YMax - box.YMin);
			}


			xAbs = PenetrationX < 0 ? -PenetrationX : PenetrationX;
			yAbs = PenetrationY < 0 ? -PenetrationY : PenetrationY;
			if (xAbs > yAbs) {
				PenetrationX = 0;//FixedMath.Mul (PenetrationX, FixedMath.One * 1 / 4);
			} else {
				PenetrationY = 0;//FixedMath.Mul (PenetrationX, FixedMath.One * 1 / 4);
			}

			//Resolving
			circle.Position.x -= PenetrationX;//(PenetrationX * Multiplier) >> FixedMath.SHIFT_AMOUNT;
			circle.Position.y -= PenetrationY;//(PenetrationY * Multiplier) >> FixedMath.SHIFT_AMOUNT;
			circle.Velocity.x -= PenetrationX;
			circle.Velocity.y -= PenetrationY;
			circle.VelocityChanged = true;
			circle.PositionChanged = true;
			circle.BuildBounds ();
		}

		public static bool CheckCircle_Box (LSBody box, LSBody circle)
		{
			Collided = false;

			xMore = circle.Position.x > box.Position.x;
			yMore = circle.Position.y > box.Position.y;
			if (!Collided) {
				Collided = false;
				if (xMore) {
					if (circle.Position.x <= box.XMax) {
						Collided = true;
					}
				} else {
					if (circle.Position.x >= box.XMin) {
						Collided = true;
					}
				}

				if (yMore) {
					if (circle.Position.y <= box.YMax) {
						Collided = true;
					}
				} else {
					if (circle.Position.y >= box.YMin) {
						Collided = true;
					}
				}

				if (!Collided) {
					if (xMore) {
						xDist = (circle.Position.x) - (box.XMax);
					} else {
						xDist = (circle.Position.x) - (box.XMin);
					}
					if (yMore) {
						yDist = (circle.Position.y) - (box.YMax);
					} else {
						yDist = (circle.Position.y) - (box.YMin);
					}

					if ((xDist * xDist + yDist * yDist) <= circle.Radius * circle.Radius) {
						Collided = true;
					}
				}	
			}

			return Collided;
		}

		public static bool CheckPoly_Poly (LSBody poly1, LSBody poly2)
		{
			int Poly1EdgeCount = poly1.EdgeNorms.Length;
			int EdgeCount = Poly1EdgeCount + poly2.EdgeNorms.Length;
			for (int i = 0; i < EdgeCount; i++) {
				Vector2d edge;
				if (i < Poly1EdgeCount) {
					edge = poly1.EdgeNorms [i];
				} else {
					edge = poly1.EdgeNorms [i - Poly1EdgeCount];
				}
				long Poly1Min;
				long Poly1Max;
				ProjectPolygon (edge.x, edge.y, poly1, out Poly1Min, out Poly1Max);
				long Poly2Min;
				long Poly2Max;
				ProjectPolygon (edge.x, edge.y, poly2, out Poly2Min, out Poly2Max);
				if (!CheckOverlap (Poly1Min, Poly1Max, Poly2Min, Poly2Max))
					return false;
			}
			return true;
		}

		public static void ProjectPolygon (long AxisX, long AxisY, LSBody Poly, out long Min, out long Max)
		{

			Min = Poly.RealPoints [0].Dot (AxisX, AxisY);
			Max = Min;

			int PointCount = Poly.RealPoints.Length;
			long Projection;
			for (int i = 1; i < PointCount; i++) {
				Projection = Poly.RealPoints [i].Dot (AxisX, AxisY);
				if (Projection < Min) {
					Min = Projection;
				} else if (Projection > Max)
					Max = Projection;
			}
		}

		public static long IntervalDistance (long Min1, long Max1, long Min2, long Max2)
		{
			if (Min1 < Min2) {
				return Min2 - Max1;
			} else {
				return Min1 - Max2;
			}
		}

		public static bool CheckOverlap (long Min1, long Max1, long Min2, long Max2)
		{
			if (Max1 >= Min2) {
				if (Min1 <= Max2) {
					return true;
				}
			}

			return false;
		}
		private enum CollisionType : byte
		{
			None,
			Circle_Circle,
			Circle_AABox,
			Circle_Polygon,
			AABox_AABox,
			AABox_Polygon,
			Polygon_Polygon
		}

		private enum PhysicsFavor
		{
			Same,
			Favor1,
			Favor2
		}
	}
}
