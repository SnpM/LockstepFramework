using UnityEngine;

namespace Lockstep
{
	public class Turn : Ability
	{

		#region Serialized
		[SerializeField, VectorRotation (true)]
		private Vector2d
		_turnRate = Vector2d.CreateRotation (FixedMath.One / 8);
		#endregion
		private bool targetReached;
		private Vector2d targetRotation;
		private long turnSin;
		private long turnCos;
		private long cachedBeginCheck;
		const int checkCollisionTurnRate = 1;//LockstepManager.FrameRate;
											 //private int checkCollisionCount;

		private long collisionTurnThreshold;

		protected override void OnSetup ()
		{
			Agent.Body = Agent.Body;

			turnSin = _turnRate.y;
			turnCos = _turnRate.x;
			collisionTurnThreshold = Agent.Body.Radius / LockstepManager.FrameRate;
			collisionTurnThreshold *= collisionTurnThreshold;
			Agent.Body.onContact += HandleContact;
		}

		protected override void OnInitialize ()
		{
			targetReached = true;
			targetRotation = Vector2d.up;
			//checkCollisionCount = 0;
			cachedBeginCheck = 0;

			bufferStartTurn = false;
		}

		protected override void OnSimulate ()
		{


			if (targetReached == false) {
				if (cachedBeginCheck != 0) {
					{
						if (cachedBeginCheck < 0) {
							Agent.Body.Rotate (turnCos, turnSin);
						} else {
							Agent.Body.Rotate (turnCos, -turnSin);
						}
					}

				} else {
					if (Agent.Body._rotation.Dot (targetRotation.x, targetRotation.y) < 0) {
						Agent.Body.Rotate (turnCos, turnSin);
					} else {
						Arrive ();
					}
				}
				Agent.Body.RotationChanged = true;
			}
		}

		protected override void OnLateSimulate ()
		{
			if (targetReached == false) {
				long check = Agent.Body._rotation.Cross (targetRotation.x, targetRotation.y);
				if (check == 0 || ((cachedBeginCheck < 0) != (check < 0))) {
					Arrive ();
				}
			}
			if (bufferStartTurn) {
				bufferStartTurn = false;
				_StartTurn (bufferTargetRot);
			}
		}

		private void Arrive ()
		{
			Agent.Body._rotation = targetRotation;
			Agent.Body.RotationChanged = true;
			targetReached = true;
		}
		public void StartTurnVector (Vector2d targetVector)
		{
			targetVector.Normalize ();
			StartTurnDirection (targetVector);
		}
		public void StartTurnDirection (Vector2d targetDirection)
		{
			bufferStartTurn = true;
			bufferTargetRot = targetDirection.ToRotation ();
		}

		private void _StartTurn (Vector2d targetRot)
		{
			long tempCheck;
			if (targetRot.NotZero () &&
			   (((tempCheck = Agent.Body._rotation.Cross (targetRot.x, targetRot.y)) != 0) ||
				(Agent.Body._rotation.Dot (targetRot.x, targetRot.y) < 0))
			   ) {
				if (tempCheck.AbsLessThan (turnSin) && Agent.Body._rotation.Dot (targetRot.x, targetRot.y) > 0) {
					targetRotation = targetRot;
					Arrive ();
				} else {
					cachedBeginCheck = tempCheck;
					targetRotation = targetRot;
					targetReached = false;
				}
			} else {

			}
		}
		bool bufferStartTurn;
		Vector2d bufferTargetRot;

		public void StopTurn ()
		{
			targetReached = true;
		}

		protected override void OnStopCast ()
		{
			StopTurn ();
		}


		private void HandleContact (LSBody other)
		{
			if (targetReached == true && Agent.IsCasting == false && !(Agent.Body.Immovable || Agent.Body.IsTrigger)) {
				Vector2d delta = this.Agent.Body._position - this.Agent.Body.LastPosition;
				if (delta.FastMagnitude () > collisionTurnThreshold) {
					delta.Normalize ();
					this.StartTurnDirection (delta);
				}
			}
		}
	}
}