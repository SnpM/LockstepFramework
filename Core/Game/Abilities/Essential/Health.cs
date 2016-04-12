using System;
using Lockstep.UI;
using UnityEngine;

namespace Lockstep
{
	public class Health : Ability
	{
		[SerializeField, FixedNumber]
		private long _maxHealth = FixedMath.One * 100;

		public long MaxHealth
		{
			get { return _maxHealth; }
			set { _maxHealth = value; }
		}

        public event Action onHealthChange;
        [SerializeField, FixedNumber]
        private long _currentHealth;
		public long HealthAmount {
            get {
                return _currentHealth;
            }
            set {
                _currentHealth = value;
                if (onHealthChange != null)
                onHealthChange ();
            }

        }

		protected override void OnSetup()
		{
		}

		protected override void OnInitialize()
		{
			HealthAmount = MaxHealth;
			OnTakeProjectile = null;
		}

		public void TakeProjectile(LSProjectile projectile)
		{
			if (Agent.IsActive && HealthAmount >= 0) {
				if (OnTakeProjectile .IsNotNull ()) 
				{
					OnTakeProjectile (projectile);
				}
				TakeRawDamage (projectile.CheckExclusiveDamage (Agent.Tag));               
			}
		}

		public void TakeRawDamage (long damage) {
			HealthAmount -= damage;
			// don't let the health go below zero
			if (HealthAmount <= 0) {
				HealthAmount = 0;
				
				if (HealthAmount <= 0) {
					Die ();
					return;
				}
			}

           
		}

        public void Die () {
            AgentController.DestroyAgent(Agent);
            if (Agent.Animator.IsNotNull ()) {
                Agent.SetState(AnimState.Dying);
                Agent.Animator.Visualize();
            }
        }

        protected override void OnDeactivate() {
			OnTakeProjectile = null;
		}

		public event Action<LSProjectile> OnTakeProjectile;
		public int shieldIndex {get; set;}


    }
}