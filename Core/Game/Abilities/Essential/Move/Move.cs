using UnityEngine;
using System.Collections;
using Lockstep;

public class Move : ActiveAbility
{
	#region Static Movement Management

	#endregion;


	#region Instance Stuff
	public long Speed;
	public bool IsMoving;
	public Vector2d Destination;
	public MovementGroup MyMovementGroup;
	public int MyMovementGroupID = -1;
	private Vector2d MovementDirection;
	public LSBody Body;
	private long timescaledSpeed;
	private long stoppingDistance;

	public override void Initialize (LSAgent agent)
	{
		MyMovementGroupID = -1;

		Body = agent.Body;
		Body.Mover = this;
		Body.OnContact += HandleCollisionEnter;
		Body.OnContact += HandleCollision;
		Body.OnContactExit += HandleCollisionExit;

		timescaledSpeed = ((Speed * LockstepManager.Timestep) >> FixedMath.SHIFT_AMOUNT);
		stoppingDistance = agent.Body.Radius;
	}

	public override void Simulate ()
	{
		if (IsMoving) {
			MovementDirection = Destination - Body.Position;
			long distance;
			MovementDirection.Normalize (out distance);

			if (distance > stoppingDistance) {
				Body.Velocity = MovementDirection * timescaledSpeed;
			} else {
				Body.Velocity = MovementDirection * ((timescaledSpeed * distance) / (stoppingDistance));
				if (distance < stoppingDistance / 4) {
					StopMove ();
				}
			}
		
			Body.VelocityChanged = true;
		}
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
		Body.Velocity = Vector2d.zero;
	}

	public void StartMove ()
	{
		IsMoving = true;
	}

	FastBucket<Move> TouchingObjects = new FastBucket<Move> ();

	private void HandleCollisionEnter (LSBody other)
	{
		if (other.Mover != null) {
			TouchingObjects.Add (other.Mover);
		}
	}

	private void HandleCollision (LSBody other)
	{
		if (other.Mover != null) {
			if (IsMoving) {
				if (!other.Mover.IsMoving && other.Mover.MyMovementGroupID == MyMovementGroupID) {
					StopMove ();
				}
			}
		}
	}

	private void HandleCollisionExit (LSBody other)
	{
		if (other.Mover != null) {
			TouchingObjects.Remove (other.Mover);
			
		}
	}

	public override InputCode ListenInput {
		get {
			return InputCode.M;
		}
	}
	#endregion
}
