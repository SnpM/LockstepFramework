using System;
using UnityEngine;
using Lockstep.Data;

namespace Lockstep
{
	[UnityEngine.DisallowMultipleComponent]
	public class Scan : ActiveAbility
	{
		private const int SearchRate = (int)(LockstepManager.FrameRate / 2);
		public const long MissModifier = FixedMath.One / 2;

		public virtual bool CanMove { get; private set; }

		protected bool CanTurn { get; private set; }

		public LSAgent Target { get; private set; }

		public virtual string ProjCode { get { return _projectileCode; } }

		public virtual long Range { get { return _range + RangeModifier; } }

		public long BaseRange { get { return _range; } }
		//Range

		[Lockstep(true)]
		public long RangeModifier { get; set; }

		public virtual long Sight { get { return _sight; } }
		//Approximate radius that's scanned for targets

		public virtual long Damage { get { return _damage; } }
		//Damage of attack

		public long BaseDamage { get { return _damage; } }


		public virtual long AttackInterval { get { return _attackInterval; } }
		//Frames between each attack

		public virtual bool TrackAttackAngle { get { return _trackAttackAngle; } }
		//Whether or not to require the unit to face the target for attacking

		public long AttackAngle { get { return _attackAngle; } }
		//The angle in front of the unit that the target must be located in

		protected virtual AllegianceType TargetAllegiance
		{ //Allegiance to the target
			get { return this._targetAllegiance; }
		}


		public virtual Vector3d ProjectileOffset { get { return _projectileOffset; } }

		private Vector3d[] cachedProjectileOffsets;

		public virtual Vector3d[] ProjectileOffsets
		{
			get
			{
				if (cachedProjectileOffsets == null)
				{
					cachedProjectileOffsets = new Vector3d[this._secondaryProjectileOffsets.Length + 1];
					cachedProjectileOffsets[0] = this.ProjectileOffset;
					for (int i = 0; i < this._secondaryProjectileOffsets.Length; i++)
					{
						cachedProjectileOffsets[i + 1] = this._secondaryProjectileOffsets[i];
					}
				}
				return cachedProjectileOffsets;
			}
		}
		public bool CycleProjectiles { get { return this._cycleProjectiles; } }
		//Offset of projectile

		#region Serialized Values (Further description in properties)

		[SerializeField, DataCode("Projectiles")]
		protected string _projectileCode;
		[FixedNumber, SerializeField]
		protected long _range = FixedMath.One * 6;
		[FixedNumber, SerializeField]
		protected long _sight = FixedMath.One * 10;
		[FixedNumber, SerializeField]
		protected long _damage = FixedMath.One;
		[SerializeField, FixedNumber]
		protected long _attackInterval = 1 * FixedMath.One;
		[SerializeField, EnumMask]
		protected AllegianceType _targetAllegiance = AllegianceType.Enemy;

		[SerializeField]
		protected bool _trackAttackAngle = true;
		[FixedNumberAngle, SerializeField]
		protected long _attackAngle = FixedMath.TenDegrees;
		[SerializeField]
		protected Vector3d _projectileOffset;
		[SerializeField]
		protected Vector3d[] _secondaryProjectileOffsets;
		[SerializeField]
		private bool _cycleProjectiles;
		[SerializeField, FixedNumber]
		protected long _windup;

		#endregion

		public long Windup { get { return _windup; } }

		[SerializeField]
		protected bool _increasePriority = true;

		public virtual bool IncreasePriority { get { return _increasePriority; } }


		//Stuff for the logic
		private bool inRange;
		//private long fastRange;
		private long fastRangeToTarget;
		private Vector2d Destination;
		private Move cachedMove;
		private Turn cachedTurn;

		protected LSBody cachedBody { get { return Agent.Body; } }

		private int basePriority;
		private Health cachedTargetHealth;
		private uint targetVersion;
		private int searchCount;
		private long attackCount;
		private bool isAttackMoving;
		private bool isFocused;

		protected override void OnSetup()
		{
			cachedTurn = Agent.GetAbility<Turn>();
			cachedMove = Agent.GetAbility<Move>();
			if (Sight < Range)
				_sight = Range + FixedMath.One * 5;

			//fastRange = (Range * Range);
			basePriority = cachedBody.Priority;
			CanMove = cachedMove.IsNotNull();
			if (CanMove)
			{
				cachedMove.onArrive += HandleOnArrive;
				cachedMove.onGroupProcessed += _HandleMoveGroupProcessed;
			}

			CanTurn = cachedTurn.IsNotNull();
			CachedAgentValid = this.AgentValid;
		}

		private void HandleOnArrive()
		{
			if (this.isAttackMoving)
			{
				if (Target == null)
					isAttackMoving = false;
			}
		}

		#region variables for quick fix for repathing to target's new position
		const long repathDistance = FixedMath.One * 2;
		FrameTimer repathTimer = new FrameTimer();
		const int repathInterval = LockstepManager.FrameRate * 2;
		int repathRandom;
		#endregion
		protected override void OnInitialize()
		{
			basePriority = Agent.Body.Priority;
			searchCount = LSUtility.GetRandom(SearchRate) + 1;
			attackCount = 0;
			Target = null;
			isAttackMoving = false;
			inRange = false;
			isFocused = false;
			CycleCount = 0;
			this.Destination = Vector2d.zero;
			repathTimer.Reset (repathInterval);
			repathRandom = LSUtility.GetRandom (repathInterval);

			//caching parameters
			var spawnVersion = Agent.SpawnVersion;
			var controller = Agent.Controller;
			CachedOnHit = (target) => OnHit (target, spawnVersion,controller);
		}

		protected override void OnSimulate()
		{
			if (attackCount > 0) {
				attackCount -= LockstepManager.DeltaTime;
			}
			if (Target != null)
			{
				BehaveWithTarget();
			}
			else {
				BehaveWithNoTarget();
			}

			if (CanMove) {
				if (Target != null || isAttackMoving) {
					cachedMove.PauseAutoStop ();
				}
			}
		}

		[Lockstep(true)]
		public bool IsWindingUp { get; set; }

		long windupCount;

		void StartWindup()
		{
			windupCount = this.Windup;
			IsWindingUp = true;
			Agent.ApplyImpulse(this.FireAnimImpulse);
			OnStartWindup();
		}

		protected virtual void OnStartWindup()
		{

		}

		protected virtual AnimState EngagingAnimState
		{
			get { return AnimState.Engaging; }
		}

		protected virtual AnimImpulse FireAnimImpulse
		{
			get { return AnimImpulse.Fire; }
		}

		bool CheckRange () {
			Vector2d targetDirection = Target.Body._position - cachedBody._position;
			long fastMag = targetDirection.FastMagnitude ();

			return fastMag <= fastRangeToTarget;
		}
		void BehaveWithTarget()
		{
			if (Target.IsActive == false || Target.SpawnVersion != targetVersion ||
				(this.TargetAllegiance & Agent.GetAllegiance(Target)) == 0)
			{
				//Target no longer exists
				StopEngage();
				BehaveWithNoTarget();
				return;
			}

			if (!IsWindingUp) {
				Vector2d targetDirection = Target.Body._position - cachedBody._position;
				long fastMag = targetDirection.FastMagnitude ();

				if (CheckRange()) {
					if (!inRange) {
						if (CanMove)
							cachedMove.StopMove ();
						inRange = true;
					}
					Agent.SetState (EngagingAnimState);

					long mag;
					targetDirection.Normalize (out mag);
					bool withinTurn = TrackAttackAngle == false ||
					                  (fastMag != 0 &&
					                  cachedBody.Forward.Dot (targetDirection.x, targetDirection.y) > 0
					                  && cachedBody.Forward.Cross (targetDirection.x, targetDirection.y).Abs () <= AttackAngle);
					bool needTurn = mag != 0 && !withinTurn;
					if (needTurn) {
						if (CanTurn) {
							cachedTurn.StartTurnDirection (targetDirection);
						} else {

						}
					} else {
						if (attackCount <= 0) {
							StartWindup ();
						}
					}

				} else {
					if (CanMove) {
						if (cachedMove.IsMoving == false) {
							cachedMove.StartMove (Target.Body._position);
							cachedBody.Priority = basePriority;
						} else {
							if (inRange) {
								cachedMove.Destination = Target.Body.Position;
							} else {
								if (repathTimer.AdvanceFrame ()) {
									if (Target.Body.PositionChangedBuffer &&
									    Target.Body.Position.FastDistance (cachedMove.Destination.x, cachedMove.Destination.y) >= (repathDistance * repathDistance)) {
										cachedMove.StartMove (Target.Body._position);
										//So units don't sync up and path on the same frame
										repathTimer.AdvanceFrames (repathRandom);
									}
								}
							}
						}
					}

					if (isAttackMoving || isFocused == false) {
						searchCount -= 1;
						if (searchCount <= 0) {
							searchCount = SearchRate;
							if (ScanAndEngage ()) {
							} else {
							}
						}
					}
					if (inRange == true) {
						inRange = false;
					}

				}
			}

			if (IsWindingUp)
			{
				windupCount -= LockstepManager.DeltaTime;
				if (windupCount < 0)
				{
					if (this.AgentConditional(Target))
					{
						Fire();
						int counter = 0;
						while (this.attackCount <= 0) {
							this.attackCount += (this.AttackInterval);
							counter++;
							if (counter > 1)
								Debug.Log ("asdf" + this.attackCount.ToDouble());
						}
						this.attackCount -= Windup;
					}
					else {
						StopEngage();
						this.ScanAndEngage();
					}
					IsWindingUp = false;

				}
			}
		}

		void BehaveWithNoTarget()
		{
			if (isAttackMoving || Agent.IsCasting == false)
			{
				if (isAttackMoving)
				{
					{
						searchCount -= 8;
					}
				}
				else {
					searchCount -= 2;
				}
				if (searchCount <= 0)
				{
					searchCount = SearchRate;
					if (ScanAndEngage())
					{
					}
				}
			}
		}

		public event Action<LSAgent,bool> ExtraOnHit;
		protected void CallExtraOnHit(LSAgent agent, bool isCurrent)
		{
			if (ExtraOnHit != null)
				ExtraOnHit(agent, isCurrent);
		}

		protected virtual void OnHit(LSAgent target, uint agentVersion, AgentController controller)
		{
			//If the shooter died, certain effects or records can't be completed
			bool isCurrent = Agent != null && agentVersion == Agent.SpawnVersion;
			Health healther = target.GetAbility<Health>();
			AttackerInfo info = new AttackerInfo (isCurrent ? Agent : null, controller);
			healther.TakeDamage(Damage, info);
			CallExtraOnHit(target,isCurrent);
		}

		private Action<LSAgent> CachedOnHit;

		public void Fire()
		{

			if (CanMove)
			{
				cachedMove.StopMove();
			}
			cachedBody.Priority = IncreasePriority ? basePriority + 1 : basePriority;

			OnFire(Target);

		}

		/// <summary>
		/// The projectile to be fired in OnFire.
		/// </summary>
		/// <value>The current projectile.</value>

		public int CycleCount { get; private set; }
		protected virtual void OnFire(LSAgent target)
		{
			if (this.CycleProjectiles)
			{
				CycleCount++;
				if (CycleCount >= ProjectileOffsets.Length)
				{
					CycleCount = 0;
				}
				FullFireProjectile(this.ProjCode,ProjectileOffsets[CycleCount], target);


			}
			else {
				for (int i = 0; i < ProjectileOffsets.Length; i++)
				{
					FullFireProjectile(ProjCode,ProjectileOffsets[i], target);

				}
			}

		}

		public LSProjectile FullFireProjectile(string projectileCode, Vector3d projOffset, LSAgent target)
		{
			LSProjectile proj = (PrepareProjectile(projectileCode, projOffset, target));
			FireProjectile(proj);
			return proj;
		}

		public static Scan LastFire;
		public LSProjectile PrepareProjectile(string projectileCode, Vector3d projOffset, LSAgent target)
		{
			LastFire = this;
			LSProjectile currentProjectile = ProjectileManager.Create(
				projectileCode,
				this.Agent,
				projOffset,
				this.TargetAllegiance,
				(other) =>
				{
					Health healther = other.GetAbility<Health>();
					return healther.IsNotNull() && healther.HealthAmount > 0;

				},
				CachedOnHit);

			switch (currentProjectile.TargetingBehavior)
			{
				case TargetingType.Homing:
					currentProjectile.InitializeHoming(target);
					break;
				case TargetingType.Timed:
					currentProjectile.InitializeTimed(Agent.Body.Forward);
					break;
				case TargetingType.Positional:
					currentProjectile.InitializePositional(target.Body.Position.ToVector3d(target.Body.HeightPos));
					break;
				case TargetingType.Directional:
					//TODO
					throw new System.Exception("Not implemented yet.");
					//break;
			}
			OnPrepareProjectile(currentProjectile);
			return currentProjectile;
		}
		public LSProjectile PrepareProjectile(string projectileCode, Vector3d projOffset, Vector3d targetPos)
		{
			LSProjectile currentProjectile = ProjectileManager.Create(
				projectileCode,
				this.Agent,
				projOffset,
				this.TargetAllegiance,
				(other) =>
				{
					Health healther = other.GetAbility<Health>();
					return healther.IsNotNull() && healther.HealthAmount > 0;

				},
				CachedOnHit);

			switch (currentProjectile.TargetingBehavior)
			{
				case TargetingType.Timed:
					currentProjectile.InitializeTimed(Agent.Body.Forward);
					break;
				case TargetingType.Positional:
					currentProjectile.InitializePositional(targetPos);
					break;
				case TargetingType.Directional:
					//TODO
					throw new System.Exception("Not implemented yet.");
					//break;
			}

			return currentProjectile;
		}
		protected virtual void OnPrepareProjectile(LSProjectile projectile)
		{

		}
		public void FireProjectile(LSProjectile projectile)
		{
			ProjectileManager.Fire(projectile);
		}

		public void Engage(LSAgent other)
		{
			if (other != Agent && other != null)
			{
				cachedTargetHealth = other.GetAbility<Health>();
				if (cachedTargetHealth.IsNotNull())
				{
					OnEngage(other);
					Target = other;
					targetVersion = Target.SpawnVersion;
					IsCasting = true;
					fastRangeToTarget = Range + (Target.Body.IsNotNull() ? Target.Body.Radius : 0) + Agent.Body.Radius;
					fastRangeToTarget *= fastRangeToTarget;

					if (!CheckRange ()) {
						cachedMove.StartMove (Target.Body.Position);
					}
				}
			}
		}
		protected virtual void OnEngage(LSAgent target)
		{

		}

		public void StopEngage(bool complete = false)
		{
			isFocused = false;
			if (complete)
			{
				isAttackMoving = false;
			}
			else {
				if (isAttackMoving)
				{
					if (ScanAndEngage() == false)
					{
						cachedMove.StartMove(this.Destination);
					}
					else {
					}
				}
				else {
					if (CanMove)
					{
						if (Target != null && inRange == false)
						{
							cachedMove.StopMove();
						}
					}
				}
			}

			Target = null;
			cachedBody.Priority = basePriority;

			IsCasting = false;
		}

		protected override void OnDeactivate()
		{
			StopEngage(true);
		}

		public void StartAttackMove(Vector2d position, bool isFormal = true)
		{
			Agent.StopCast(this.ID);

			//if formal (going through normal Execute routes), do the group stuff
			if (isFormal)
			{
				if (Target != null)
				{
					cachedMove.RegisterGroup(false);
				}
				else {
					cachedMove.RegisterGroup();
				}
			}
			else {
				if (Target == null)
					cachedMove.StartMove(position);
			}
			isAttackMoving = true;
			isFocused = false;
		}
		protected override void OnExecute(Command com)
		{
			Vector2d pos;
			DefaultData target;
			if (com.TryGetData<Vector2d>(out pos) && CanMove)
			{
				StartAttackMove(pos);
			}
			else if (com.TryGetData<DefaultData>(out target) && target.Is(DataType.UShort))
			{
				isFocused = true;
				isAttackMoving = false;
				LSAgent tempTarget;
				ushort targetValue = (ushort)target.Value;
				if (AgentController.TryGetAgentInstance(targetValue, out tempTarget))
					Engage(tempTarget);
				else
					Debug.Log("nope");
			}


		}

		protected sealed override void OnStopCast()
		{
			StopEngage(true);
		}

		Action _handleMoveGroupProcessed;

		Action _HandleMoveGroupProcessed { get { return _handleMoveGroupProcessed ?? (_handleMoveGroupProcessed = HandleMoveGroupProcessed); } }

		void HandleMoveGroupProcessed()
		{
			this.Destination = cachedMove.Destination;
		}

		private bool ScanAndEngage()
		{
			LSAgent agent = DoScan();
			if (agent == null || agent == Target)
			{
				return false;
			}
			else {
				Engage(agent);
				return true;
			}
		}

		protected virtual bool AgentValid(LSAgent agent)
		{
			return true;
		}

		public Func<LSAgent, bool> CachedAgentValid { get; private set;}

		protected Func<LSAgent, bool> AgentConditional
		{
			get
			{
				Func<LSAgent, bool> agentConditional = null;

				if (this.Damage >= 0)
				{
					agentConditional = (other) =>
					{
						Health health = other.GetAbility<Health>();
						return Agent.GlobalID != other.GlobalID && health != null && health.CanLose && CachedAgentValid(other);
					};
				}
				else {
					agentConditional = (other) =>
					{
						Health health = other.GetAbility<Health>();
						return Agent.GlobalID != other.GlobalID && health != null && health.CanGain && CachedAgentValid(other);
					};
				}
				return agentConditional;
			}
		}
		protected virtual bool HardAgentConditional()
		{
			Health health = Target.GetAbility<Health>();
			if (health != null)
			{
				if (this.Damage >= 0)
				{
					return health.CanLose;
				}
				else {
					Debug.Log("asdf");
					return health.CanGain;
				}
			}
			return true;
		}

		protected virtual LSAgent DoScan()
		{
			Func<LSAgent, bool> agentConditional = AgentConditional;

			LSAgent agent = InfluenceManager.Scan(
										 this.cachedBody.Position,
										 this.Sight,
										 agentConditional,
										 (bite) =>
										 {
											 return ((this.Agent.Controller.GetAllegiance(bite) & this.TargetAllegiance) != 0);
										 }
									 );

			return agent;
		}

		public bool ScanWithinRangeAndEngage()
		{
			LSAgent agent = this.DoScan();
			if (agent == null)
			{
				return false;
			}
			else {
				Engage(agent);
				return true;
			}
		}

#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if (Agent == null || Agent.IsActive == false) return;
			if (Agent.Body == null)
				Debug.Log (Agent.gameObject);
			Gizmos.DrawWireSphere(Application.isPlaying ? Agent.Body._visualPosition : this.transform.position, this.Range.ToFloat());
		}
#endif
	}
}