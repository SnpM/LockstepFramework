using UnityEngine;

namespace Lockstep
{
	public class LSAnimator : LSAnimatorBase
	{
        //Sphagetti :D
        [SerializeField]
        protected string _idling = "idling";

        [SerializeField]
        protected string _moving = "moving";

        [SerializeField]
        protected string _engaging = "engaging";

        [SerializeField]
        protected string _specialEngaging = "specialEngaging";

        [SerializeField]
        protected string _dying = "dying";

        [Space(10f), SerializeField]
        protected string _fire = "fire";

        [SerializeField]
        protected string _specialFire = "specialFire";

        [SerializeField]
        protected string _specialAttack = "specialAttack";

        [SerializeField]
        protected string _extra = "extra";


        private AnimationClip idlingClip;
		private AnimationClip movingClip;
		private AnimationClip engagingClip;
		private AnimationClip dyingClip;
		private AnimationClip specialEngagingClip;

		private AnimationClip fireClip;
		private AnimationClip specialFireClip;

		private AnimationClip specialAttackClip;
		private AnimationClip extraClip;

		private Animation animator;

		protected override void OnSetup()
        { 
		}

        protected override void OnInitialize()
		{
			animator = GetComponent<Animation>();
			if (animator == null)
				animator = this.GetComponentInChildren<Animation>();
			if (CanAnimate = (animator != null))
			{
				//States
				idlingClip = animator.GetClip(_idling);
				movingClip = animator.GetClip(_moving);
				engagingClip = animator.GetClip(_engaging);
				dyingClip = animator.GetClip(_dying);
				specialEngagingClip = animator.GetClip(_specialEngaging);
				//Impulses
				fireClip = animator.GetClip(_fire);
				specialFireClip = animator.GetClip(_specialFire);
				specialAttackClip = animator.GetClip(_specialAttack);
				extraClip = animator.GetClip(_extra);
			}
			Play(AnimState.Idling);
		}

		const float fadeLength = .5f;

        protected override void OnApplyImpulse(AnimImpulse animImpulse, double speed)
        {
            if (CanAnimate)
            {
                AnimationClip clip = GetImpulseClip(animImpulse);
                if (clip.IsNotNull())
                {
                    animator.Play(clip.name, PlayMode.StopSameLayer);
                }
            }
        }
        protected override void OnPlay(AnimState state, double speed)
		{
			if (CanAnimate)
			{
				AnimationClip clip = GetStateClip(state);
				if (clip.IsNotNull())
				{
					//animator.Blend(clip.name,.8f,fadeLength);
					animator.CrossFade(clip.name, fadeLength);
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