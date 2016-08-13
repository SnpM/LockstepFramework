using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Lockstep
{
	public class LSAnimator : LSAnimatorBase
	{
		[SerializeField]
		private string idling = "idling";

		[SerializeField]
		private string moving = "moving";

		[SerializeField]
		private string engaging = "engaging";

        [SerializeField]
        private string specialEngaging = "specialEngaging";

		[SerializeField]
		private string dying = "dying";

		[Space(10f), SerializeField]
		private string fire = "fire";

        [SerializeField]
        private string specialFire = "specialFire";

        [SerializeField]
		private string specialAttack = "specialAttack";

		[SerializeField]
		private string extra = "extra";


		private AnimationClip idlingClip;
		private  AnimationClip movingClip;
		private  AnimationClip engagingClip;
		private  AnimationClip dyingClip;
        private AnimationClip specialEngagingClip;

		private  AnimationClip fireClip;
        private AnimationClip specialFireClip;

		private AnimationClip specialAttackClip;
		private AnimationClip extraClip;

		private Animation animator;

		public override void Setup()
		{
			base.Setup();

		}

		public override void Initialize()
		{
			base.Initialize();
            animator = GetComponent<Animation>();
            if (animator == null)
                animator = this.GetComponentInChildren<Animation>();
            if (CanAnimate = (animator != null))
            {
                //States
                idlingClip = animator.GetClip(idling);
                movingClip = animator.GetClip(moving);
                engagingClip = animator.GetClip(engaging);
                dyingClip = animator.GetClip(dying);
                specialEngagingClip = animator.GetClip(this.specialEngaging);
                //Impulses
                fireClip = animator.GetClip(fire);
                specialFireClip = animator.GetClip(specialFire);
				specialAttackClip = animator.GetClip(specialAttack);
				extraClip = animator.GetClip(extra);
            }
			Play(AnimState.Idling);
		}

		public override void Play(AnimState state)
		{
			base.Play(state);
			if (CanAnimate)
			{
				AnimationClip clip = GetStateClip(state);
				if (clip.IsNotNull())
				{
					animator.CrossFade(clip.name, fadeLength);
				}
			}
		}
		const float fadeLength = .5f;
		public override void Play(AnimImpulse impulse, int rate = 0)
		{
			base.Play(impulse, rate);

			if (CanAnimate)
			{ 
				AnimationClip clip = GetImpulseClip(impulse);
				if (clip.IsNotNull())
				{
					animator.Blend(clip.name,.8f,fadeLength);
				}
			}
		}

		private AnimationClip GetStateClip(AnimState state)
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
                case AnimState.SpecialEngaging:
                    return this.specialEngagingClip;
			}
			return idlingClip;
		}

		public string GetStateName(AnimState state)
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
                case AnimState.SpecialEngaging:
                    return this.specialEngaging;
			}
			return idling;
		}

		public string GetImpulseName(AnimImpulse impulse)
		{
			switch (impulse)
			{
				case AnimImpulse.Fire:
					return fire;
                case AnimImpulse.SpecialFire:
                    return specialFire;
                case AnimImpulse.SpecialAttack:
					return specialAttack;
				case AnimImpulse.Extra:
					return extra;
			}
			return idling;
		}

		private AnimationClip GetImpulseClip(AnimImpulse impulse)
		{
			switch (impulse)
			{
				case AnimImpulse.Fire:
					return fireClip;
                case AnimImpulse.SpecialFire:
                    return specialFireClip;
                case AnimImpulse.SpecialAttack:
					return specialAttackClip;
				case AnimImpulse.Extra:
						return extraClip;
			}
			return idlingClip;
		}
	}
}