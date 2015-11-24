using UnityEngine;
using System.Collections;
using Lockstep.UI;
namespace Lockstep
{
	public class Shield : Ability
	{
		[SerializeField]
		Vector3 guiOffset = Vector3.up;
		[SerializeField]
		Vector2 guiScale = Vector2.one;
		[SerializeField, FixedNumber]
		private long _maxShield;
		public long MaxShield {get {return _maxShield;}}
		[SerializeField, FixedNumber (true)]
		private long _regeneration;
		public long Regeneration {get {return _regeneration;}}
		[SerializeField, FrameCount]
		private int _regenerationDelay;
		public int RegenerationDelay {get {return _regenerationDelay;}}
		public long ShieldAmount {get; private set;}

        FastBucket<Health> shieldedHealths = new FastBucket<Health>();
		private int timeUntilRegeneration;

		public bool IsShielding {get; private set;}
        
		protected override void OnSetup ()
		{
			Agent.onBuildChild += HandleonBuildChild;
		}


        protected override void OnInitialize ()
		{

			ShieldAmount = MaxShield;
            timeUntilRegeneration = 0;

			shieldedHealths.FastClear ();
		}

		protected override void OnSimulate ()
		{
			if (timeUntilRegeneration <= 0) {
				TakeRawDamage (-Regeneration);
			}
			else {
				timeUntilRegeneration--;
            }
		}

		protected override void OnDeactivate ()
		{
			shieldedHealths.Enumerate (LSUtility.bufferHealths);
			for (int i = 0; i < LSUtility.bufferHealths.Count; i++){
				RemoveHealth (LSUtility.bufferHealths[i]);
			}
		}
		public void CoverHealth (Health health) {
			health.shieldIndex = shieldedHealths.Add (health);
			health.OnTakeProjectile += OnTakeDamage;
			health.Agent.onDeactivation += OnCoveredDie;
			health.Protect (this);
		}
		void HandleonBuildChild (LSAgent obj)
		{
			if (obj.Healther .IsNotNull ())
				CoverHealth (obj.Healther);
		}
		private void OnCoveredDie (LSAgent agent) {
			RemoveHealth (agent.Healther);
			shieldedHealths.Enumerate (LSUtility.bufferHealths);
			bool hasAlive = false;
			for (int i = 0; i < LSUtility.bufferHealths.Count; i++) {
				Health health = LSUtility.bufferHealths[i];
				if (health.HealthAmount > 0) {
					hasAlive = true;
					break;
				}
			}
			if (hasAlive) {

			}
			else {
				Agent.Die ();
			}
		}
		public void RemoveHealth (Health health) {
			shieldedHealths.RemoveAt (health.shieldIndex);
			health.OnTakeProjectile -= OnTakeDamage;
			health.Agent.onDeactivation -= OnCoveredDie;
			health.Unprotect (this);
		}

		void OnTakeDamage (LSProjectile projectile) {
			if (IsShielding) {
				TakeRawDamage ( projectile.CheckExclusiveDamage (Agent.Tag));
				projectile.Damage = 0;
			}
			timeUntilRegeneration = RegenerationDelay;

		}


		private void TakeRawDamage (long damage) {
			ShieldAmount -= damage;
			if (ShieldAmount < 0) ShieldAmount = 0;
			else if (ShieldAmount > MaxShield) {
				ShieldAmount = MaxShield;
			}
			IsShielding = ShieldAmount > 0;

			Agent.StatsBarer.SetFill (StatType.Shield,(float)(ShieldAmount / (double)MaxShield));
        }
	}
}