using System;
using UnityEngine;
using Lockstep.Pathfinding;
using FastCollections;
namespace Lockstep
{
	public class Move : ActiveAbility
	{
		public const long FormationStop = FixedMath.One / 4;
		public const long GroupDirectStop = FixedMath.One;
		public const long DirectStop = FixedMath.One / 8;
		private const int MinimumOtherStopTime = (int)(LockstepManager.FrameRate / 4);
		private const int StuckTimeThreshold = LockstepManager.FrameRate / 4;
		private const int StuckRepathTries = 4;

		public int GridSize { get { return (cachedBody.Radius * 2).ToInt(); } }


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
		private int pathIndex;
		private int StoppedTime;
		private Vector2d targetPos;
		[HideInInspector]
		public Vector2d Destination;

		public bool GetFullCanCollisionStop()
		{

			return CanCollisionStop && TempCanCollisionStop;
		}

		public bool CanCollisionStop { get; set; }

		public bool TempCanCollisionStop { get; set; }

		public long StopMultiplier { get; set; }


		private bool collisionTicked;

		public bool CollisionTicked { get { return collisionTicked; } }


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

		public long timescaledSpeed {
			get {
				long ret = ((this.Speed + this.AdditiveSpeedModifier) / LockstepManager.FrameRate).Mul(FixedMath.One + this.MultiplicativeSpeedModifier);
				if (ret < 0)
					ret = 0;
				return ret;
			}
		}

		private long timescaledAcceleration;

		[Lockstep(true)]
		public long AdditiveSpeedModifier { get; set; }

		[Lockstep(true)]
		public long MultiplicativeSpeedModifier { get; set; }

		private Vector2d lastTargetPos;
		private Vector2d targetDirection;
		private GridNode currentNode;
		private GridNode destinationNode;
		private Vector2d movementDirection;
		private long distance;
		private long closingDistance;
		private long stuckTolerance;

		[Lockstep(true)]
		public bool SlowArrival {
			get;
			set;
		}

		#region Serialized

		[SerializeField]
		private bool _canMove = true;

		public bool CanMove {
			get { return _canMove; }
		}

		[SerializeField]
		private bool _canTurn = true;

		public bool CanTurn { get; private set; }

		[SerializeField, FixedNumber]
		private long _speed = FixedMath.One * 4;

		public virtual long Speed {
			get { return _speed; }
		}


		[SerializeField, FixedNumber]
		private long _acceleration = FixedMath.One;

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

			timescaledAcceleration = Acceleration * 32 / LockstepManager.FrameRate;
			if (timescaledAcceleration > FixedMath.One)
				timescaledAcceleration = FixedMath.One;
			closingDistance = cachedBody.Radius;
			stuckTolerance = ((Agent.Body.Radius * Speed) >> FixedMath.SHIFT_AMOUNT) / LockstepManager.FrameRate;
			stuckTolerance *= stuckTolerance;
			CanPathfind = _canPathfind;
			this.SlowArrival = true;
		}

		protected override void OnInitialize()
		{
			myPath.FastClear();
			pathIndex = 0;
			StoppedTime = 0;

			IsFormationMoving = false;
			MyMovementGroupID = -1;
			CanCollisionStop = true;
			TempCanCollisionStop = true;
			StopMultiplier = DirectStop;

			viableDestination = false;

			Destination = Vector2d.zero;
			hasPath = false;
			IsMoving = false;
			collisionTicked = false;
			StuckTime = 0;
			RepathTries = 0;


			Arrived = true;
			AveragePosition = Agent.Body.Position;
			DoPathfind = false;
		}

		private int StuckTime {
			get { return _stuckTime; }
				set {
				_stuckTime = value;
				if (_stuckTime == 0) {
				}
				}
		}
		private int _stuckTime;

		private int RepathTries;

		bool DoPathfind;

		protected override void OnSimulate()
		{
			if (!CanMove) {
				return;
			}
			if (IsMoving) {
				if (CanPathfind) {
					if (DoPathfind) {
						DoPathfind = false;
						if (viableDestination) {
							if (Pathfinder.GetPathNode(cachedBody._position.x, cachedBody._position.y, out currentNode)) {
								if (currentNode.DoesEqual(this.destinationNode)) {
									if (this.RepathTries >= 1) {
										this.Arrive();
									}
								} else {
									if (straightPath) {
										if (Pathfinder.NeedsPath(currentNode, destinationNode, this.GridSize)) {
											if (Pathfinder.FindPath(Destination, currentNode, destinationNode, myPath,
												GridSize)) {
												hasPath = true;
												pathIndex = 0;
											} else {
												if (IsFormationMoving) {
													StartMove(MyMovementGroup.Destination);
													IsFormationMoving = false;
												}
											}
											straightPath = false;
										} else {
										}
									} else {
										if (Pathfinder.NeedsPath(currentNode, destinationNode, this.GridSize)) {
											if (Pathfinder.FindPath(Destination, currentNode, destinationNode, myPath,
												GridSize)) {
												hasPath = true;
												pathIndex = 0;
											} else {
												if (IsFormationMoving) {
													StartMove(MyMovementGroup.Destination);
													IsFormationMoving = false;
												}
											}
										} else {
											straightPath = true;
										}
									}
								}
							} else {

							}
						} else {
							hasPath = false;
							if (IsFormationMoving) {
								StartMove(MyMovementGroup.Destination);
								IsFormationMoving = false;
							}
						}
					} else {

					}

					if (straightPath) {
						targetPos = Destination;
					} else if (hasPath) {
						if (pathIndex >= myPath.Count) {
							targetPos = this.Destination;
						} else {
							targetPos = myPath[pathIndex];
						}
					} else {
						targetPos = Destination;
					}
				} else {
					targetPos = Destination;
				}

				movementDirection = targetPos - cachedBody._position;

				movementDirection.Normalize(out distance);
				if (targetPos.x != lastTargetPos.x || targetPos.y != lastTargetPos.y) {
					lastTargetPos = targetPos;
					targetDirection = movementDirection;
				}
				bool movingToWaypoint = (this.hasPath && this.pathIndex < myPath.Count - 1);
				long stuckThreshold = 0;

				if (distance > closingDistance || movingToWaypoint) {
					desiredVelocity = (movementDirection);

					if (CanTurn)
						CachedTurn.StartTurnDirection(movementDirection);


					stuckThreshold = this.timescaledSpeed / 4;
				} else {
					if (distance < FixedMath.Mul(closingDistance, StopMultiplier)) {
						Arrive();
						return;
					}
					if (this.SlowArrival) {
						long closingSpeed = distance.Div(closingDistance);
						desiredVelocity = movementDirection * (closingSpeed);
						stuckThreshold = closingSpeed / 2;

					} else {
						desiredVelocity = (movementDirection);
						stuckThreshold = this.timescaledSpeed / 2;

					}

				}
				if (GetFullCanCollisionStop() && (Agent.Body.Position - this.AveragePosition).FastMagnitude() < (stuckThreshold * stuckThreshold)) {
					StuckTime += 1;

					if (StuckTime > StuckTimeThreshold) {
						if (RepathTries < StuckRepathTries) {
							DoPathfind = true;
							RepathTries++;

						} else {
							RepathTries = 0;
							this.Arrive();
						}
						StuckTime = 0;

					}
				} else {
					if (StuckTime > 0)
					StuckTime -= 1;

					RepathTries = 0;
				}

				if (movingToWaypoint) {
					if (distance < FixedMath.Mul(closingDistance, FixedMath.Half)) {
						this.pathIndex++;
					}
				}
				desiredVelocity *= timescaledSpeed;

				cachedBody._velocity += (desiredVelocity - cachedBody._velocity) * timescaledAcceleration;

				cachedBody.VelocityChanged = true;

				TempCanCollisionStop = true;
			} else {
				if (cachedBody.VelocityFastMagnitude > 0) {
					cachedBody._velocity -= cachedBody._velocity * timescaledAcceleration;
					cachedBody.VelocityChanged = true;
				}
				StoppedTime++;
			}
			AveragePosition = AveragePosition.Lerped(Agent.Body.Position, FixedMath.One / 4);

		}

		public Command LastCommand;

		protected override void OnExecute(Command com)
		{
			LastCommand = com;
			if (com.ContainsData<Vector2d>()) {
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

			if (onArrive.IsNotNull()) {
				onArrive();
			}
			this.OnArrive();
			Arrived = true;
		}

		protected virtual void OnArrive()
		{

		}


		public void StopMove()
		{
			if (IsMoving) {

				if (MyMovementGroup.IsNotNull()) {
					MyMovementGroup.Remove(this);
				}

				IsMoving = false;
				StoppedTime = 0;

				IsCasting = false;
				if (OnStopMove.IsNotNull()) {
					OnStopMove();
				}
			}
		}

		public void OnGroupProcessed(Vector2d destination)
		{
			Destination = destination;
			if (MoveOnGroupProcessed) {
				StartMove(destination);
				MoveOnGroupProcessed = false;
			} else {
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
			Agent.SetState(AnimState.Moving);
			hasPath = false;
			straightPath = false;
			this.Destination = destination;
			IsMoving = true;
			StoppedTime = 0;
			Arrived = false;

			viableDestination = Pathfinder.GetClosestViableNode(Agent.Body.Position, destination, this.GridSize, out destinationNode);

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

		private void HandleCollision(LSBody other)
		{

			if (!CanMove) {
				return;
			}
			if ((tempAgent = other.Agent) == null) {
				return;
			}

			Move otherMover = tempAgent.GetAbility<Move>();
			if (ReferenceEquals(otherMover, null) == false) {
				if (IsMoving && (GetFullCanCollisionStop())) {
					if (otherMover.MyMovementGroupID == MyMovementGroupID) {
						if (otherMover.IsMoving == false && otherMover.Arrived && otherMover.StoppedTime > MinimumOtherStopTime) {
							if (otherMover.CanCollisionStop == false) {
								TempCanCollisionStop = false;
							} else {
								Arrive();
							}
						} else if (hasPath && otherMover.hasPath && otherMover.pathIndex > 0 && otherMover.lastTargetPos.SqrDistance(targetPos.x, targetPos.y) < FixedMath.One) {
							if (movementDirection.Dot(targetDirection.x, targetDirection.y) < 0) {
								pathIndex++;
							}
						}
					}
				}
			}
		}

		static Vector2d desiredVelocity;

#if UNITY_EDITOR
		public bool DrawPath = true;

		void OnDrawGizmos()
		{
			if (DrawPath) {
				const float height = 0f;
				for (int i = 1; i < myPath.Count; i++) {
					UnityEditor.Handles.Label(myPath[i - 1].ToVector3(height), i.ToString());
					Gizmos.DrawLine(myPath[i - 1].ToVector3(height), myPath[i].ToVector3(height));
				}
			}
		}
#endif
	}
}