using UnityEngine;
using System.Collections;
using Lockstep;

public class Move : ActiveAbility
{
	#region Behavior
	const long SteeringWeight = FixedMath.One * 3/4;
	const int MinimumOtherStopTime = (int)(FixedMath.One / LockstepManager.Timestep / 4);
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
	public MovementGroup MyMovementGroup;
	public int MyMovementGroupID = -1;
	public bool IsFormationMoving;
	private Vector2d MovementDirection;
	public LSBody Body;
	private long timescaledSpeed;
	private long closingDistance;
	public long closingDistanceMultiplier;
	public int StopTime;

	public override void Initialize (LSAgent agent)
	{
		MyMovementGroupID = -1;

		Body = agent.Body;
		Body.Mover = this;
		Body.OnContact += HandleCollision;

		timescaledSpeed = ((Speed * LockstepManager.Timestep) >> FixedMath.SHIFT_AMOUNT);
		closingDistance = agent.Body.Radius;
	}

	public override void Simulate ()
	{
		if (IsMoving) {

			MovementDirection = Destination - Body.Position;
			long distance;
			MovementDirection.Normalize (out distance);

			if (distance > closingDistance) {
				Body.Velocity += (MovementDirection * timescaledSpeed - Body.Velocity) * SteeringWeight;
			} else {
				Body.Velocity += (MovementDirection * ((timescaledSpeed * distance) / (closingDistance)) - Body.Velocity) * SteeringWeight;
				if (distance < ((closingDistance * closingDistanceMultiplier) >> FixedMath.SHIFT_AMOUNT)) {
					StopMove ();
				}
			}
			
			Body.VelocityChanged = true;
		}
		else {

			if (TouchingObjects.Count > 0) Body.Velocity /= TouchingObjects.Count;

			Body.Velocity -= Body.Velocity * SteeringWeight;

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
		if (other.Mover != null) {
			TouchingObjects.Add (other.Mover);
			if (IsMoving) {
				if (!other.Mover.IsMoving &&
				    other.Mover.MyMovementGroupID == MyMovementGroupID &&
				    other.Mover.StopTime > MinimumOtherStopTime) {
					StopMove ();
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
