using System;
using UnityEngine;
using Lockstep.Pathfinding;
using FastCollections;

namespace Lockstep
{
	public class Move : ActiveAbility
	{
		//Stop multipliers determine accuracy required for stopping on the destination
		public const long FormationStop = FixedMath.One / 4;
		public const long GroupDirectStop = FixedMath.One;
		public const long DirectStop = FixedMath.One / 4;
		private const int MinimumOtherStopTime = (int)(LockstepManager.FrameRate / 4);
		private const int StuckTimeThreshold = LockstepManager.FrameRate / 4;
		private const int StuckRepathTries = 4;

		public int GridSize { get { return (cachedBody.Radius).CeilToInt(); } }


		public Vector2d Position { get { return cachedBody._position; } }

		public long CollisionSize { get { return cachedBody.Radius; } }

		public MovementGroup MyMovementGroup { get; set; }

		public int MyMovementGroupID { get; set; }

		public bool IsFormationMoving { get; set; }

		public bool IsMoving { get; private set; }

		private bool hasPath;
		private bool straightPath;
		private bool viableDestination;
		private readonly FastList<Vector2d> myPath = new FastList<Vector2d>();
		private int _pathIndex;

		private int pathIndex
		{
			get { return _pathIndex; }
			set
			{
				if (value != _pathIndex)
				{
					_pathIndex = value;
				}
			}
		}

		private int StoppedTime;
		private Vector2d targetPos;
		[HideInInspector]
		public Vector2d Destination;


		#region Auto stopping
		public bool GetCanAutoStop()
		{
			return AutoStopPauser <= 0;
		}

		public bool GetCanCollisionStop()
		{
			return CollisionStopPauser <= 0;
		}

		const int AUTO_STOP_PAUSE_TIME = LockstepManager.FrameRate / 8;
		private int AutoStopPauser;

		public void PauseAutoStop()
		{
			AutoStopPauser = AUTO_STOP_PAUSE_TIME;
		}

		private int StopPauseLayer;

		private int CollisionStopPauser;

		public void PauseCollisionStop()
		{
			CollisionStopPauser = AUTO_STOP_PAUSE_TIME;
		}

		//TODO: Improve the naming
		bool GetLookingForStopPause()
		{
			return StopPauseLooker >= 0;
		}

		private int StopPauseLooker;
		/// <summary>
		/// Start the search process for collisions/obstructions that are in the same attack group.
		/// </summary>
		public void StartLookingForStopPause()
		{
			StopPauseLooker = AUTO_STOP_PAUSE_TIME;
		}
		#endregion
		public long StopMultiplier { get; set; }

		//Has this unit arrived at destination? Default set to false.
		public bool Arrived { get; private set; }

		//Called when unit arrives at destination
		public event Action onArrive;

		public event Action onStartMove;

		//Called whenever movement is stopped... i.e. to attack
		public event Action OnStopMove;


		private LSBody cachedBody { get; set; }

		private Turn CachedTurn { get; set; }

		public Vector2d AveragePosition { get; set; }

		private long timescaledAcceleration;
		private long timescaledDecceleration;
		bool decellerating;

		private Vector2d lastTargetPos;
		private Vector2d targetDirection;

		private Vector2d waypointDirection { get { return this.targetDirection; } }

		private GridNode currentNode;
		private GridNode destinationNode;
		private Vector2d movementDirection;
		private long distance;
		private long closingDistance;
		private long stuckTolerance;

		[Lockstep(true)]
		public bool SlowArrival
		{
			get;
			set;
		}

		#region Serialized

		[SerializeField]
		private bool _canMove = true;

		public bool CanMove
		{
			get;
			set;
		}

		[SerializeField]
		private bool _canTurn = true;

		public bool CanTurn { get; private set; }

		[SerializeField, FixedNumber]
		private long _speed = FixedMath.One * 4;

		public virtual long Speed
		{
			get { return _speed; }
		}


		[SerializeField, FixedNumber]
		private long _acceleration = FixedMath.One * 4;

		public long Acceleration { get { return _acceleration; } }

		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs("Flying")]
		private bool _canPathfind = true;

		public bool CanPathfind { get; private set; }

		#endregion

		protected override void OnSetup()
		{
			cachedBody = Agent.Body;
			cachedBody.onContact += HandleCollision;
			CachedTurn = Agent.GetAbility<Turn>();
			CanTurn = _canTurn && CachedTurn != null;

			timescaledAcceleration = Acceleration.Mul(Speed) / LockstepManager.FrameRate;
			//Cleaner stops with more decelleration
			timescaledDecceleration = timescaledAcceleration * 4;
			//Fatter objects can afford to land imprecisely
			closingDistance = cachedBody.Radius;
			stuckTolerance = ((Agent.Body.Radius * Speed) >> FixedMath.SHIFT_AMOUNT) / LockstepManager.FrameRate;
			stuckTolerance *= stuckTolerance;
			CanPathfind = _canPathfind;
			this.SlowArrival = true;
		}

		protected override void OnInitialize()
		{

			myPath.FastClear();
			_pathIndex = 0;
			StoppedTime = 0;

			CanMove = _canMove;
			IsFormationMoving = false;
			MyMovementGroupID = -1;

			AutoStopPauser = 0;
			CollisionStopPauser = 0;
			StopPauseLooker = 0;
			StopPauseLayer = 0;

			StopMultiplier = DirectStop;

			viableDestination = false;

			Destination = Vector2d.zero;
			hasPath = false;
			IsMoving = false;
			StuckTime = 0;
			RepathTries = 0;

			Arrived = true;
			AveragePosition = Agent.Body.Position;
			DoPathfind = false;
		}

		private int StuckTime
		{
			get { return _stuckTime; }
			set
			{
				_stuckTime = value;
				if (_stuckTime == 0)
				{
				}
			}
		}

		private int _stuckTime;

		private int RepathTries;

		bool DoPathfind;


		uint GetNodeHash(GridNode node)
		{
			//TODO: At the moment, the CombinePathVersion is based on the destination... essentially caching the path to the last destination
			//Should this be based on commands instead?
			//Also, a lot of redundancy can be moved into MovementGroupHelper... i.e. getting destination node 
			uint ret = (uint)(node.gridX * GridManager.Width);
			ret += (uint)node.gridY;
			return ret;
		}

		protected override void OnSimulate()
		{
			if (!CanMove)
			{
				return;
			}
			//TODO: Organize/split this function
			if (IsMoving)
			{
				Agent.SetState(AnimState.Moving);

				if (CanPathfind)
				{
					if (DoPathfind)
					{
						DoPathfind = false;
						if (viableDestination)
						{
							if (Pathfinder.GetStartNode(cachedBody.Position, out currentNode))
							{
								if (currentNode.DoesEqual(this.destinationNode))
								{
									if (this.RepathTries >= 1)
									{
										this.Arrive();
									}
								}
								else
								{
									if (straightPath)
									{
										if (Pathfinder.NeedsPath(currentNode, destinationNode, this.GridSize))
										{
											if (Pathfinder.FindPath(Destination, currentNode, destinationNode, myPath,
													GridSize, GetNodeHash(destinationNode)))
											{
												hasPath = true;
												pathIndex = 0;
											}
											else
											{
												if (IsFormationMoving)
												{
													StartMove(MyMovementGroup.Destination);
													IsFormationMoving = false;
												}
											}
											straightPath = false;
										}
										else
										{
										}
									}
									else
									{
										if (Pathfinder.NeedsPath(currentNode, destinationNode, this.GridSize))
										{
											if (Pathfinder.FindPath(Destination, currentNode, destinationNode, myPath,
													GridSize, GetNodeHash(destinationNode)))
											{
												hasPath = true;
												pathIndex = 0;
											}
											else
											{
												if (IsFormationMoving)
												{
													StartMove(MyMovementGroup.Destination);
													IsFormationMoving = false;
												}
											}
										}
										else
										{
											straightPath = true;
										}
									}
								}
							}
							else
							{

							}
						}
						else
						{
							hasPath = false;
							if (IsFormationMoving)
							{
								StartMove(MyMovementGroup.Destination);
								IsFormationMoving = false;
							}
						}
					}
					else
					{

					}

					if (straightPath)
					{
						targetPos = Destination;
					}
					else if (hasPath)
					{
						if (pathIndex >= myPath.Count)
						{
							targetPos = this.Destination;
						}
						else
						{
							targetPos = myPath[pathIndex];
						}
					}
					else
					{
						targetPos = Destination;
					}
				}
				else
				{
					targetPos = Destination;
				}

				movementDirection = targetPos - cachedBody._position;

				movementDirection.Normalize(out distance);
				if (targetPos.x != lastTargetPos.x || targetPos.y != lastTargetPos.y)
				{
					lastTargetPos = targetPos;
					targetDirection = movementDirection;
				}
				bool movingToWaypoint = (this.hasPath && this.pathIndex < myPath.Count - 1);
				long stuckThreshold = timescaledAcceleration / LockstepManager.FrameRate;

				long slowDistance = cachedBody.VelocityMagnitude.Div(timescaledDecceleration);


				if (distance > slowDistance || movingToWaypoint)
				{
					desiredVelocity = (movementDirection);
					if (CanTurn)
						CachedTurn.StartTurnDirection(movementDirection);
				}
				else
				{

					if (distance < FixedMath.Mul(closingDistance, StopMultiplier))
					{
						Arrive();
						//TODO: Don't skip this frame of slowing down
						return;
					}
					if (distance > closingDistance)
					{
						if (CanTurn)
							CachedTurn.StartTurnDirection(movementDirection);
					}
					if (distance <= slowDistance)
					{
						long closingSpeed = distance.Div(slowDistance);
						if (CanTurn)
							CachedTurn.StartTurnDirection(movementDirection);
						desiredVelocity = movementDirection * closingSpeed;
						decellerating = true;
						//Reduce occurence of units preventing other units from reaching destination
						stuckThreshold *= 4;
					}


				}
				//If unit has not moved stuckThreshold in a frame, it's stuck
				StuckTime++;
				if (GetCanAutoStop())
				{
					if (Agent.Body.Position.FastDistance(AveragePosition) <= (stuckThreshold * stuckThreshold))
					{

						if (StuckTime > StuckTimeThreshold)
						{
							if (movingToWaypoint)
							{
								this.pathIndex++;
							}
							else
							{
								if (RepathTries < StuckRepathTries)
								{
									DoPathfind = true;
									RepathTries++;
								}
								else
								{
									RepathTries = 0;
									this.Arrive();
								}
							}
							StuckTime = 0;
						}
					}
					else
					{
						if (StuckTime > 0)
							StuckTime -= 1;

						RepathTries = 0;
					}
				}
				if (movingToWaypoint)
				{

					if (
						(
							this.pathIndex >= 0 &&
							distance < closingDistance &&
							(movementDirection).Dot(waypointDirection) < 0
						) ||
						distance < FixedMath.Mul(closingDistance, FixedMath.Half))
					{
						this.pathIndex++;
					}
				}

				desiredVelocity *= Speed;
				cachedBody._velocity += GetAdjustVector(desiredVelocity);

				cachedBody.VelocityChanged = true;


			}
			else
			{
				decellerating = true;

				//Slowin' down
				if (cachedBody.VelocityMagnitude > 0)
				{
					cachedBody.Velocity += GetAdjustVector(Vector2d.zero);
				}
				StoppedTime++;
			}
			decellerating = false;

			AutoStopPauser--;
			CollisionStopPauser--;
			StopPauseLooker--;
			AveragePosition = AveragePosition.Lerped(Agent.Body.Position, FixedMath.One / 2);

		}

		Vector2d GetAdjustVector(Vector2d desiredVel)
		{
			var adjust = desiredVel - cachedBody._velocity;
			var adjustFastMag = adjust.FastMagnitude();
			//Cap acceleration vector magnitude
			float accel = timescaledAcceleration;
			if (decellerating)
			{
				accel = timescaledDecceleration;
			}
			if (adjustFastMag > timescaledAcceleration * (timescaledAcceleration))
			{
				var mag = FixedMath.Sqrt(adjustFastMag >> FixedMath.SHIFT_AMOUNT);
				adjust *= timescaledAcceleration.Div(mag);
			}
			return adjust;
		}

		public Command LastCommand;

		protected override void OnExecute(Command com)
		{
			LastCommand = com;
			if (com.ContainsData<Vector2d>())
			{
				StartFormalMove(com.GetData<Vector2d>());
			}
		}

		public void StartFormalMove(Vector2d position)
		{
			Agent.StopCast(ID);
			IsCasting = true;
			RegisterGroup();

		}

		public void RegisterGroup(bool moveOnProcessed = true)
		{
			MoveOnGroupProcessed = moveOnProcessed;
			if (MovementGroupHelper.CheckValidAndAlert())
				MovementGroupHelper.LastCreatedGroup.Add(this);


		}

		public void Arrive()
		{
			StopMove();

			if (onArrive.IsNotNull())
			{
				onArrive();
			}
			this.OnArrive();

			//TODO: Reset this variables when changing destination/command
			AutoStopPauser = 0;
			CollisionStopPauser = 0;
			StopPauseLooker = 0;
			StopPauseLayer = 0;

			Arrived = true;
		}

		protected virtual void OnArrive()
		{

		}


		public void StopMove()
		{
			if (IsMoving)
			{

				if (MyMovementGroup.IsNotNull())
				{
					MyMovementGroup.Remove(this);
				}

				IsMoving = false;
				StoppedTime = 0;

				IsCasting = false;
				if (OnStopMove.IsNotNull())
				{
					OnStopMove();
				}
			}
		}

		public void OnGroupProcessed(Vector2d destination)
		{
			Destination = destination;
			if (MoveOnGroupProcessed)
			{
				StartMove(destination);
				MoveOnGroupProcessed = false;
			}
			else
			{
				this.Destination = destination;
			}
			if (this.onGroupProcessed != null)
				this.onGroupProcessed();
		}

		public event Action onGroupProcessed;

		public bool MoveOnGroupProcessed { get; private set; }




		public void StartMove(Vector2d destination)
		{
			DoPathfind = true;
			hasPath = false;
			straightPath = false;
			this.Destination = destination;
			IsMoving = true;
			StoppedTime = 0;
			Arrived = false;

			//For now, use old next-best-node system when size requires consideration
			viableDestination = this.GridSize <= 1 ?
				Pathfinder.GetEndNode(Agent.Body.Position, destination, out destinationNode) :
				Pathfinder.GetClosestViableNode(Agent.Body.Position, destination, this.GridSize, out destinationNode);
			//TODO: If next-best-node, autostop more easily
			//Also implement stopping sooner based on distance

			StuckTime = 0;
			RepathTries = 0;
			IsCasting = true;
			if (onStartMove != null)
				onStartMove();
		}

		protected override void OnStopCast()
		{
			StopMove();
		}


		private int collidedCount;
		private ushort collidedID;

		static LSAgent tempAgent;

		bool paused;
		private void HandleCollision(LSBody other)
		{
			if (!CanMove)
			{
				return;
			}
			if ((tempAgent = other.Agent) == null)
			{
				return;
			}

			Move otherMover = tempAgent.GetAbility<Move>();
			if (ReferenceEquals(otherMover, null) == false)
			{
				if (IsMoving)
				{
					//If the other mover is moving to a similar point
					if (otherMover.MyMovementGroupID == MyMovementGroupID || otherMover.targetPos.FastDistance(this.targetPos) <= (closingDistance * closingDistance))
					{
						if (otherMover.IsMoving == false)
						{
							if (otherMover.Arrived && otherMover.StoppedTime > MinimumOtherStopTime)
							{
								Arrive();
							}
						}
						else
						{
							if (hasPath && otherMover.hasPath && otherMover.pathIndex > 0 && otherMover.lastTargetPos.SqrDistance(targetPos.x, targetPos.y) < closingDistance.Mul(closingDistance))
							{
								if (this.distance < this.closingDistance)
								{
									this.pathIndex++;
								}
							}
						}
					}

					if (GetLookingForStopPause())
					{
						//As soon as the original collision stop unit is released, units will start breaking out of pauses
						if (otherMover.GetCanCollisionStop() == false)
						{
							StopPauseLayer = -1;
							PauseAutoStop();
						}
						else if (otherMover.GetCanAutoStop() == false)
						{
							if (otherMover.StopPauseLayer < StopPauseLayer)
							{
								StopPauseLayer = otherMover.StopPauseLayer + 1;
								PauseAutoStop();
							}
						}
					}
					else
					{

					}
				}
			}
		}
		static Vector2d desiredVelocity;

#if UNITY_EDITOR
		public bool DrawPath = true;

		void OnDrawGizmos()
		{
			if (DrawPath)
			{
				const float height = 0f;
				for (int i = 1; i < myPath.Count; i++)
				{
					UnityEditor.Handles.Label(myPath[i - 1].ToVector3(height), i.ToString());
					Gizmos.DrawLine(myPath[i - 1].ToVector3(height), myPath[i].ToVector3(height));
				}
			}
		}
#endif
	}
}