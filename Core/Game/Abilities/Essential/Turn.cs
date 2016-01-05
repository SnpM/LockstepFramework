using UnityEngine;

namespace Lockstep
{
	public class Turn : Ability
	{

		#region Serialized
		[SerializeField]
		private VectorRotation
			_turnRate;
		#endregion
		private LSBody cachedBody;
		private bool targetReached;
		private Vector2d targetRotation;
		private long turnSin;
		private long turnCos;
		private long cachedBeginCheck;
		const int checkCollisionTurnRate = 1;//LockstepManager.FrameRate;
		private int checkCollisionCount;

        private long collisionTurnThreshold;

		protected override void OnSetup ()
		{
			cachedBody = Agent.Body;

			turnSin = _turnRate.Sin;
			turnCos = _turnRate.Cos;
            collisionTurnThreshold = cachedBody.Radius / LockstepManager.FrameRate;
            collisionTurnThreshold *= collisionTurnThreshold;
			cachedBody.OnContact += HandleContact;
		}

		protected override void OnInitialize ()
		{
			targetReached = true;
			targetRotation = Vector2d.up;
            checkCollisionCount = 0;
            cachedBeginCheck = 0;

			bufferStartTurn = false;
		}

		protected override void OnSimulate ()
		{


			if (targetReached == false) {

				if (cachedBeginCheck != 0) {
					{
						if (cachedBeginCheck < 0) {
							cachedBody.Rotate (turnCos, turnSin);
						} else {
							cachedBody.Rotate (turnCos, -turnSin);
						}
					}

				} else {
					if (cachedBody._rotation.Dot (targetRotation.x, targetRotation.y) < 0) {
						cachedBody.Rotate (turnCos, turnSin);
					} else {
						Arrive ();
					}
				}
				cachedBody.RotationChanged = true;
			}
		}

		protected override void OnLateSimulate ()
		{
			if (targetReached == false) {
				long check = cachedBody._rotation.Cross (targetRotation.x, targetRotation.y);
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
			cachedBody._rotation = targetRotation;
            cachedBody.RotationChanged = true;
			targetReached = true;
		}

		public void TurnDirection (Vector2d targetDirection)
		{
			targetDirection.Normalize ();
			StartTurn (targetDirection);
		}

		public void StartTurn (Vector2d targetRot)
		{
            bufferStartTurn = true;
			bufferTargetRot = targetRot;
        }
		public void StartTurnRaw (Vector2d targetRot) {
			targetRotation = targetRot;
			targetReached = false;
			cachedBeginCheck = cachedBody._rotation.Cross (targetRot.x,targetRot.y);
		}
		private void _StartTurn (Vector2d targetRot) {
			long tempCheck;
			if (targetRot.NotZero () && (tempCheck = cachedBody._rotation.Cross (targetRot.x, targetRot.y)) != 0) {
				if (tempCheck.AbsMoreThan (turnSin) == false && cachedBody._rotation.Dot (targetRot.x,targetRot.y) > 0)
				{
					targetRotation = targetRot;
                    Arrive ();
				}
				else {
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
            if (targetReached == true && Agent.IsCasting == false) {
                Vector2d delta = this.cachedBody._position - this.cachedBody.LastPosition;
                if (delta.FastMagnitude() > collisionTurnThreshold) {
                    delta.Normalize();
                    this.StartTurn(delta);
                }
			}
		}
	}
}