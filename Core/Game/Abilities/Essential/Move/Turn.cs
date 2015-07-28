using UnityEngine;
using System.Collections;
using Lockstep;

public class Turn : Ability
{
	#region Static Helpers
	static Vector2d tempVec;
	static long prevDot;
	static long leDot;
	static long sqrMag;
	#endregion

	#region Settings
	public long TurnRate;
	#endregion

	LSBody Body;
	public long timescaledTurnRate;
	public Vector2d TargetRotation;
	private bool TargetReached = true;

	public override void Initialize (LSAgent agent)
	{
		Debug.Log ("asdf");
		Body = agent.Body;
		timescaledTurnRate = TurnRate * LockstepManager.Timestep >> FixedMath.SHIFT_AMOUNT;
		TargetReached = true;
		TargetRotation = Vector2d.up;
	}
	
	public override void Simulate ()
	{
		if (TargetReached == false) {

			sqrMag = TargetRotation.SqrMagnitude ();
			if (sqrMag != 0) {

				tempVec = Body.Rotation.rotatedRight ();
				prevDot = tempVec.Dot (TargetRotation.x, TargetRotation.y);
				if (prevDot != 0) {
					if (prevDot > 0) {
						Body.Rotation.Lerp (tempVec.x, tempVec.y, timescaledTurnRate);
					} else {
						Body.Rotation.Lerp (-tempVec.x, -tempVec.y, timescaledTurnRate);
					}
					tempVec = Body.Rotation.rotatedRight ();
					leDot = tempVec.Dot (TargetRotation.x, TargetRotation.y);
					if (leDot != 0) {
						if (leDot > 0) {
							if (prevDot < 0) {
								/*
								Body.VelocityMagnitude = FixedMath.Sqrt (sqrMag);
								Body.Rotation = Body.Velocity;
								Body.Rotation /= Body.VelocityMagnitude;*/
								TargetReached = true;
							}
						} else {
							if (prevDot > 0) {
								/*
								Body.VelocityMagnitude = FixedMath.Sqrt (sqrMag);
								Body.Rotation = Body.Velocity;
								Body.Rotation /= Body.VelocityMagnitude;*/
								TargetReached = true;
							}
						}
					}

					Body.Rotation.Normalize ();
					Body.RotationChanged = true;

				} else {
					if (Body.Rotation.Dot (TargetRotation.x, TargetRotation.y) < 0) {
						tempVec = Body.Rotation.rotatedRight ();
						Body.Rotation.Lerp (tempVec.x, tempVec.y, TurnRate);
						Body.Rotation.Normalize ();
						Body.RotationChanged = true;
					}
				}
			}
		}
	}

	public void StartTurn (Vector2d targetRotation)
	{
		TargetRotation = targetRotation;
		TargetReached = false;
	}

	public override void Deactivate ()
	{

	}
}
