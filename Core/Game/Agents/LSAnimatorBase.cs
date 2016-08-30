using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Lockstep
{
	public class LSAnimatorBase : MonoBehaviour
	{
		public event Action<AnimState> OnStatePlay;
		public event Action<AnimImpulse, int> OnImpulsePlay;

		public bool CanAnimate { get; protected set; }

		private AnimImpulse currentImpulse;

		public virtual void Setup()
		{
		}

		public virtual void Initialize()
		{
			animStateChanged = false;
			lastAnimState = currentAnimState = AnimState.Idling;
		}

		public virtual void ApplyImpulse(AnimImpulse animImpulse, int rate = 0)
		{
			Play (animImpulse, rate);
		}
			
		public virtual void Play(AnimState state)
		{
			if (OnStatePlay.IsNotNull())
				OnStatePlay(state);
		}

		public virtual void Play(AnimImpulse impulse, int rate = 0)
		{
			if (OnImpulsePlay.IsNotNull())
				OnImpulsePlay(impulse, rate);
		}

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
				} else
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