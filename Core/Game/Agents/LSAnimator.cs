using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Lockstep {
    public class LSAnimator : MonoBehaviour {
        [SerializeField]
		private string idling = "idling";

        [SerializeField]
		private string moving = "moving";

        [SerializeField]
		private string engaging = "engaging";

        [SerializeField]
		private string dying = "dying";

        [Space(10f), SerializeField]
		private string fire = "fire";

		private AnimationClip idlingClip;
		private  AnimationClip movingClip;
		private  AnimationClip engagingClip;
		private  AnimationClip dyingClip;
		private  AnimationClip fireClip;

		public event Action<AnimState> OnStatePlay;
		public event Action<AnimImpulse> OnImpulsePlay;

        private Animation animator;
		public bool CanAnimate {get; private set;}

        public void Setup() {
            animator = GetComponentInChildren<Animation>();
			if (CanAnimate = (animator .IsNotNull ()))
			{
				//States
				idlingClip = animator.GetClip (idling);
				movingClip = animator.GetClip (moving);
				engagingClip = animator.GetClip (engaging);
				dyingClip = animator.GetClip (dying);
				//Impulses
				fireClip = animator.GetClip (fire);
			}
        }


        public void Initialize() {
           // animator = GetComponentInChildren<Animator>();
            Play(AnimState.Idling);
			currentAnimState = AnimState.Idling;
			animStateChanged = true;
			isImpulsing = false;
        }

        private AnimImpulse currentImpulse;

        public void ApplyImpulse(AnimImpulse impulse) {
			Play(impulse);
		}

        public void Play(AnimState state) {
			if (OnStatePlay .IsNotNull ())
				OnStatePlay (state);
            if (CanAnimate) {
				AnimationClip clip = GetStateClip (state);
				if (clip .IsNotNull ())
				{
					animator.CrossFade (clip.name);
				}
            }
        }
		public void Play(AnimImpulse impulse) {
			if (OnImpulsePlay .IsNotNull ())
				OnImpulsePlay (impulse);
			if (CanAnimate) { 
				AnimationClip clip = GetImpulseClip (impulse);
				if (clip .IsNotNull ())
				{
					animator.CrossFade (clip.name);
				}
			}
		}

		private AnimationClip GetStateClip (AnimState state)
		{
			switch (state)
			{
			case AnimState.Moving:
				return movingClip;
			case AnimState.Idling:
				return idlingClip;
			case AnimState.Engaging:
				return engagingClip;
			case AnimState.Dying:
				return dyingClip;
			}
			return idlingClip;
		}

		public string GetStateName (AnimState state)
		{
			switch (state)
			{
			case AnimState.Moving:
				return moving;
			case AnimState.Idling:
				return idling;
			case AnimState.Engaging:
				return engaging;
			case AnimState.Dying:
				return dying;
			}
			return idling;
		}

        public string GetImpulseName(AnimImpulse impulse) {
            switch (impulse) {
            case AnimImpulse.Fire:
                return fire;

            }
            return idling;
        }
		private AnimationClip GetImpulseClip (AnimImpulse impulse)
		{
			switch (impulse) {
			case AnimImpulse.Fire:
				return fireClip;

			}
			return idlingClip;
		}
		[SerializeField]
		private AnimState currentAnimState;
		public AnimState CurrentAnimState {
			get { return currentAnimState; }
			set {
				if (value != lastAnimState) {
					animStateChanged = true;
				} else {

				}
				currentAnimState = value;
			}
		}
		
		private bool isImpulsing;
		private bool animStateChanged;
		private AnimState lastAnimState;

		public void Visualize() {
			if (isImpulsing == false) {
				if (animStateChanged) {
					Play(currentAnimState);
					animStateChanged = false;
					lastAnimState = currentAnimState;
				}
			}
		}

    }
	public enum AnimState {
		Idling,
		Moving,
		Dying,
		Engaging
	}
	
	public enum AnimImpulse {
		Fire,
		SpecialAttack
	}
}