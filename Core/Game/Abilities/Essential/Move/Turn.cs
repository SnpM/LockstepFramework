using UnityEngine;
using System.Collections;
using Lockstep;

public class Turn : Ability
{
	#region Static Helpers
	static Vector2d tempVec;
	static long prevDot;
	static long leDot;
	#endregion

	#region Settings
	public long TurnRate;
	#endregion

	LSBody Body;
	public long timescaledTurnRate;
	private Vector2d lastVelocity;
	private bool TargetReached;

	public override void Initialize (LSAgent agent)
	{
		Body = agent.Body;
		timescaledTurnRate = TurnRate * LockstepManager.Timestep >> FixedMath.SHIFT_AMOUNT;
		TargetReached = false;
	}
	
	public override void Simulate ()
	{
		if (TargetReached == false)
		{
			if (Body.VelocityMagnitude != 0) {
				tempVec = Body.Rotation;
				tempVec.RotateRight ();
				prevDot = tempVec.Dot (Body.Velocity.x, Body.Velocity.y);
				if (prevDot != 0) {
					if (prevDot > 0) {
						Body.Rotation.Lerp (tempVec.x, tempVec.y, timescaledTurnRate);
					} else {
						Body.Rotation.Lerp (-tempVec.x, -tempVec.y, timescaledTurnRate);
					}
					tempVec = Body.Rotation;
					tempVec.RotateRight ();
					leDot = tempVec.Dot (Body.Velocity.x, Body.Velocity.y);
					if (leDot != 0) {
						if (leDot > 0) {
							if (prevDot < 0) {
								Body.Rotation = Body.Velocity;
							}
						} else {
							if (prevDot > 0) {
								Body.Rotation = Body.Velocity;
							}
						}
					}
					Body.Rotation.Normalize ();
					Body.RotationChanged = true;

				} else {
					if (Body.Rotation.Dot (Body.Velocity.x, Body.Velocity.y) < 0) {
						tempVec = Body.Rotation;
						tempVec.RotateRight ();
						Body.Rotation.Lerp (tempVec.x, tempVec.y, TurnRate);
						Body.Rotation.Normalize();
						Body.RotationChanged = true;
					}
				}
			}
			else if (lastVelocity.x != Body.Velocity.x || lastVelocity.y != Body.Velocity.y)
			{
				lastVelocity = Body.Velocity;
				TargetReached = false;
			}
		}
	}

	public override void Deactivate ()
	{

	}
}
