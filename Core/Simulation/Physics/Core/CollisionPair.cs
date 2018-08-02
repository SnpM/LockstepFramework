//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
namespace Lockstep
{
	public class CollisionPair
	{
		public LSBody Body1;
		public LSBody Body2;
		private long FastCollideDistance;
		private long FastDistance;
		private CollisionType LeCollisionType;
		private bool DoPhysics = true;
		public bool Active;
		public uint PartitionVersion;

		public ushort _Version = 1;
		//If negative, prevent culling altogether
		internal short CullCounter;
		internal bool PreventDistanceCull;
		internal int LastCollidedFrame;
		private long FastDistanceOffset;

		public bool _isColliding;

		public bool IsColliding
		{
			get { return _isColliding; }
			set
			{
				_isColliding = value;
			}
		}

		static bool IsCollidingChanged;

		public int LastFrame;

		public static long DistX;
		public static long DistY;
		public static long PenetrationX;
		public static long PenetrationY;
		public static CollisionPair CurrentCollisionPair;

		bool IsValid { get; set; }

		public void Initialize(LSBody b1, LSBody b2)
		{

			IsValid = true;
			if (!IsValid)
				return;

			if (b1.ID < b2.ID)
			{
				Body1 = b1;
				Body2 = b2;
			}
			else
			{
				Body1 = b2;
				Body2 = b1;
			}

			_ranIndex = -1;

			_isColliding = false;

			DistX = 0;
			DistY = 0;
			PenetrationX = 0;
			PenetrationY = 0;

			FastCollideDistance = b1.Radius + b2.Radius;
			FastCollideDistance *= FastCollideDistance;

			LeCollisionType = CollisionType.None;
			if (Body1.Shape == ColliderType.None || Body2.Shape == ColliderType.None)
			{
			}
			else if (Body1.Shape == ColliderType.Circle)
			{
				if (Body2.Shape == ColliderType.Circle)
				{
					LeCollisionType = CollisionType.Circle_Circle;
				}
				else if (Body2.Shape == ColliderType.AABox)
				{
					LeCollisionType = CollisionType.Circle_AABox;

				}
				else if (Body2.Shape == ColliderType.Polygon)
				{
					LeCollisionType = CollisionType.Circle_Polygon;
				}
			}
			else if (Body1.Shape == ColliderType.AABox)
			{
				if (Body2.Shape == ColliderType.Circle)
				{
					LeCollisionType = CollisionType.Circle_AABox;
				}
				else if (Body2.Shape == ColliderType.AABox)
				{
					LeCollisionType = CollisionType.AABox_AABox;
				}
				else if (Body2.Shape == ColliderType.Polygon)
				{
					LeCollisionType = CollisionType.AABox_Polygon;
				}
			}
			else if (Body1.Shape == ColliderType.Polygon)
			{
				if (Body2.Shape == ColliderType.Circle)
				{
					LeCollisionType = CollisionType.Circle_Polygon;
				}
				else if (Body2.Shape == ColliderType.AABox)
				{
					LeCollisionType = CollisionType.AABox_Polygon;
				}
				else if (Body2.Shape == ColliderType.Polygon)
				{
					LeCollisionType = CollisionType.Polygon_Polygon;
				}
			}

			DoPhysics = ((Body1.IsTrigger || Body2.IsTrigger) == false);
			if (DoPhysics)
			{

			}


			//TODO: Space out checks when culled
			//TODO: The time between collision checks might cause goofy behavior
			//Maybe use a distance or velocity heuristic for culling instead of time since last collision
			//It wouldn't be able to replace partitions because of raycasts and fast-moving objects
			//Let's see if this works well or if something better is needed. 
			if (Body1.PreventCulling || Body2.PreventCulling)
			{
				//Never cull
				CullCounter = -1;
			}
			else
			{
				//Immediately check collision
				CullCounter = 0;
				//If collision distance is too large, don't cull based on distance
				PreventDistanceCull = FastCollideDistance > PhysicsManager.CullFastDistanceMax;
				LastCollidedFrame = LockstepManager.FrameCount;
				FastDistanceOffset = FixedMath.Sqrt(FastCollideDistance >> FixedMath.SHIFT_AMOUNT) + FixedMath.One * 2;
				FastDistanceOffset *= FastDistanceOffset;
			}
			Active = true;
			_Version++;
		}

		public void Deactivate()
		{
			if (IsColliding)
			{
				Body1.NotifyContact(Body2, false, IsColliding);
				Body2.NotifyContact(Body1, false, IsColliding);
			}
			Active = false;
			if (_ranIndex >= 0)
			{
				PhysicsManager.RanCollisionPairs.RemoveAt(_ranIndex);
				_ranIndex = -1;
			}
		}

		static long dist, depth;

		private void DistributeCollision()
		{
			if (!DoPhysics)
				return;

			switch (LeCollisionType)
			{
				case CollisionType.Circle_Circle:
					DistX = Body1._position.x - Body2._position.x;
					DistY = Body1._position.y - Body2._position.y;
					dist = FixedMath.Sqrt((DistX * DistX + DistY * DistY) >> FixedMath.SHIFT_AMOUNT);

					if (dist == 0)
					{
						//If objects are on the same position, give them push in random direction
						const long randomMax = FixedMath.One / 32;
						Body1._position.x += LSUtility.GetRandomLong(randomMax) - randomMax / 2;
						Body1._position.y += LSUtility.GetRandomLong(randomMax) - randomMax / 2;
						Body1.PositionChanged = true;
						Body2._position.x += LSUtility.GetRandomLong(randomMax) - randomMax / 2;
						Body2._position.y += LSUtility.GetRandomLong(randomMax) - randomMax / 2;
						Body2.PositionChanged = true;
						return;
					}


					depth = (Body1.Radius + Body2.Radius - dist);

					if (depth <= 0)
					{
						return;
					}
					DistX = (DistX * depth / dist);
					DistY = (DistY * depth / dist);

					//Resolving collision


					if (Body1.Immovable || (Body2.Immovable == false && Body1.Priority > Body2.Priority))
					{
						DistX *= -1;
						DistY *= -1;
						DistributeCircle_CirclePriority(Body1, Body2);
					}
					else if (Body2.Immovable || Body2.Priority > Body1.Priority)
					{
						DistributeCircle_CirclePriority(Body2, Body1);
					}
					else
					{
						DistX /= 2;
						DistY /= 2;
						DistributeCircle(Body1);
						DistX *= -1;
						DistY *= -1;
						DistributeCircle(Body2);
					}
					break;
				case CollisionType.Circle_AABox:
					if (Body1.Shape == ColliderType.AABox)
					{
						DistributeCircle_Box(Body1, Body2);
					}
					else
					{
						DistributeCircle_Box(Body2, Body1);
					}
					break;

				case CollisionType.Circle_Polygon:
					if (Body1.Shape == ColliderType.Circle)
					{
						this.DistributeCircle_Poly(Body1, Body2);
					}
					else
					{
						this.DistributeCircle_Poly(Body2, Body1);
					}
					break;
			}
		}

		void DistributeCircle_CirclePriority(LSBody higherPriority, LSBody lowerPriority)
		{


			if (true || higherPriority.Immovable || lowerPriority.ImmovableCollisionDirection.EqualsZero())
			{
				DistributeCircle(lowerPriority);
				lowerPriority.ImmovableCollisionDirection = new Vector2d(DistX, DistY);

			}
			else
			{
				//TODO: Fix this behavior. It's supposed to prevent pass-through between i.e. buildings.
				//Only move if there isn't an immovable object in that direction
				if (lowerPriority.ImmovableCollisionDirection.x.Sign() != DistX.Sign())
				{
					lowerPriority._position.x += DistX;
				}
				if (lowerPriority.ImmovableCollisionDirection.y.Sign() != DistY.Sign())
				{
					lowerPriority._position.y += DistY;
				}
			}
		}

		void DistributeCircle(LSBody body)
		{
			body._position.x += DistX + PhysicsManager.PositionCycler;
			body._position.y += DistY;
			body.PositionChanged = true;

			if (true)
			{
				body._velocity.x += DistX / 8;
				body._velocity.y += DistY / 8;
				body.VelocityChanged = true;
			}
		}

		void DistributeCircle_Poly(LSBody circle, LSBody poly)
		{
			Vector2d edgeAxis = ClosestAxis.rotatedRight;
			long horProjection = circle._position.Dot(edgeAxis.x, edgeAxis.y);
			long verProjection = ClosestAxisProjection + ClosestDist;
			Vector2d newPos = ClosestAxis * verProjection + edgeAxis * horProjection;
			circle._position = newPos;
			circle.PositionChanged = true;
		}

		public int _ranIndex;

		public void CheckAndDistributeCollision()
		{

			if (!Active)
			{
				return;
			}
			if (_ranIndex < 0)
			{
				_ranIndex = PhysicsManager.RanCollisionPairs.Add(new PhysicsManager.InstanceCollisionPair(_Version, this));
			}
			IsCollidingChanged = false;
			LastFrame = LockstepManager.FrameCount;
			CurrentCollisionPair = this;
			if (CullCounter <= 0)
			{
				GenerateCircleValues();
				if (CheckHeight())
				{
					bool result = CheckCollision();

					if (result != IsColliding)
					{
						IsColliding = result;
						IsCollidingChanged = true;
					}
					if (CheckCollision())
					{
						DistributeCollision();
					}
				}
				Body1.NotifyContact(Body2, IsColliding, IsCollidingChanged);
				Body2.NotifyContact(Body1, IsColliding, IsCollidingChanged);
				if (IsColliding == false)
				{
					//A negative cull counter means a Body is preventing culling
					if (CullCounter >= 0)
					{

						if (PreventDistanceCull)
						{
							CullCounter = (short)(
								(LockstepManager.FrameCount - LastCollidedFrame) / PhysicsManager.CullTimeStep);
							if (CullCounter > PhysicsManager.CullTimeMax)
								CullCounter = PhysicsManager.CullTimeMax;
						}
						else
						{
							//Set number of frames until next collision check based on distance
							var distCull = (
										((FastDistance - FastDistanceOffset) >> FixedMath.SHIFT_AMOUNT)
												  / PhysicsManager.CullDistanceStep + PhysicsManager.CullDistributor
											  );
							if (distCull > PhysicsManager.CullDistanceMax)
								distCull = PhysicsManager.CullDistanceMax;
							if (distCull < 0)
								distCull = 0;

							var timeCull = (LockstepManager.FrameCount - LastCollidedFrame) / PhysicsManager.CullTimeStep;
							if (timeCull > PhysicsManager.CullTimeMax)
								timeCull = PhysicsManager.CullTimeMax;

							CullCounter = (short)(timeCull + distCull);
						}

					}
				}
				else
				{
					LastCollidedFrame = LockstepManager.FrameCount;
				}
			}
			else
			{
				if (Body1.PartitionChanged || Body2.PartitionChanged)
				{
					//New partition so collision culling may not be calculated yet
					CullCounter = 0;
					CheckAndDistributeCollision();
				}
				else
				{
					//Culled and counter 1 step closer until checking again
					CullCounter--;
				}
			}

		}

		public bool CheckHeight()
		{
			return Body1.HeightMax >= Body2.HeightMin && Body1.HeightMin <= Body2.HeightMax;
		}

		public bool CheckCollision()
		{
			if (!Body1.PositionChangedBuffer && !Body2.PositionChangedBuffer && !Body1.RotationChangedBuffer && !Body2.RotationChangedBuffer)
			{
				return IsColliding;
			}
			switch (LeCollisionType)
			{
				case CollisionType.None:
					break;

				//Check
				case CollisionType.Circle_Circle:
					return CheckCircle();
				//break;

				//Check
				case CollisionType.Circle_AABox:
					if (CheckBox())
					{

						if (Body1.Shape == ColliderType.AABox)
						{
							if (CheckCircle_Box(Body1, Body2))
							{
								return true;
							}

						}
						else
						{
							if (CheckCircle_Box(Body2, Body1))
							{
								return true;
							}
						}

					}

					break;

				case CollisionType.Circle_Polygon:
					if (CheckCircle())
					{
						if (Body1.Shape == ColliderType.Circle)
						{
							if (CheckCircle_Poly(Body1, Body2))
							{
								return true;
							}
						}
						else
						{
							if (CheckCircle_Poly(Body2, Body1))
							{
								return true;
							}
						}
					}
					break;

				//Check
				case CollisionType.AABox_AABox:
					//Not supported
					return false;
				/*
						if (DoPhysics)
						{
							if (CheckCircle())
							{
								if (CheckBox())
								{
									return true;
								}
							}
						} else
						{
							if (CheckBox())
							{
								return true;
							}
						}
						break;
						*/

				case CollisionType.AABox_Polygon:
					//Not supported
					return false;
				/*
						if (CheckCircle())
						{
							if (Body1.Shape == ColliderType.AABox)
							{
								if (CheckBox_Poly(Body1, Body2))
								{
									return true;
								}
							} else
							{
								if (CheckBox_Poly(Body2, Body1))
								{

									return true;
								}
							}
						}
						break;
						*/

				case CollisionType.Polygon_Polygon:
					//Not supported
					return false;
					/*
							if (CheckCircle())
							{
								if (CheckPoly_Poly(Body1, Body2))
								{
									return true;
								}
							}
							break;
							*/
			}

			return false;
		}

		private void GenerateCircleValues()
		{
			DistX = Body1._position.x - Body2._position.x;
			DistY = Body1._position.y - Body2._position.y;
			FastDistance = DistX * DistX + DistY * DistY;
		}
		private bool CheckCircle()
		{
			if (FastDistance <= FastCollideDistance)
			{
				return true;
			}
			return false;
		}

		private bool CheckBox()
		{
			if (Body1.XMin <= Body2.XMax)
			{
				if (Body1.XMax >= Body2.XMin)
				{
					if (Body1.YMin <= Body2.YMax)
					{
						if (Body1.YMax >= Body2.YMin)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private static bool CheckBox_Poly(LSBody box, LSBody poly)
		{
			bool Right = poly._position.x > box._position.x;
			bool Top = poly._position.y > box._position.y;
			bool xPassed = false;
			bool yPassed = false;
			int vertCount = poly.RealPoints.Length;
			for (int i = 0; i < vertCount; i++)
			{
				if (!xPassed)
				{
					if (Right)
					{
						if (poly.RealPoints[i].x <= box.XMax)
						{
							xPassed = true;
						}
					}
					else
					{
						if (poly.RealPoints[i].x >= box.XMin)
						{
							xPassed = true;
						}
					}
				}
				if (!yPassed)
				{
					if (Top)
					{
						if (poly.RealPoints[i].y <= box.YMax)
						{
							yPassed = true;
						}
					}
					else
					{
						if (poly.RealPoints[i].y >= box.YMin)
						{
							yPassed = true;
						}
					}
				}
				if (xPassed && yPassed)
				{
					return true;
				}
			}
			return false;
		}

		private static Vector2d ClosestAxis;
		private static long ClosestDist;
		private static long ClosestAxisProjection;


		private static bool CheckCircle_Poly(LSBody circle, LSBody poly)
		{
			int EdgeCount = poly.EdgeNorms.Length;
			ClosestDist = long.MaxValue;
			for (int i = 0; i < EdgeCount; i++)
			{
				Vector2d axis = poly.EdgeNorms[i];
				long CircleProjection = circle._position.Dot(axis.x, axis.y);
				long CircleMin = CircleProjection - circle.Radius;
				long CircleMax = CircleProjection + circle.Radius;

				long PolyMin;
				long PolyMax;
				ProjectPolygon(axis.x, axis.y, poly, out PolyMin, out PolyMax);
				//TODO: Cache PolyMin and PolyMax?
				if (CheckOverlap(CircleMin, CircleMax, PolyMin, PolyMax))
				{
					long dist1 = PolyMax - CircleMin;
					long dist2 = CircleMax - PolyMin;
					long localCloseDist = 0;
					if (dist1 <= dist2)
					{
						localCloseDist = dist1;
					}
					else
					{
						localCloseDist = -dist2;
					}
					if (localCloseDist.Abs() < ClosestDist.Abs())
					{
						ClosestDist = localCloseDist;
						ClosestAxis = axis;
						ClosestAxisProjection = CircleProjection;
					}
				}
				else
				{
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

		private void DistributeCircle_Box(LSBody box, LSBody circle)
		{
			xMore = circle._position.x > box._position.x;
			yMore = circle._position.y > box._position.y;

			if (xMore)
			{
				PenetrationX = (circle.XMin - box.XMax);
			}
			else
			{
				PenetrationX = (circle.XMax - box.XMin);
			}
			if (yMore)
			{
				PenetrationY = (circle.YMin - box.YMax);
			}
			else
			{
				PenetrationY = (circle.YMax - box.YMin);
			}

			//PenetrationX = PenetrationX + circle.Velocity.x;
			//PenetrationY = PenetrationY + circle.Velocity.y;
			xAbs = PenetrationX < 0 ? -PenetrationX : PenetrationX;
			yAbs = PenetrationY < 0 ? -PenetrationY : PenetrationY;

			if ((xAbs <= circle.Radius && yAbs <= circle.Radius))
			{
				Vector2d corner;
				corner.x = xMore ? box.Position.x + box.HalfWidth : box.Position.x - box.HalfWidth;
				corner.y = yMore ? box.Position.y + box.HalfHeight : box.Position.y - box.HalfHeight;
				Vector2d dir = circle.Position - corner;
				dir.Normalize();

				circle.Position = corner + dir * circle.Radius;
			}
			else
			{
				if (xAbs > yAbs)
				{
					PenetrationX = 0;
					//if (yAbs < circle.Radius) PenetrationY = PenetrationY * yAbs / circle.Radius;
					if (PenetrationY > 0 == yMore)
						PenetrationY = -PenetrationY;

				}
				else
				{
					PenetrationY = 0;
					//if (xAbs < circle.Radius) PenetrationX = PenetrationX * xAbs / circle.Radius;
					if (PenetrationX > 0 == xMore)
						PenetrationX = -PenetrationX;
				}



				//Resolving
				circle._position.x -= PenetrationX;
				circle._position.y -= PenetrationY;
			}




			circle.PositionChanged = true;
			circle.BuildBounds();
		}

		public static bool CheckCircle_Box(LSBody box, LSBody circle)
		{
			Collided = false;

			xMore = circle._position.x > box._position.x;
			yMore = circle._position.y > box._position.y;
			if (!Collided)
			{
				Collided = false;
				if (xMore)
				{
					if (circle._position.x <= box.XMax)
					{
						Collided = true;
					}
				}
				else
				{
					if (circle._position.x >= box.XMin)
					{
						Collided = true;
					}
				}

				if (yMore)
				{
					if (circle._position.y <= box.YMax)
					{
						Collided = true;
					}
				}
				else
				{
					if (circle._position.y >= box.YMin)
					{
						Collided = true;
					}
				}

				if (!Collided)
				{
					if (xMore)
					{
						xDist = (circle._position.x) - (box.XMax);
					}
					else
					{
						xDist = (circle._position.x) - (box.XMin);
					}
					if (yMore)
					{
						yDist = (circle._position.y) - (box.YMax);
					}
					else
					{
						yDist = (circle._position.y) - (box.YMin);
					}

					if ((xDist * xDist + yDist * yDist) <= circle.Radius * circle.Radius)
					{
						Collided = true;
					}
				}
			}

			return Collided;
		}

		public static bool CheckPoly_Poly(LSBody poly1, LSBody poly2)
		{
			int Poly1EdgeCount = poly1.EdgeNorms.Length;
			int EdgeCount = Poly1EdgeCount + poly2.EdgeNorms.Length;
			for (int i = 0; i < EdgeCount; i++)
			{
				Vector2d edge;
				if (i < Poly1EdgeCount)
				{
					edge = poly1.EdgeNorms[i];
				}
				else
				{
					edge = poly1.EdgeNorms[i - Poly1EdgeCount];
				}
				long Poly1Min;
				long Poly1Max;
				ProjectPolygon(edge.x, edge.y, poly1, out Poly1Min, out Poly1Max);
				long Poly2Min;
				long Poly2Max;
				ProjectPolygon(edge.x, edge.y, poly2, out Poly2Min, out Poly2Max);
				if (!CheckOverlap(Poly1Min, Poly1Max, Poly2Min, Poly2Max))
				{
					return false;
				}
			}
			return true;
		}

		public static void ProjectPolygon(long AxisX, long AxisY, LSBody Poly, out long Min, out long Max)
		{
			Min = Poly.RealPoints[0].Dot(AxisX, AxisY);
			Max = Min;

			int PointCount = Poly.RealPoints.Length;
			long Projection;
			for (int i = 1; i < PointCount; i++)
			{
				Projection = Poly.RealPoints[i].Dot(AxisX, AxisY);
				if (Projection < Min)
				{
					Min = Projection;
				}
				else if (Projection > Max)
				{
					Max = Projection;
				}
			}
		}

		public static long IntervalDistance(long Min1, long Max1, long Min2, long Max2)
		{
			if (Min1 < Min2)
			{
				return Min2 - Max1;
			}
			else
			{
				return Min1 - Max2;
			}
		}

		public static bool CheckOverlap(long Min1, long Max1, long Min2, long Max2)
		{
			if (Max1 >= Min2)
			{
				if (Min1 <= Max2)
				{
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

	}
}
