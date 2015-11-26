using System;
using UnityEngine;
namespace Lockstep {
    public class Move : ActiveAbility
    {
        public const long FormationStop = FixedMath.One / 8;
		public const long GroupDirectStop = FixedMath.One / 2;
        public const long DirectStop = FixedMath.One / 8;
        private const int MinimumOtherStopTime = (int)(LockstepManager.FrameRate / 4);
        private const int repathRate = (int)LockstepManager.FrameRate * 3 / 4;
		private const int CollisionStopCount = LockstepManager.FrameRate * 2;
		private const long CollisionStopTreshold = FixedMath.One / 2;
        
        public Vector2d Position { get { return cachedBody.Position; } }
        public long CollisionSize { get { return cachedBody.Radius; } }

        public MovementGroup MyMovementGroup { get; set; }

        private int RepathRate {
            get { return LSUtility.GetRandom(repathRate); }
        }
        private const int straightRepathRate = repathRate * 4;
        private int StraightRepathRate {
            get { return LSUtility.GetRandom(straightRepathRate); }
        }

        public int MyMovementGroupID { get; set; }
        public bool IsFormationMoving { get; set; }

        public bool IsMoving { get; private set; }
		private const bool canPathfind = false;
        private bool hasPath;
        private bool straightPath;
        private bool viableDestination;
        private readonly FastList<Vector2d> myPath = new FastList<Vector2d>();
        private int pathIndex;
        private int stopTime;
        private Vector2d targetPos;
		[HideInInspector]
        public Vector2d Destination;
        private int repathCount;
        public bool CanCollisionStop { get; set; }
        public long CollisionStopMultiplier { get; set; }

        private bool forcePathfind { get; set; }
        private Vector2d lastPosition;
        private bool collisionTicked;
        private int stuckTick;

        public bool Arrived { get; private set; }

        public Action OnArrive;

        private LSBody cachedBody;
		private Turn cachedTurn;
        private long timescaledSpeed {
            get {
                return ((this.Speed / LockstepManager.FrameRate) + this.AdditiveSpeedModifier).Mul(FixedMath.One + this.MultiplicativeSpeedModifier);
            }
        }
		private long collisionStopTreshold;
		private long timescaledAcceleration;
        public long AdditiveSpeedModifier {get; set;}
        public long MultiplicativeSpeedModifier {get; set;}

        private Vector2d lastTargetPos;
        private Vector2d targetDirection;
        private GridNode currentNode;
        private GridNode destinationNode;
        private Vector2d movementDirection;
		private Vector2d lastMovementDirection;
        private long distance;
        private long closingDistance;
        private long stuckTolerance;

		#region Serialized
		[SerializeField]
		private bool CanMove = true;
		[SerializeField, FixedNumber]
		private long Speed = FixedMath.One * 4;
		[SerializeField, FixedNumber]
		private long Acceleration = FixedMath.One;
		[SerializeField]
		private bool Flying;
		#endregion

        protected override void OnSetup() {
            cachedBody = Agent.Body;
            cachedBody.OnContact += HandleCollision;
			cachedTurn = Agent.Turner;
			collisionStopTreshold = FixedMath.Mul (timescaledSpeed,CollisionStopTreshold);
			collisionStopTreshold *= collisionStopTreshold;
			timescaledAcceleration = Acceleration * 32 / LockstepManager.FrameRate;
			if (timescaledAcceleration > FixedMath.One) timescaledAcceleration = FixedMath.One;
			closingDistance = cachedBody.Radius;
			if (closingDistance < FixedMath.One / 4) closingDistance = closingDistance;
            stuckTolerance = ((Agent.Body.Radius * Speed) >> FixedMath.SHIFT_AMOUNT) / LockstepManager.FrameRate;
            stuckTolerance *= stuckTolerance;
        }

        protected override void OnInitialize() {
            myPath.FastClear();
            pathIndex = 0;
            stopTime = 0;

            IsFormationMoving = false;
            MyMovementGroupID = -1;
            CanCollisionStop = true;
            CollisionStopMultiplier = DirectStop;

            repathCount = RepathRate;
            viableDestination = false;

            Destination = Vector2d.zero;
            hasPath = false;
            IsMoving = false;
            collisionTicked = false;
            stuckTick = 0;

            forcePathfind = false;
            lastPosition = Vector2d.zero;
			lastMovementDirection = Vector2d.up;

            Arrived = true;
        }

        protected override void OnSimulate() {
            if (!CanMove) {
                return;
            }
            if (IsMoving) {
                if (canPathfind) {
                    if (repathCount <= 0) {
                        if (viableDestination) {
                            if (Pathfinder.GetPathNode(cachedBody.Position.x, cachedBody.Position.y, out currentNode)) {
                                if (straightPath) {
                                    if (forcePathfind || Pathfinder.NeedsPath(currentNode, destinationNode)) {
                                        if (Pathfinder.FindPath(Destination, currentNode, destinationNode, myPath)) {
                                            hasPath = true;
                                            pathIndex = 0;
                                        } else {
                                            if (IsFormationMoving) {
                                                StartMove(MyMovementGroup.Destination);
                                                IsFormationMoving = false;
                                            }
                                        }
                                        straightPath = false;
                                        repathCount = RepathRate;
                                    } else {
                                        repathCount = StraightRepathRate;
                                    }
                                } else {
                                    if (forcePathfind || Pathfinder.NeedsPath(currentNode, destinationNode)) {
                                        if (Pathfinder.FindPath(Destination, currentNode, destinationNode, myPath)) {
                                            hasPath = true;
                                            pathIndex = 0;
                                        } else {
                                            if (IsFormationMoving) {
                                                StartMove(MyMovementGroup.Destination);
                                                IsFormationMoving = false;
                                            }
                                        }
                                        repathCount = RepathRate;
                                    } else {
                                        straightPath = true;
                                        repathCount = StraightRepathRate;
                                    }
                                }
                            } else {}
                        } else {
                            hasPath = false;
                            if (IsFormationMoving) {
                                StartMove(MyMovementGroup.Destination);
                                IsFormationMoving = false;
                            }
                        }
                    } else {
                        if (hasPath) {
                            repathCount--;
                        } else {
                            repathCount--;
                        }
                    }

                    if (straightPath) {
                        targetPos = Destination;
                    } else if (hasPath) {
                        if (pathIndex >= myPath.Count) {
                            pathIndex = myPath.Count - 1;
                        }
                        targetPos = myPath[pathIndex];
                    } else {
                        targetPos = Destination;
                    }
                } else {
                    targetPos = Destination;
                }

                movementDirection = targetPos - cachedBody.Position;
                movementDirection.Normalize(out distance);
                if (targetPos.x != lastTargetPos.x || targetPos.y != lastTargetPos.y) {
                    lastTargetPos = targetPos;
                    targetDirection = movementDirection;
                }

                if (distance > closingDistance) {
					desiredVelocity = (movementDirection);
					if (movementDirection.Cross (lastMovementDirection.x, lastMovementDirection.y) != 0)
					{
						lastMovementDirection = movementDirection;
						cachedTurn.StartTurnRaw (movementDirection);
					}
                } else {
                    if (distance < FixedMath.Mul (closingDistance, CollisionStopMultiplier)) {
                        Arrive();
						return;
                    }
					desiredVelocity = (movementDirection * (distance) / (closingDistance));
                }

				desiredVelocity *= timescaledSpeed;
                
				cachedBody._velocity += (desiredVelocity - cachedBody._velocity) * timescaledAcceleration;
				if (distance <= closingDistance) {
					pathIndex++;
				}
				cachedBody.VelocityChanged = true;
				if (collidedWithTrackedAgent) {
					if (collidedCount >= CollisionStopCount) {
						collidedCount = 0;
						collidingAgent = null;
						Arrive ();
					}
					else {
						if (lastPosition.FastDistance (cachedBody.Position.x, cachedBody.Position.y)
						    < collisionStopTreshold) {
							collidedCount++;
						}
						else {
							lastPosition = cachedBody.Position;
							collidedCount = 0;
						}
					}
					collidedWithTrackedAgent = false;
				}
				else {
					collidingAgent = null;
					collidedCount = 0;
				}
            } else {
                if (cachedBody.VelocityFastMagnitude > 0) {
					cachedBody._velocity -= cachedBody._velocity * timescaledAcceleration;
					cachedBody.VelocityChanged = true;
                }
                stopTime++;
            }
        }

        protected override void OnExecute(Command com) {
            if (com.HasPosition) {
                Agent.StopCast(ID);
                RegisterGroup ();
             }
        }
		public void RegisterGroup (bool moveOnProcessed = true) {
            MoveOnGroupProcessed = moveOnProcessed;
			MovementGroupHandler.LastCreatedGroup.Add(this);
			
			if (straightPath) {
				repathCount /= 8;
			} else {
				repathCount /= 4;
			}
		}

        public void Arrive() {
            if (OnArrive .IsNotNull ()) {
                OnArrive();
            }
            Arrived = true;
            StopMove();
        }

        public event Action OnStopMove;

        public void StopMove() {
            if (IsMoving) {
                forcePathfind = false;

                if (MyMovementGroup .IsNotNull ()) {
                    MyMovementGroup.Remove(this);
                }

                IsMoving = false;
                stopTime = 0;

                IsCasting = false;
                stuckTick = 0;
                if (OnStopMove .IsNotNull ()) {
                    OnStopMove();
                }
            }
        }

        public void OnGroupProcessed (Vector2d destination) {
            Destination = destination;
            if (MoveOnGroupProcessed) {
                StartMove (destination);
                MoveOnGroupProcessed = false;
            }
            else {
                this.Destination = destination;
            }
            if (this.onGroupProcessed != null)
                this.onGroupProcessed();
        }
        public event Action onGroupProcessed;

        public bool MoveOnGroupProcessed {get; private set;}
        public void StartMove(Vector2d destination) {
            if (false && IsMoving == true && destination.x == this.Destination.x && destination.y == this.Destination.y) {
                //TODO: guard return
            } else {
				cachedTurn.TurnDirection (destination - cachedBody.Position);
                Agent.SetState(AnimState.Moving);
                hasPath = false;
                straightPath = false;
                this.Destination = destination;
                IsMoving = true;
                stopTime = 0;
                Arrived = false;

                viableDestination = Pathfinder.GetPathNode(this.Destination.x, this.Destination.y, out destinationNode);

                IsCasting = true;
                stuckTick = 0;
                forcePathfind = false;
            }
        }

        protected override void OnStopCast() {
            StopMove();
        }


		private int collidedCount;
		private ushort collidedID;
		private LSAgent collidingAgent;
		private bool collidedWithTrackedAgent;
		static LSAgent tempAgent;
        private void HandleCollision(LSBody other) {
            if (!CanMove) {
                return;
            }
            if ((tempAgent = other.Agent) == null) {
                return;
            }
			if (tempAgent == collidingAgent) collidedWithTrackedAgent = true;
			else if (collidingAgent == null) {
				collidingAgent = tempAgent;
				collidedWithTrackedAgent = true;
			}
            Move otherMover = tempAgent.Mover;
            if (ReferenceEquals(otherMover, null) == false) {
                if (IsMoving && CanCollisionStop) {
                    if (otherMover.MyMovementGroupID == MyMovementGroupID) {
                        if (otherMover.IsMoving == false && otherMover.Arrived && otherMover.stopTime > MinimumOtherStopTime) {
                            Arrive();
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
    }
}