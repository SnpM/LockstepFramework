using System;
using Lockstep.UI;
using UnityEngine;

namespace Lockstep
{
    public class Health : Ability
    {
        [SerializeField, FixedNumber]
        private long _maxHealth = FixedMath.One * 100;

        public long BaseHealth { get { return _maxHealth; } }

        public long MaxHealth
        {
            get { return _maxHealth + MaxHealthModifier; }
        }

        private long _maxHealthModifier;

        [Lockstep(true)]
        public long MaxHealthModifier
        {
            get
            {
                return _maxHealthModifier;
            }
            set
            {
                if (value != _maxHealthModifier)
                {
                    long dif = _maxHealthModifier - value;
                    if (dif > 0)
                    {
                        this.TakeDamage(-dif);
                        _maxHealthModifier = value;
                    }
                }
            }
        }

        public long DamageMultiplier
        {
            get;
            set;
        }

        public event Action onHealthChange;
        public event Action<long> onHealthDelta;

        public bool CanLose
        {
            get
            {
                return HealthAmount > 0;
            }
        }

        public bool CanGain
        {
            get
            {
                return HealthAmount < MaxHealth;
            }
        }

        [SerializeField, FixedNumber]
        private long _currentHealth;

        public long HealthAmount
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                long delta = value - _currentHealth;
                _currentHealth = value;
                if (onHealthChange != null)
                    onHealthChange();
                if (onHealthDelta != null)
                    onHealthDelta(delta);
            }

        }

        public LSAgent LastDamageSource { get; set; }

        protected override void OnSetup()
        {
        }

        protected override void OnInitialize()
        {
            HealthAmount = MaxHealth;
            OnTakeProjectile = null;
            MaxHealthModifier = 0;
            LastDamageSource = null;
        }

        public void TakeProjectile(LSProjectile projectile)
        {
            if (Agent.IsActive && HealthAmount >= 0)
            {
                if (OnTakeProjectile.IsNotNull())
                {
                    OnTakeProjectile(projectile);
                }
                TakeDamage(projectile.CheckExclusiveDamage(Agent.Tag));               
            }
        }

        public void TakeDamage(long damage, LSAgent source = null)
        {
            if (damage >= 0)
            {
                damage.Mul(DamageMultiplier);
                HealthAmount -= damage;
                if (source != null)
                    LastDamageSource = source;

                // don't let the health go below zero
                if (HealthAmount <= 0)
                {
                    HealthAmount = 0;
				
                    if (HealthAmount <= 0)
                    {
                        Die();
                        return;
                    }

                }
            } else
            {
                HealthAmount -= damage;
                if (HealthAmount >= this.MaxHealth)
                {
                    HealthAmount = MaxHealth;
                }
            }
           
        }

        public event Action<Health,LSAgent> onDie;

        public void Die()
        {
            if (Agent.IsActive)
            {
                if (onDie != null)
                    this.onDie(this,this.LastDamageSource);
                AgentController.DestroyAgent(Agent);
                if (Agent.Animator.IsNotNull())
                {
                    Agent.SetState(AnimState.Dying);
                    Agent.Animator.Visualize();
                }
            }
        }

        protected override void OnDeactivate()
        {
            OnTakeProjectile = null;
        }

        public event Action<LSProjectile> OnTakeProjectile;

        public int shieldIndex { get; set; }


    }
}