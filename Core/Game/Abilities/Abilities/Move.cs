using UnityEngine;
using System.Collections;
using Lockstep;
public class Move : ActiveAbility {
	#region Static Movement Management

	#endregion;


	#region Instance Stuff
	public long Speed;
	public bool IsMoving;
	public Vector2d Destination;
	private Vector2d MovementDirection;
	private LSBody Body;
	private long timescaledSpeed;
	private long stoppingDistance;

	public override void Initialize (LSAgent agent)
	{
		Body = agent.Body;
		timescaledSpeed = ((Speed * LockstepManager.Timestep) >> FixedMath.SHIFT_AMOUNT);
		stoppingDistance = agent.Body.Radius;
	}

	public override void Simulate ()
	{
		if (IsMoving)
		{
			MovementDirection = Destination - Body.Position;
			long distance;
			MovementDirection.Normalize (out distance);

			if (distance > stoppingDistance)
			{
				Body.Velocity = MovementDirection * timescaledSpeed;
			}
			else {
				Body.Velocity = MovementDirection * ((timescaledSpeed * distance) / (stoppingDistance));
				if (distance < stoppingDistance / 4)
				{
					Body.Velocity = Vector2d.zero;
					IsMoving = false;
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
		Destination = com._position;
		InitializeMove ();
	}

	private void InitializeMove ()
	{
		IsMoving = true;
	}


	public override InputCode ListenInput {
		get {
			return InputCode.M;
		}
	}
	#endregion
}
