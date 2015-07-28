//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using System.Collections;
using Lockstep;

public class Move : ActiveAbility
{
	#region Behavior
	const long SteeringWeight = FixedMath.One * 3 / 4;
	const int MinimumOtherStopTime = (int)(FixedMath.One / LockstepManager.Timestep / 4);
	const int RepathRate = (int)(FixedMath.One / LockstepManager.Timestep) * 3 / 4;
	const int StraightRepathRate = RepathRate * 4;
	#endregion

	#region Static Movement Variables
	static int i, j;
	static long sqrDistance;
	static Vector2d Offset;
	static long prevDot;
	static long leDot;
	static Vector2d tempVec;
	#endregion;

	#region Defined Variables
	public long Speed;
	#endregion

	public bool IsMoving;
	public Vector2d Destination;
	public Vector2d TargetPos;
	public Vector2d LastTargetPos;
	public Vector2d TargetDirection;
	public GridNode CurrentNode;
	public GridNode DestinationNode;
	public MovementGroup MyMovementGroup;
	public int MyMovementGroupID = -1;
	public bool IsFormationMoving;
	private Vector2d MovementDirection;
	public LSBody Body;
	private long timescaledSpeed;
	private long timescaledTurnRate;
	private long distance;
	private long closingDistance;
	public long closingDistanceMultiplier;
	public int StopTime;
	private int RepathCount;
	private int BadPathCount;
	public int PathIndex;
	public FastList<Vector2d> MyPath = new FastList<Vector2d> ();
	public bool HasPath;
	public bool StraightPath;
	private bool ViablePath;

	public override void Initialize (LSAgent agent)
	{
		MyMovementGroupID = -1;

		Body = agent.Body;
		Body.Mover = this;
		Body.OnContact += HandleCollision;

		timescaledSpeed = ((Speed * LockstepManager.Timestep) >> FixedMath.SHIFT_AMOUNT);
		closingDistance = agent.Body.Radius;

		RepathCount = LSUtility.GetRandom (RepathRate);
	}

	public override void Simulate ()
	{
		if (IsMoving) {
			if (RepathCount <= 0) {
				Pathfinder.GetPathNode (Body.Position.x, Body.Position.y, out CurrentNode);
				ViablePath =
					System.Object.ReferenceEquals (CurrentNode, null) == false &&
					System.Object.ReferenceEquals (DestinationNode, null) == false;
				if (ViablePath) {
					if (StraightPath) {
						if (Pathfinder.NeedsPath (CurrentNode, DestinationNode)) {
							if (Pathfinder.FindPath (Destination, CurrentNode, DestinationNode, MyPath)) {
								HasPath = true;
								PathIndex = 0;
							}
							StraightPath = false;
							RepathCount = RepathRate;
						} else {
							RepathCount = StraightRepathRate;
						}
					} else {
						if (Pathfinder.NeedsPath (CurrentNode, DestinationNode)) {
							if (Pathfinder.FindPath (Destination, CurrentNode, DestinationNode, MyPath)) {
								HasPath = true;	
								PathIndex = 0;
							} else {
								if (IsFormationMoving) {
									StartMove (MyMovementGroup.Destination);
									IsFormationMoving = false;
								} else {

								}
							}
							RepathCount = RepathRate;
						} else {
							StraightPath = true;
							RepathCount = StraightRepathRate;
						}
					}
				} else {
					HasPath = false;
					if (IsFormationMoving) {
						StartMove (MyMovementGroup.Destination);
						IsFormationMoving = false;
					}
				}
			} else {

				if (HasPath) {
					RepathCount--;
					
				} else {
					RepathCount--;
				}
			}

			if (StraightPath) {
				TargetPos = Destination;
			} else if (HasPath) {
				TargetPos = MyPath [PathIndex];
			} else {
				TargetPos = Destination;
			}
		
			MovementDirection = TargetPos - Body.Position;
			MovementDirection.Normalize (out distance);
			if (TargetPos.x != LastTargetPos.x || TargetPos.y != LastTargetPos.y) {
				LastTargetPos = TargetPos;
				TargetDirection = MovementDirection;
			}
			if (PathIndex + 1 >= MyPath.Count) {
				if (distance > closingDistance) {
					Body.Velocity += (MovementDirection * timescaledSpeed - Body.Velocity) * SteeringWeight;
				} else {
					Body.Velocity += (MovementDirection * ((timescaledSpeed * distance) / (closingDistance)) - Body.Velocity) * SteeringWeight;
					if (distance < ((closingDistance * closingDistanceMultiplier) >> FixedMath.SHIFT_AMOUNT)) {
						StopMove ();
					}
				}
			} else {
				Body.Velocity += (MovementDirection * timescaledSpeed - Body.Velocity) * SteeringWeight;
				if (distance <= closingDistance) {
					PathIndex++;
				}
			}
			
			Body.VelocityChanged = true;
		} else {
			if (Body.VelocityMagnitude > 0) {
				Body.Velocity -= Body.Velocity * SteeringWeight;
				Body.VelocityChanged = true;
			}
			StopTime++;
		}
		TouchingObjects.FastClear ();
	}





	public override void Deactivate ()
	{

	}

	public override void Execute (Command com)
	{
		if (!com.Used) {
			MovementGroup.CreateGroup (com);
			com.Used = true;
		}

		MovementGroup.LastCreatedGroup.Add (this);


	}

	public void StopMove ()
	{
		IsMoving = false;
		if (MyMovementGroup != null) {
			MyMovementGroup.Remove (this);
		}
		StopTime = 0;
	}

	public void StartMove (Vector2d destination)
	{
		if (destination.x == Destination.x && destination.y == Destination.y) {
			
		} else {
			if (StraightPath)
				RepathCount /= 4;
			else
				RepathCount /= 2;

			HasPath = false;
			BadPathCount = 0;
			StraightPath = false;
			Destination = destination;
			IsMoving = true;

			Pathfinder.GetPathNode (Destination.x, Destination.y, out DestinationNode);
		}

	}

	FastList<Move> TouchingObjects = new FastList<Move> ();

	private void HandleCollision (LSBody other)
	{
		if (System.Object.ReferenceEquals (other.Mover, null) == false) {
			TouchingObjects.Add (other.Mover);
			if (IsMoving) {
				if (other.Mover.MyMovementGroupID == MyMovementGroupID) {
					if (!other.Mover.IsMoving && other.Mover.StopTime > MinimumOtherStopTime) {
						if (IsFormationMoving) {
							if (MovementDirection.Dot (TargetDirection.x, TargetDirection.y) < 0) {
								StopMove ();
							}
						} else {
							StopMove ();
						}
					} else if (HasPath && other.Mover.HasPath &&
						other.Mover.PathIndex > 0
						&& other.Mover.LastTargetPos.SqrDistance (TargetPos.x, TargetPos.y) < FixedMath.One) {
						if (MovementDirection.Dot (TargetDirection.x, TargetDirection.y) < 0)
							PathIndex++;
					}
				}
			}
		}
	}

	public override InputCode ListenInput {
		get {
			return InputCode.M;
		}
	}
}
