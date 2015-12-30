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

		protected override void OnSetup ()
		{
			cachedBody = Agent.Body;

			turnSin = _turnRate.Sin;
			turnCos = _turnRate.Cos;

			cachedBody.OnContact += HandleContact;
		}

		protected override void OnInitialize ()
		{
			targetReached = true;
			targetRotation = Vector2d.up;
			contactChecked = false;
			checkCollisionCount = 0;
			contactDif = Vector2d.zero;
			cachedBeginCheck = 0;

			contacted = false;
			isContactTurning = false;

			bufferStartTurn = false;
		}

		protected override void OnSimulate ()
		{
			if (targetReached) {
                if (contactChecked) {
						contactChecked = false;
						checkCollisionCount = checkCollisionTurnRate;
						contactDif /= contactCount;
						TurnDirection (contactDif);
						isContactTurning = true;
						contactDif = Vector2d.zero;
						contactCount = 0;
					} else if (checkCollisionCount > 0) {
						checkCollisionCount--;
					}
				
			}


			if (targetReached == false) {
				if (isContactTurning) {
					if (contacted) {

						contacted = false;
					}
					else {
						targetReached = true;
						isContactTurning = false;
					}
				}
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
			isContactTurning = false;
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

		private bool isContactTurning;
		private bool contactChecked;
		private Vector2d contactDif;
		private int contactCount;
		private bool contacted;

		private void HandleContact (LSBody other)
		{
			contacted = true;
            if (targetReached == true && Agent.IsCasting == false) {
					if (other.Priority >= cachedBody.Priority) {
						if (checkCollisionCount == 0) {
							const long collisionTurnTreshold = FixedMath.One;
							if (this.cachedBody.VelocityFastMagnitude < other.VelocityFastMagnitude || other.VelocityFastMagnitude == 0) {
                                contactDif += cachedBody._position - other._position;
                                if (cachedBody._rotation.Dot (contactDif.x, contactDif.y) >= 0) {
									contactCount++;
									contactChecked = true;
								}
							}
						}

					}
				
			}
		}
	}
}