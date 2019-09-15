using UnityEngine;
using System;

namespace Lockstep
{
	public abstract class LSAnimatorBase : MonoBehaviour
	{

        public event Action<AnimState> onStatePlay;
		public event Action<AnimImpulse> onImpulsePlay;

		public bool CanAnimate { get; protected set; }

		private AnimImpulse currentImpulse;

		public void Setup()
		{
            OnSetup();
		}
        protected virtual void OnSetup() { }

		public void Initialize()
		{
			animStateChanged = false;
			lastAnimState = currentAnimState = AnimState.Idling;
            OnInitialize();
		}
        protected virtual void OnInitialize (){ }

		public void ApplyImpulse(AnimImpulse animImpulse, double speed = 1d)
		{
            if (onImpulsePlay.IsNotNull())
			    onImpulsePlay(animImpulse);
            OnApplyImpulse(animImpulse, speed);
		}
        protected abstract void OnApplyImpulse(AnimImpulse animImpulse, double speed);

        public void Play(AnimState state, double speed = 1d)
		{
			if (onStatePlay.IsNotNull())
				onStatePlay(state);
            OnPlay(state, speed);
		}
        protected abstract void OnPlay(AnimState impulse, double speed);

		[SerializeField]
		private AnimState currentAnimState;

		public AnimState CurrentAnimState
		{
			get { return currentAnimState; }
			set
			{
				if (value != lastAnimState)
				{
					animStateChanged = true;
				}
				else
				{

				}
				currentAnimState = value;
			}
		}

		private bool isImpulsing = false;
		private bool animStateChanged;
		private AnimState lastAnimState;


		public virtual void Visualize()
		{
			if (isImpulsing == false)
			{

				if (animStateChanged)
				{
					Play(currentAnimState);

					animStateChanged = false;
					lastAnimState = currentAnimState;
				}
			}
		}
	}

	public enum AnimState
	{
		Idling,
		Moving,
		Dying,
		Engaging,
		SpecialEngaging
	}

	public enum AnimImpulse
	{
		Fire,
		SpecialFire,
		SpecialAttack,
		Extra
	}
}