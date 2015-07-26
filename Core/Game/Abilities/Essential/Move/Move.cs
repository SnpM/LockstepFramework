using UnityEngine;
using System.Collections;
using Lockstep;

public class Move : ActiveAbility
{
	#region Behavior
	const long SteeringWeight = FixedMath.One * 3 / 4;
	const int MinimumOtherStopTime = (int)(FixedMath.One / LockstepManager.Timestep / 4);
	const int RepathRate = (int)(FixedMath.One / LockstepManager.Timestep);
	const int BadPathStopCount = 3;
	#endregion

	#region Static Movement Variables
	static Vector2d steering;
	static int i, j;
	static long sqrDistance;
	static Vector2d Offset;
	#endregion;

	#region Instance Stuff
	public long Speed;
	public bool IsMoving;
	public Vector2d Destination;
	public Vector2d TargetPos;
	public Vector2d LastTargetPos;
	public Vector2d TargetDirection;
	public MovementGroup MyMovementGroup;
	public int MyMovementGroupID = -1;
	public bool IsFormationMoving;
	private Vector2d MovementDirection;
	public LSBody Body;
	private long timescaledSpeed;
	private long distance;
	private long closingDistance;
	public long closingDistanceMultiplier;
	public int StopTime;
	private int RepathCount;
	private int BadPathCount;
	public int PathIndex;
	public FastList<Vector2d> MyPath = new FastList<Vector2d> ();
	public bool HasPath;

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
				RepathCount = RepathRate;
				if (Pathfinder.FindPath (Body.Position, Destination, MyPath)) {
					HasPath = true;	
					PathIndex = 0;
				} else {
					if (IsFormationMoving) {
						HasPath = false;
						Destination = MyMovementGroup.Destination;
						IsFormationMoving = false;
					}
					BadPathCount++;
					if (BadPathCount == BadPathStopCount)
					{
						StopMove ();
					}
				}
			} else {
				if (HasPath) {
					RepathCount--;
				} else {
					RepathCount--;
				}
			}

			if (HasPath) {
				TargetPos = MyPath [PathIndex];
			} else {
				TargetPos = Destination;
			}
		
			MovementDirection = TargetPos - Body.Position;
			MovementDirection.Normalize (out distance);
			if (TargetPos.x != LastTargetPos.x || TargetPos.y != LastTargetPos.y)
			{
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

	private void ApplySteeringForces ()
	{
		steering -= Body.Velocity;
		steering *= SteeringWeight;
		Body.Velocity += steering;
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

		if (com._position.x == Destination.x && com._position.y == Destination.y)
		{

		}
		else {
			HasPath = false;
			BadPathCount = 0;
			RepathCount -= RepathRate / 2;
		}
	}

	public void StopMove ()
	{
		IsMoving = false;
		if (MyMovementGroup != null) {
			MyMovementGroup.Remove (this);
		}
		StopTime = 0;
	}

	public void StartMove ()
	{
		IsMoving = true;
	}

	FastList<Move> TouchingObjects = new FastList<Move> ();

	private void HandleCollision (LSBody other)
	{
		if (System.Object.ReferenceEquals (other.Mover, null) == false) {
			TouchingObjects.Add (other.Mover);
			if (IsMoving) {
				if (other.Mover.MyMovementGroupID == MyMovementGroupID) {
					if (!other.Mover.IsMoving && other.Mover.StopTime > MinimumOtherStopTime)
					{
						if (IsFormationMoving) {
							if (MovementDirection.Dot (TargetDirection.x,TargetDirection.y) < 0) {
								StopMove ();
							}
						} else {
							StopMove ();
						}
					}
					else if (HasPath && other.Mover.HasPath &&
					         other.Mover.PathIndex > 0
					         && other.Mover.LastTargetPos.SqrDistance (TargetPos.x,TargetPos.y) < FixedMath.One)
					{
						if (MovementDirection.Dot (TargetDirection.x,TargetDirection.y) < 0)
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
	#endregion
}
