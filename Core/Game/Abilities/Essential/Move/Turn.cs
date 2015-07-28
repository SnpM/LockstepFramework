using UnityEngine;
using System.Collections;
using Lockstep;

public class Turn : Ability
{
	#region Static Helpers
	static Vector2d tempVec;
	static long sideCheck1;
	static long sideCheck2;
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
				sideCheck1 = Body.Rotation.Cross (TargetRotation.x, TargetRotation.y);
				if (sideCheck1 != 0) {
					tempVec = Body.Rotation.rotatedRight ();

					if (sideCheck1 < 0) {
						Body.Rotation.Lerp (tempVec.x, tempVec.y, timescaledTurnRate);
					} else {
						Body.Rotation.Lerp (-tempVec.x, -tempVec.y, timescaledTurnRate);
					}
					tempVec = Body.Rotation.rotatedRight ();
					sideCheck2 = Body.Rotation.Cross (TargetRotation.x, TargetRotation.y);
					if (sideCheck2 != 0) {
						if (sideCheck2 > 0) {
							if (sideCheck1 < 0) {
								Body.Rotation = TargetRotation;
								TargetReached = true;
								Body.RotationChanged = true;

								return;

							}
						} else {
							if (sideCheck1 > 0) {
								Body.Rotation = TargetRotation;
								TargetReached = true;
								Body.RotationChanged = true;
								return;
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
			else {
				TargetReached = true;
			}
		}
		else {
			if ((TargetRotation.Cross (Body.Velocity.x, Body.Velocity.y)) != 0)
			{
				StartTurn (Body.Velocity);
			}
		}
	}

	public void StartTurn (Vector2d targetRotation)
	{
		TargetRotation = targetRotation;
		TargetRotation.Normalize();
		TargetReached = false;
	}

	public override void Deactivate ()
	{

	}
}
