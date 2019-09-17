using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep
{
    
    public class LSMecanimAnimator : LSAnimatorBase
    {
        [SerializeField]
        protected string _idling;

        [SerializeField]
        protected string _moving;

        [SerializeField]
        protected string _engaging;

        [SerializeField]
        protected string _specialEngaging;

        [SerializeField]
        protected string _dying;

        [Space(10f), SerializeField]
        protected string _fire;

        [SerializeField]
        protected string _specialFire;

        [SerializeField]
        protected string _specialAttack;

        [SerializeField]
        protected string _extra;
        Animator animator;
        protected override void OnInitialize()
        {
            animator = GetComponentInChildren<Animator>();
            animator.Play(GetStateState(AnimState.Idling), -1, Random.Range(.5f, 1.5f));
        }
        protected override void OnPlay(AnimState impulse, double speed)
        {
            string animation = GetStateState(impulse);
            if (!string.IsNullOrEmpty(animation))
                animator.Play(animation);
        }
        protected override void OnApplyImpulse(AnimImpulse animImpulse, double speed)
        {
            string animation = GetImpulseState(animImpulse);
            if (!string.IsNullOrEmpty(animation))
            {
                animator.Play(animation);//,-1,0f);
            
            }
        }
        private string GetStateState(AnimState state)
        {
            switch (state)
            {
                case AnimState.Moving:
                    return _moving;
                case AnimState.Idling:
                    return _idling;
                case AnimState.Engaging:
                    return _engaging;
                case AnimState.Dying:
                    return _dying;
                case AnimState.SpecialEngaging:
                    return this._specialEngaging;
            }
            return _idling;
        }

        private string GetImpulseState(AnimImpulse impulse)
        {
            switch (impulse)
            {
                case AnimImpulse.Fire:
                    return _fire;
                case AnimImpulse.SpecialFire:
                    return _specialFire;
                case AnimImpulse.SpecialAttack:
                    return _specialAttack;
                case AnimImpulse.Extra:
                    return _extra;
            }
            return _idling;
        }
    }
}